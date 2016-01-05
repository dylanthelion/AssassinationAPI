using Assassination.Models;
using Microsoft.Web.WebSockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Assassination.WebsocketHandlers
{
    public class IndividualTargetsGameWebSocketHandler : GameWebSocketHandler
    {
        public int gameID { get; set; }
        public string playerName { get; set; }
        public string targetName { get; set; }
        public static Dictionary<int, List<string>> DeadPlayers { get; set; }
        // <gameid<playername<targetname, clients>>>
        private static Dictionary<int, Dictionary<string, double[]>> locations = new Dictionary<int, Dictionary<string, double[]>>();
        private static Dictionary<int, Dictionary<string, Dictionary<string, WebSocketCollection>>> targets = new Dictionary<int, Dictionary<string, Dictionary<string, WebSocketCollection>>>();

        public override void SetUpGroup()
        {
            if (!targets.ContainsKey(gameID))
            {
                targets[gameID] = new Dictionary<string, Dictionary<string, WebSocketCollection>>();
            }

            if (!targets[gameID].ContainsKey(playerName))
            {
                targets[gameID][playerName] = new Dictionary<string, WebSocketCollection>();
            }

            if(!targets[gameID][playerName].ContainsKey(targetName))
            {
                targets[gameID][playerName][targetName] = new WebSocketCollection();
            }

            if (!locations.ContainsKey(gameID))
            {
                locations[gameID] = new Dictionary<string, double[]>();
            }

            if (!DeadPlayers.ContainsKey(gameID))
            {
                DeadPlayers[gameID] = new List<string>();
            }

            if (DeadPlayers[gameID].Contains(playerName))
            {
                return;
            }

            if (!locations[gameID].ContainsKey(playerName))
            {
                locations[gameID][playerName] = new double[3];
                locations[gameID][playerName][2] = 0;
            }
        }

        public override void OnOpen()
        {
            targets[gameID][playerName][targetName].Add(this);
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
                    foreach (KeyValuePair<string, Dictionary<string, WebSocketCollection>> outer in targets[gameID])
                    {
                        foreach (KeyValuePair<string, WebSocketCollection> inner in outer.Value)
                        {
                            inner.Value.Broadcast(data[1]);
                        }
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

                if (!targets[int.Parse(data[0])].ContainsKey(data[1]))
                {
                    this.Close();
                    return;
                }

                foreach (KeyValuePair<string, Dictionary<string, WebSocketCollection>> outer in targets[gameID])
                {
                    foreach (KeyValuePair<string, WebSocketCollection> inner in outer.Value)
                    {
                        if (inner.Key.ToString() == data[1])
                        {
                            inner.Value.Broadcast(data.ToString());
                        }
                    }
                }
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            targets[gameID].Remove(playerName);
            if (targets[gameID].Count < 1)
            {
                targets.Remove(gameID);
            }
        }

        public override Tuple<bool, Geocoordinate> GetPlayerLocation(int game, string teamName, string player)
        {
            if (locations.ContainsKey(game))
            {
                if (locations[game].ContainsKey(player))
                {
                    if (locations[game][player][2] == 0)
                    {
                        return new Tuple<bool, Geocoordinate>(true, new Geocoordinate(Convert.ToSingle(locations[game][player][0]), Convert.ToSingle(locations[game][player][1])));
                    }
                    else if (locations[game][player][2] > 0)
                    {
                        return new Tuple<bool, Geocoordinate>(true, new Geocoordinate(Convert.ToSingle(locations[game][player][0]), Convert.ToSingle(locations[game][player][1]), Convert.ToSingle(locations[game][player][2])));
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

            if (!locations[gameID].ContainsKey(playerName))
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

            return true;
        }
    }
}