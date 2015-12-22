using Assassination.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace Assassination.Controllers
{
    public class RallyController : ApiController
    {
        private AssassinationContext db = new AssassinationContext();

        [HttpGet]
        public HttpResponseMessage AllGames()
        {
            List<Game> allGames = db.AllGames.ToList();
            if (allGames.Count == 0)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "No games currently. Create one?" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(allGames).ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpPut]
        public HttpResponseMessage JoinGame(int gameID, int playerID, string password)
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

            if (checkGame.IsActiveGame == true)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That game has already started" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            PlayerGame pg = new PlayerGame(checkPlayer, checkGame);
            db.AllPlayerGames.Add(pg);
            db.SaveChanges();


                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Joined!" }).ToString(), Encoding.UTF8, "application/json")
                };
        }

        [HttpDelete]
        public HttpResponseMessage LeaveGame(int gameID, int playerID, string password)
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

            if (checkGame.IsActiveGame == false)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That game has already started" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            PlayerGame checkPlayerGame = (from check in db.AllPlayerGames
                                          where check.PlayerID == playerID && check.GameID == gameID
                                          select check).FirstOrDefault();

            if (checkPlayerGame == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You are not in that game" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkPlayerGame.IsModerator)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "The moderator cannot leave. Please cancel the game, instead." }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            db.AllPlayerGames.Remove(checkPlayerGame);
            db.Entry(checkPlayerGame).State = EntityState.Deleted;
            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { "Removed!" }).ToString(), Encoding.UTF8, "application/json")
            };
        }
    }
}