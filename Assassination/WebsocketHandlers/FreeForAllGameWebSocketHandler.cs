using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.WebSockets;
using Microsoft.Web.WebSockets;
using System.Diagnostics;
using Assassination.Models;

namespace Assassination.WebsocketHandlers
{
    public class FreeForAllGameWebSocketHandler : GameWebSocketHandler
     {
         public int gameID { get; set; }
         private static Dictionary<int, WebSocketCollection> clients = new Dictionary<int, WebSocketCollection>();
        // gameID, playername
         private static Dictionary<int, Dictionary<string, double[]>> locations = new Dictionary<int, Dictionary<string, double[]>>();
         public string userName { get; set; }
 
         public override void SetUpGroup()
         {
             if (!clients.ContainsKey(gameID))
             {
                 clients[gameID] = new WebSocketCollection();
             }

             if(!locations.ContainsKey(gameID))
             {
                 locations[gameID] = new Dictionary<string, double[]>();
             }

             if (!locations[gameID].ContainsKey(userName))
             {
                 locations[gameID][userName] = new double[3];
                 locations[gameID][userName][2] = 0;
             }
         }
 
         public override void OnOpen()
         {
             clients[gameID].Add(this);
         }
 
         public override void OnMessage(string message)
         {
             char[] delimiters = { ',' };
             String[] data = message.Split(delimiters);

             if(data.Length != 2 && data.Length != 4)
             {
                 Debug.WriteLine("Message problem: " + message);
                 return;
             }

             if(data.Length == 2)
             {
                 int n;
                 if(!int.TryParse(data[0], out n))
                 {
                     return;
                 }

                 if(!clients.ContainsKey(int.Parse(data[0])))
                 {
                     this.Close();
                     return;
                 } else {
                     clients[int.Parse(data[0])].Broadcast(data[1]);
                 }
             }

             else {
                 int game;
                 int player;
                 double lat;
                 double longi;
                 if(!int.TryParse(data[0], out game) || !int.TryParse(data[1], out player) || !double.TryParse(data[2], out lat) || !double.TryParse(data[3], out longi))
                 {
                     return;
                 }

                 if(!clients.ContainsKey(int.Parse(data[0])) || !locations.ContainsKey(int.Parse(data[0])))
                 {
                     this.Close();
                     return;
                 }

                 if(!locations[int.Parse(data[0])].ContainsKey(data[1]))
                 {
                     this.Close();
                     return;
                 }

                 locations[int.Parse(data[0])][data[1]][0] = double.Parse(data[2]);
                 locations[int.Parse(data[0])][data[1]][1] = double.Parse(data[3]);

                 clients[int.Parse(data[0])].Broadcast(locations[int.Parse(data[0])].ToString());
             }
       }

         public override void OnClose()
         {
             base.OnClose();
             clients[gameID].Remove(this);
             if (clients[gameID].Count < 1)
             {
                 clients.Remove(gameID);
             }
         }

         public override Tuple<bool, Geocoordinate> GetPlayerLocation(int game, string teamName, string playerName)
         {
             if(locations.ContainsKey(game))
             {
                 if(locations[game].ContainsKey(playerName))
                 {
                     if(locations[game][playerName][2] == 0)
                     {
                         return new Tuple<bool,Geocoordinate>(true, new Geocoordinate(Convert.ToSingle(locations[game][playerName][0]), Convert.ToSingle(locations[game][playerName][1])));
                     }
                     else if (locations[game][playerName][2] > 0)
                     {
                         return new Tuple<bool, Geocoordinate>(true, new Geocoordinate(Convert.ToSingle(locations[game][playerName][0]), Convert.ToSingle(locations[game][playerName][1]), Convert.ToSingle(locations[game][playerName][2])));
                     }
                 }
             }

             return new Tuple<bool, Geocoordinate>(false, new Geocoordinate());
         }

         public override void KillPlayer(int gameID, string playerName)
         {
             if (!locations.ContainsKey(gameID))
             {
                 return;
             }

             if (!locations[gameID].ContainsKey(playerName))
             {
                 return;
             }

             locations[gameID].Remove(playerName);
         }

         public override bool CheckIfAlive(int gameID, string playerName)
         {
             if (!locations.ContainsKey(gameID))
             {
                 return false;
             }

             if (!locations[gameID].ContainsKey(playerName))
             {
                 return false;
             }

             return true;
         }
    }
}