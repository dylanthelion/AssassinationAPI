using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.WebSockets;
using Microsoft.Web.WebSockets;
using System.Diagnostics;

namespace Assassination.WebsocketHandlers
{
    public class FreeForAllGameWebSocketHandler : WebSocketHandler
     {
         public int gameID { get; set; }
         private static Dictionary<int, WebSocketCollection> clients = new Dictionary<int, WebSocketCollection>();
         private static Dictionary<int, Dictionary<int, double[]>> locations = new Dictionary<int, Dictionary<int, double[]>>();
         public string userName { get; set; }
 
         public void setUpGroup()
         {
             if (!clients.ContainsKey(gameID))
             {
                 clients[gameID] = new WebSocketCollection();
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

             foreach(String s in data)
             {
                 double n;
                 if(!double.TryParse(s, out n))
                 {
                     return;
                 }
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
                     return;
                 }

                 if(!locations[int.Parse(data[0])].ContainsKey(int.Parse(data[1])))
                 {
                     return;
                 }

                 locations[int.Parse(data[0])][int.Parse(data[1])][0] = double.Parse(data[2]);
                 locations[int.Parse(data[0])][int.Parse(data[1])][1] = double.Parse(data[3]);

                 clients[int.Parse(data[0])].Broadcast(locations[int.Parse(data[0])].ToString());
             }
       }
    }
}