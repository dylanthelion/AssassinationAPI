using Assassination.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.Web.WebSockets;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using Assassination.WebsocketHandlers;
using System.Net;

namespace Assassination.Controllers
{
    public class JoinTeamGameController : ApiController
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

            if (checkGame.GameType != GameType.Team)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That is not a team game" }).ToString(), Encoding.UTF8, "application/json")
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

            if (checkIfInGame.TeamName == null || checkIfInGame.TeamName.Length == 0)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Teams are not yet set up for that game." }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (!checkIfInGame.Alive)
            {
                if (checkGame.GameType != GameType.Team)
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(JArray.FromObject(new List<String>() { "YOU ARE DEAD" }).ToString(), Encoding.UTF8, "application/json")
                    };
                }
            }

            TeamGameWebSocketHandler handler = new TeamGameWebSocketHandler();
            handler.gameID = gameID;
            handler.playerName = checkPlayer.UserName;
            handler.teamName = checkIfInGame.TeamName;
            handler.setUpGroup();
            HttpContext.Current.AcceptWebSocketRequest(handler);
            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }
    }
}