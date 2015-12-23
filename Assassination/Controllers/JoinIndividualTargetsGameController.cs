using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.Web.WebSockets;
using Assassination.Models;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using Assassination.WebsocketHandlers;
using System.Net;

namespace Assassination.Controllers
{
    public class JoinIndividualTargetsGameController : ApiController
    {
        AssassinationContext db = new AssassinationContext();

        [HttpGet]
        public HttpResponseMessage JoinGame([FromBody] Geocoordinate location, int gameID, int playerID, string password)
        {
            Player checkPlayer = db.AllPlayers.Find(playerID);

            if (checkPlayer == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid player ID" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkPlayer.Password != password)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid password" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Game checkGame = db.AllGames.Find(gameID);

            if (checkGame == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid game ID" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (!checkGame.IsActiveGame)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That game has not started, yet" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            PlayerGame checkIfInGame = (from check in db.AllPlayerGames
                                        where check.PlayerID == playerID && check.GameID == gameID
                                        select check).FirstOrDefault();

            if (checkIfInGame == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You are not in that game" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            String checkTarget = (from check in db.AllTargets
                                 join playerGames in db.AllPlayerGames on check.PlayerGameID equals playerGames.ID
                                 join players in db.AllPlayers on check.TargetID equals players.ID
                                 where playerGames.GameID == gameID && playerGames.PlayerID == playerID
                                 select players.UserName).FirstOrDefault();

            if(checkTarget == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That game has not been set up yet." }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            IndividualTargetsGameWebSocketHandler handler = new IndividualTargetsGameWebSocketHandler();
            handler.gameID = gameID;
            handler.playerName = checkPlayer.UserName;
            handler.targetName = checkTarget;
            handler.setUpGroup();
            HttpContext.Current.AcceptWebSocketRequest(handler);
            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }
    }
}