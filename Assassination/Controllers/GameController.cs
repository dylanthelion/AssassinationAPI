using Assassination.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
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
            DateTime oneWeekAgo = DateTime.Now.AddDays(-7);

            int gamesThisWeek = (from archive in db.AllGameArchives
                                 join pga in db.AllPlayerGameArchives on archive.ID equals pga.GameID
                                 join pa in db.AllAccountArchives on pga.PlayerID equals pa.ID
                                 join map in db.AppAccountArchiveMap on pa.ID equals map.AccountArchiveID
                                 join player in db.AllPlayers on map.PlayerID equals player.ID
                                 where player.ID == playerID && DateTime.Compare(archive.EndTime, oneWeekAgo) > 0
                                 select pga).ToList().Count;

            int currentGames = (from check in db.AllPlayerGames
                                where check.PlayerID == playerID
                                select check).ToList().Count;

            gamesThisWeek += currentGames;

            Debug.WriteLine("Games this week: " + gamesThisWeek.ToString());
            Debug.WriteLine("Max Games: " + accountDetails.MaxGamesPerWeek.ToString());

            if (gamesThisWeek >= accountDetails.MaxGamesPerWeek)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { String.Format("You are only allowed {0} games per week", accountDetails.MaxGamesPerWeek.ToString()) }).ToString(), Encoding.UTF8, "application/json")
                };
            }

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

            if (game.NumberOfPlayers == 0)
            {
                g.NumberOfPlayers = 5;
            }
            else if (game.NumberOfPlayers > accountDetails.MaxPlayers)
            {
                g.NumberOfPlayers = accountDetails.MaxPlayers;
            } else {
                g.NumberOfPlayers = game.NumberOfPlayers;
            }

            if(game.RadiusInMeters == 0)
            {
                g.RadiusInMeters = 500;
            }
            else if (game.RadiusInMeters > accountDetails.MaxRadiusInMeters)
            {
                g.RadiusInMeters = accountDetails.MaxRadiusInMeters;
            }
            else
            {
                g.RadiusInMeters = game.RadiusInMeters;
            }

            if(game.GameLengthInMinutes == 0)
            {
                g.GameLengthInMinutes = 45;
            }
            else if (game.GameLengthInMinutes > accountDetails.MaxGameLengthInMinutes)
            {
                g.GameLengthInMinutes = accountDetails.MaxGameLengthInMinutes;
            }
            else
            {
                g.GameLengthInMinutes = game.GameLengthInMinutes;
            }

            if (game.GameType == 0)
            {
                g.GameType = GameType.IndividualTargets;
            }
            else {
                g.GameType = game.GameType;
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

            db.Entry(g).GetDatabaseValues();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { String.Format("Game created! ID: {0}", g.ID.ToString()) }).ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpPut]
        public HttpResponseMessage EditGame([FromBody] Game game, int playerID, string password, int gameID)
        {
            if (!ModelState.IsValid)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid game object" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Player checkPlayer = db.AllPlayers.Find(playerID);
            Debug.WriteLine("ID: " + gameID);
            Game checkGame = db.AllGames.Find(gameID);

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
                Geocoordinate loc = (from check in db.AllGameCoords
                                     join games in db.AllGames on check.ID equals games.LocationID
                                     select check).FirstOrDefault();
                if (loc != null)
                {
                    db.AllGameCoords.Remove(loc);
                    db.Entry(loc).State = EntityState.Deleted;
                }
                checkGame.Location = null;
                checkGame.LocationID = coord.ID;
                checkGame.Location = coord;
            }

            if (game.NumberOfPlayers == 0)
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

            if (game.RadiusInMeters == 0)
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

            if (game.GameType != 0)
            {
                checkGame.GameType = game.GameType;
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
            if (checkGame.GameType != 0)
            {
                results.Add("Game Type", checkGame.GameType.ToString());
            }
            if (checkGame.Location != null)
            {
                results.Add("Latitude", checkGame.Location.Latitude.ToString());
                results.Add("Longitude", checkGame.Location.Longitude.ToString());
            }

            if(checkGame.StartTime != null)
            {
                results.Add("Start Time", checkGame.StartTime.ToShortTimeString());
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