using Microsoft.Web.WebSockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Assassination.WebsocketHandlers
{
    public class IndividualTargetsGameWebSocketHandler : WebSocketHandler
    {
        public int gameID { get; set; }
        public string playerName { get; set; }
        public string targetName { get; set; }
        private static Dictionary<int, Dictionary<string, Dictionary<string, WebSocketCollection>>> targets = new Dictionary<int, Dictionary<string, Dictionary<string, WebSocketCollection>>>();
        private static Dictionary<int, Dictionary<int, double[]>> locations = new Dictionary<int, Dictionary<int, double[]>>();
        public string userName { get; set; }

        public void setUpGroup()
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
                    return;
                }

                if (!targets[int.Parse(data[0])].ContainsKey(data[1]))
                {
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
    }
}