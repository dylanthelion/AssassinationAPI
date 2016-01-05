using Assassination.Models;
using Microsoft.Web.WebSockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Assassination.WebsocketHandlers
{
    public class TeamGameWebSocketHandler : GameWebSocketHandler
    {
        public int gameID { get; set; }
        public string playerName { get; set; }
        public string teamName { get; set; }
        public static Dictionary<int, List<string>> DeadPlayers { get; set; }
        // <gameid<teamname, clients>>>
        private static Dictionary<int, Dictionary<string, WebSocketCollection>> targets = new Dictionary<int, Dictionary<string, WebSocketCollection>>();
        // <gameid<teamname<playername, location>>>
        private static Dictionary<int, Dictionary<string, Dictionary<string, double[]>>> locations = new Dictionary<int, Dictionary<string, Dictionary<string, double[]>>>();

        public override void SetUpGroup()
        {
            if (!targets.ContainsKey(gameID))
            {
                targets[gameID] = new Dictionary<string, WebSocketCollection>();
            }

            if (!targets[gameID].ContainsKey(teamName))
            {
                targets[gameID][teamName] = new WebSocketCollection();
            }

            if (!locations[gameID].ContainsKey(teamName))
            {
                locations[gameID] = new Dictionary<string, Dictionary<string, double[]>>();
            }

            if (!DeadPlayers.ContainsKey(gameID))
            {
                DeadPlayers[gameID] = new List<string>();
            }

            if (DeadPlayers[gameID].Contains(playerName))
            {
                return;
            }

            if (!locations[gameID][teamName].ContainsKey(playerName))
            {
                locations[gameID][teamName][playerName] = new double[2];
            }
        }

        public override void OnOpen()
        {
            targets[gameID][teamName].Add(this);
        }

        public override void OnMessage(string message)
        {
            char[] delimiters = { ',' };
            String[] data = message.Split(delimiters);

            if (data.Length != 2 && data.Length != 4)
            {
                Debug.WriteLine("Message problem: " + message);
                return;
            }

            if (data.Length == 2)
            {
                int n;
                if (!int.TryParse(data[0], out n))
                {
                    return;
                }

                if (!targets.ContainsKey(int.Parse(data[0])))
                {
                    this.Close();
                    return;
                }
                else
                {
                    foreach (KeyValuePair<string, WebSocketCollection> outer in targets[gameID])
                    {
                        outer.Value.Broadcast(data[1]);
                    }
                }
            }

            else
            {
                int game;
                double lat;
                double longi;
                if (!int.TryParse(data[0], out game) || !double.TryParse(data[2], out lat) || !double.TryParse(data[3], out longi))
                {
                    return;
                }

                if (!targets.ContainsKey(int.Parse(data[0])))
                {
                    this.Close();
                    return;
                }

                bool inGame = false;
                string team = "";
                foreach(KeyValuePair<string, Dictionary<string, double[]>> outer in locations[int.Parse(data[0])])
                {
                    if(outer.Value.ContainsKey(data[1]))
                    {
                        inGame = true;
                        team = outer.Key;
                        break;
                    }
                }

                if (!inGame)
                {
                    this.Close();
                    return;
                }

                foreach (KeyValuePair<string, WebSocketCollection> entry in targets[int.Parse(data[0])])
                {
                    if (entry.Key != team)
                    {
                        entry.Value.Broadcast(data.ToString());
                    }
                }
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            locations[gameID][teamName].Remove(playerName);
            if (locations[gameID][teamName].Count < 1)
            {
                locations[gameID].Remove(teamName);
            }
            if (locations[gameID].Count < 1)
            {
                locations.Remove(gameID);
                targets.Remove(gameID);
            }
        }

        public override Tuple<bool, Geocoordinate> GetPlayerLocation(int game, string player, string team)
        {
            if (locations.ContainsKey(game))
            {
                if (locations[game].ContainsKey(team))
                {
                    if (locations[game][team].ContainsKey(player))
                    {
                        if (locations[game][team][player][2] == 0)
                        {
                            return new Tuple<bool, Geocoordinate>(true, new Geocoordinate(Convert.ToSingle(locations[game][team][player][0]), Convert.ToSingle(locations[game][team][player][1])));
                        }
                        else if (locations[game][team][player][2] > 0)
                        {
                            return new Tuple<bool, Geocoordinate>(true, new Geocoordinate(Convert.ToSingle(locations[game][team][player][0]), Convert.ToSingle(locations[game][team][player][1]), Convert.ToSingle(locations[game][team][player][2])));
                        }
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

            foreach(KeyValuePair<string, Dictionary<string, double[]>> entry in locations[gameID])
            {
                if (entry.Value.ContainsKey(playerName))
                {
                    entry.Value.Remove(playerName);
                }
            }

            if (!DeadPlayers.ContainsKey(gameID))
            {
                DeadPlayers[gameID] = new List<string>();
            }

            DeadPlayers[gameID].Add(playerName);
        }

        public override bool CheckIfAlive(int gameID, string playerName)
        {
            if (!locations.ContainsKey(gameID))
            {
                return false;
            }

            if (!DeadPlayers.ContainsKey(gameID))
            {
                return false;
            }

            if (DeadPlayers[gameID].Contains(playerName))
            {
                return false;
            }

            foreach (KeyValuePair<string, Dictionary<string, double[]>> entry in locations[gameID])
            {
                if (entry.Value.ContainsKey(playerName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}