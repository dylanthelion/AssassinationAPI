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
    public class GameController : ApiController
    {
        private AssassinationContext db = new AssassinationContext();

        [HttpPost]
        public HttpResponseMessage CreateGame([FromBody] Game game, int playerID, string password)
        {
            if (!ModelState.IsValid)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid game object" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

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

            Account accountDetails = (from check in db.AllAccounts
                                      where check.PlayerID == playerID
                                      select check).FirstOrDefault();

            if (accountDetails == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Something went wrong. You don't have an account set up." }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Game g = new Game(game.LocationDescription);

            Geocoordinate coord = null;

            if (game.Location != null)
            {
                coord = new Geocoordinate(game.Location.Latitude, game.Location.Longitude);
                db.AllGameCoords.Add(coord);
                g.Location = coord;
            }

            if (game.NumberOfPlayers == null)
            {
                g.NumberOfPlayers = 5;
            }
            else if (game.NumberOfPlayers > accountDetails.MaxPlayers)
            {
                g.NumberOfPlayers = accountDetails.MaxPlayers;
            } else {
                g.NumberOfPlayers = game.NumberOfPlayers;
            }

            if(game.RadiusInMeters == null)
            {
                game.RadiusInMeters = 500;
            }
            else if (game.RadiusInMeters > accountDetails.MaxRadiusInMeters)
            {
                g.RadiusInMeters = accountDetails.MaxRadiusInMeters;
            }
            else
            {
                g.RadiusInMeters = game.RadiusInMeters;
            }

            if (game.StartTime == null)
            {
                g.StartTime = DateTime.Now.AddDays(2.0);
            }
            else
            {
                g.StartTime = game.StartTime;
            }

            PlayerGame pg = new PlayerGame(checkPlayer, g);
            pg.IsModerator = true;

            db.AllGames.Add(g);
            db.AllPlayerGames.Add(pg);
            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { String.Format("Game created! ID: {0}", g.ID.ToString()) }).ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpPut]
        public HttpResponseMessage EditGame([FromBody] Game game, int playerID, string password)
        {
            if (!ModelState.IsValid)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid game object" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Player checkPlayer = db.AllPlayers.Find(playerID);
            Game checkGame = db.AllGames.Find(game.ID);

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

            Account accountDetails = (from check in db.AllAccounts
                                      where check.PlayerID == playerID
                                      select check).FirstOrDefault();

            if (accountDetails == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Something went wrong. You don't have an account set up." }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkGame == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid game ID" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Geocoordinate coord = null;

            if (game.Location != null)
            {
                coord = new Geocoordinate(game.Location.Latitude, game.Location.Longitude);
                db.AllGameCoords.Add(coord);
                db.AllGameCoords.Remove(checkGame.Location);
                db.Entry(checkGame.Location).State = EntityState.Deleted;
                checkGame.Location = coord;
            }

            if (game.NumberOfPlayers == null)
            {
            }
            else if (game.NumberOfPlayers > accountDetails.MaxPlayers)
            {
                checkGame.NumberOfPlayers = accountDetails.MaxPlayers;
            }
            else
            {
                checkGame.NumberOfPlayers = game.NumberOfPlayers;
            }

            if (game.RadiusInMeters == null)
            {
            }
            else if (game.RadiusInMeters > accountDetails.MaxRadiusInMeters)
            {
                checkGame.RadiusInMeters = accountDetails.MaxRadiusInMeters;
            }
            else
            {
                checkGame.RadiusInMeters = game.RadiusInMeters;
            }

            if (game.StartTime == null)
            {
            }
            else
            {
                checkGame.StartTime = game.StartTime;
            }

            db.Entry(checkGame).State = EntityState.Modified;
            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { String.Format("Game edited! ID: {0}", checkGame.ID.ToString()) }).ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpDelete]
        public HttpResponseMessage DeleteGame(int gameID, int playerID, string password)
        {
            Game checkGame = db.AllGames.Find(gameID);
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

            if (checkGame == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid game ID" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            bool checkPG = (from check in db.AllPlayerGames
                            where check.PlayerID == playerID && check.GameID == gameID
                            select check.IsModerator).FirstOrDefault();
            if (!checkPG)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You are not the moderator of that game" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            List<PlayerGame> allPG = (from check in db.AllPlayerGames
                                      where check.GameID == gameID
                                      select check).ToList();
            db.AllGames.Remove(checkGame);
            db.Entry(checkGame).State = EntityState.Deleted;

            foreach (PlayerGame pg in allPG)
            {
                db.AllPlayerGames.Remove(pg);
                db.Entry(pg).State = EntityState.Deleted;
            }

            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { "Deleted!" }).ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpGet]
        public HttpResponseMessage GetGame(int gameID)
        {
            Game checkGame = db.AllGames.Find(gameID);
            if (checkGame == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid game ID" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            JObject results = new JObject();
            string moderator = (from check in db.AllPlayers
                                      join playerGame in db.AllPlayerGames on check.ID equals playerGame.PlayerID
                                      where playerGame.GameID == gameID && playerGame.IsModerator == true
                                      select check.UserName).FirstOrDefault();
            int numberOfPlayers = (from check in db.AllPlayerGames
                                   where check.GameID == gameID
                                   select check).ToList().Count;
            int maxPlayers = checkGame.NumberOfPlayers;
            string location = checkGame.LocationDescription;
            if (checkGame.Location != null)
            {
                results.Add("Latitude", checkGame.Location.Latitude.ToString());
                results.Add("Longitude", checkGame.Location.Longitude.ToString());
            }
            results.Add("Moderator", moderator);
            results.Add("Location", location);
            results.Add("Joined", numberOfPlayers.ToString());
            results.Add("Needed", maxPlayers.ToString());

            return new HttpResponseMessage()
            {
                Content = new StringContent(results.ToString(), Encoding.UTF8, "application/json")
            };
        }
    }
}