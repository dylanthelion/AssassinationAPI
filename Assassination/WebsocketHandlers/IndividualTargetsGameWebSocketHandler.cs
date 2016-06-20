using Assassination.Models;
using Microsoft.Web.WebSockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;

namespace Assassination.WebsocketHandlers
{
    public class IndividualTargetsGameWebSocketHandler : GameWebSocketHandler
    {
        private ReaderWriterLockSlim messageLock = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim targetLock = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim locationLock = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim killLock = new ReaderWriterLockSlim();
        public int gameID { get; set; }
        public string playerName { get; set; }
        public string targetName { get; set; }
        public static Dictionary<int, List<string>> DeadPlayers { get; set; }
        // <gameid<playername<targetname, clients>>>
        private static Dictionary<int, Dictionary<string, double[]>> locations = new Dictionary<int, Dictionary<string, double[]>>();
        private static Dictionary<int, Dictionary<string, Dictionary<string, WebSocketCollection>>> targets = new Dictionary<int, Dictionary<string, Dictionary<string, WebSocketCollection>>>();

        public override void SetUpGroup()
        {
            if (DeadPlayers == null)
            {
                DeadPlayers = new Dictionary<int, List<string>>();
            }
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
                    Debug.WriteLine("Parse fail: " + message + " Data: " + data[0] + data[1]);
                    return;
                }

                if (!targets.ContainsKey(int.Parse(data[0])))
                {
                    this.Close();
                    return;
                }
                else
                {
                    messageLock.EnterReadLock();
                    try
                    {
                        foreach (KeyValuePair<string, Dictionary<string, WebSocketCollection>> outer in targets[gameID])
                        {
                            foreach (KeyValuePair<string, WebSocketCollection> inner in outer.Value)
                            {
                                Debug.WriteLine("Full: " + message);
                                foreach (string output in data)
                                {
                                    Debug.WriteLine("Data: " + output);
                                }
                                inner.Value.Broadcast(data[1]);
                            }
                        }
                    }
                    finally
                    {
                        messageLock.ExitReadLock();
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
                    Debug.WriteLine("Parse fail: " + message + " Data: " + data[0] + data[1] + data[2] + data[3]);
                    return;
                }

                if (!targets.ContainsKey(int.Parse(data[0])))
                {
                    this.Close();
                    return;
                }

                if (!targets[int.Parse(data[0])].ContainsKey(data[1]))
                {
                    foreach (KeyValuePair<string, Dictionary<string, WebSocketCollection>> entry in targets[int.Parse(data[0])])
                    {
                        Debug.WriteLine("Entry: " + entry.Key);
                    }
                    Debug.WriteLine("Failed to find: " + data[1]);
                    this.Close();
                    return;
                }

                locationLock.EnterWriteLock();
                try
                {
                    foreach (KeyValuePair<int, Dictionary<string, double[]>> entry in locations)
                    {
                        if (!locations.ContainsKey(game))
                        {
                            Debug.WriteLine("Failed to find: " + data[1]);
                            this.Close();
                            return;
                        }
                        double[] toSet = new double[3];
                        toSet[0] = lat;
                        toSet[1] = longi;
                        toSet[2] = 0.0;
                        locations[game][data[1]] = toSet;
                    }
                }

                finally
                {
                    locationLock.ExitWriteLock();
                }

                targetLock.EnterReadLock();
                try
                {
                    foreach (KeyValuePair<string, Dictionary<string, WebSocketCollection>> outer in targets[gameID])
                    {
                        foreach (KeyValuePair<string, WebSocketCollection> inner in outer.Value)
                        {
                            if (inner.Key.ToString() == data[1])
                            {
                                inner.Value.Broadcast(message);
                            }
                            else
                            {
                                Debug.WriteLine("Key " + inner.Key.ToString() + " does not match " + data[1]);
                            }
                        }
                    }
                }
                finally
                {
                    targetLock.ExitReadLock();
                }
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            targetLock.EnterWriteLock();
            try
            {
                targets[gameID].Remove(playerName);
                if (targets[gameID].Count < 1)
                {
                    targets.Remove(gameID);
                }
            }
            finally
            {
                targetLock.ExitWriteLock();
            }
        }

        public override Tuple<bool, Geocoordinate> GetPlayerLocation(int game, string teamName, string player)
        {
            locationLock.EnterReadLock();
            try
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
            finally
            {
                locationLock.ExitReadLock();
            }
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
            locationLock.EnterWriteLock();
            killLock.EnterWriteLock();
            try
            {
                locations[gameID].Remove(playerName);

                if (!DeadPlayers.ContainsKey(gameID))
                {
                    DeadPlayers[gameID] = new List<string>();
                }

                DeadPlayers[gameID].Add(playerName);
            }
            finally
            {
                locationLock.ExitWriteLock();
                killLock.ExitWriteLock();
            }

            targetLock.EnterWriteLock();
            string killerName = "";
            string newTargetName = "";
            try
            {
                foreach (KeyValuePair<string, Dictionary<string, WebSocketCollection>> oldTarget in targets[gameID])
                {
                    if (oldTarget.Key == playerName)
                    {
                        newTargetName = oldTarget.Value.Keys.ToArray()[0];
                    }
                    else if (oldTarget.Value.ContainsKey(playerName))
                    {
                        killerName = oldTarget.Key;
                    }
                }
                if (killerName != "" && newTargetName != "")
                {
                    targets[gameID][killerName] = null;
                    targets[gameID][killerName] = new Dictionary<string, WebSocketCollection>();
                    targets[gameID][killerName][newTargetName] = new WebSocketCollection();
                }

            }
            finally
            {
                targetLock.ExitWriteLock();
            }
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