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
        public HttpResponseMessage JoinGame(int gameID, int playerID, string password)
        {
            RequestValidators validator = new RequestValidators();

            Tuple<bool, HttpResponseMessage> aliveValidator = validator.ValidateAliveInGame(playerID, password, gameID);
            if (aliveValidator.Item1)
            {
                return aliveValidator.Item2;
            }
            

            Game checkGame = db.AllGames.Find(gameID);

            if (checkGame.GameType != GameType.FreeForAll)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That is not a free for all game" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            

            FreeForAllGameWebSocketHandler handler = new FreeForAllGameWebSocketHandler();
            handler.gameID = gameID;
            handler.userName = (from check in db.AllPlayers
                                where check.ID == playerID
                                select check.UserName).FirstOrDefault();
            handler.SetUpGroup();
            HttpContext.Current.AcceptWebSocketRequest(handler);
            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }
    }
}