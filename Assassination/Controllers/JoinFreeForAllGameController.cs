using Assassination.Models;
using Assassination.WebsocketHandlers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Microsoft.Web.WebSockets;
using Assassination.Helpers;

namespace Assassination.Controllers
{
    public class JoinFreeForAllGameController : ApiController
    {
        AssassinationContext db = new AssassinationContext();

        [HttpGet]
        public HttpResponseMessage JoinGame([FromBody] Geocoordinate location, int gameID, int playerID, string password)
        {

            Tuple<bool, HttpResponseMessage> validator = RequestValidators.ValidateAliveInGame(playerID, password, gameID);
            if (validator.Item1)
            {
                return validator.Item2;
            }
            //Player checkPlayer = db.AllPlayers.Find(playerID);

            /*if (checkPlayer == null)
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
            }*/

            Game checkGame = db.AllGames.Find(gameID);

            if (checkGame.GameType != GameType.FreeForAll)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That is not a free for all game" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            /*PlayerGame checkIfInGame = (from check in db.AllPlayerGames
                                        where check.PlayerID == playerID && check.GameID == gameID
                                        select check).FirstOrDefault();

            if (checkIfInGame == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You are not in that game" }).ToString(), Encoding.UTF8, "application/json")
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
            }*/

            FreeForAllGameWebSocketHandler handler = new FreeForAllGameWebSocketHandler();
            handler.gameID = gameID;
            handler.userName = (from check in db.AllPlayers
                                where check.ID == playerID
                                select check.UserName).FirstOrDefault();
            handler.setUpGroup();
            HttpContext.Current.AcceptWebSocketRequest(handler);
            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }
    }
}