using Assassination.Models;
using Microsoft.Web.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assassination.WebsocketHandlers
{
    public abstract class GameWebSocketHandler : WebSocketHandler
    {
        public abstract Tuple<bool, Geocoordinate> GetPlayerLocation(int game, string teamName, string playerName);

        public abstract void SetUpGroup();

        public override void OnClose()
        {
            base.OnClose();
        }

        public abstract void KillPlayer(int gameID, string playerName);

        public abstract bool CheckIfAlive(int gameID, string playerName);
    }
}