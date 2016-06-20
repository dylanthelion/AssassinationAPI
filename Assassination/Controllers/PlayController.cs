using Assassination.Helpers;
using Assassination.Models;
using Assassination.WebsocketHandlers;
using Microsoft.Web.WebSockets;
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
    public class PlayController : ApiController
    {
        AssassinationContext db = new AssassinationContext();

        [HttpGet]
        public HttpResponseMessage GetLiveCharacters(int gameID, int playerID, string password)
        {
            RequestValidators validator = new RequestValidators();
            Tuple<bool, HttpResponseMessage> inGameValidator = validator.ValidateIfInActiveGame(playerID, password, gameID);
            if (inGameValidator.Item1)
            {
                return inGameValidator.Item2;
            }

            JObject results = new JObject();

            List<String> Targets = (from check in db.AllPlayers
                                    join target in db.AllTargets on check.ID equals target.TargetID
                                    join pg in db.AllPlayerGames on target.PlayerGameID equals pg.ID
                                    where pg.GameID == gameID && pg.PlayerID == playerID && target.Killed == false
                                    select check.UserName).ToList();
            List<String> NonTargets = (from check in db.AllPlayers
                                       join pg in db.AllPlayerGames on check.ID equals pg.PlayerID
                                       where pg.Alive == true && pg.GameID == gameID && check.ID != playerID && !Targets.Contains(check.UserName)
                                       select check.UserName).ToList();
            results.Add("Targets", JArray.FromObject(Targets));
            results.Add("NonTargets", JArray.FromObject(NonTargets));

            return new HttpResponseMessage()
            {
                Content = new StringContent(results.ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        public HttpResponseMessage UpdateLocation([FromBody] Geocoordinate coords, int gameID, int playerID, string password)
        {
            RequestValidators validator = new RequestValidators();
            Tuple<bool, HttpResponseMessage> aliveValidator = validator.ValidateAliveInGame(playerID, password, gameID);
            if (aliveValidator.Item1)
            {
                return aliveValidator.Item2;
            }

            if(!ModelState.IsValid)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid location information" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            PlayerGame checkIfInGame = (from check in db.AllPlayerGames
                                        where check.PlayerID == playerID && check.GameID == gameID
                                        select check).FirstOrDefault();

            checkIfInGame.Latitude = coords.Latitude;
            checkIfInGame.Longitude = coords.Longitude;
            db.Entry(checkIfInGame).State = EntityState.Modified;
            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { "Updated!" }).ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpDelete]
        public HttpResponseMessage KillPlayer([FromBody] Geocoordinate coords, int gameID, int playerID, string password, string targetName)
        {
            RequestValidators validator = new RequestValidators();
            Tuple<bool, HttpResponseMessage> aliveValidator = validator.ValidateAliveInGame(playerID, password, gameID);
            if (aliveValidator.Item1)
            {
                return aliveValidator.Item2;
            }

            if (!ModelState.IsValid || coords == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid location information" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Player checkPlayer = db.AllPlayers.Find(playerID);

            Game checkGame = db.AllGames.Find(gameID);

            PlayerGame checkIfInGame = (from check in db.AllPlayerGames
                                        where check.PlayerID == playerID && check.GameID == gameID
                                        select check).FirstOrDefault();

            Target checkTarget = (from check in db.AllTargets
                                  join player in db.AllPlayers on check.TargetID equals player.ID
                                  where check.PlayerGameID == checkIfInGame.ID && player.UserName == targetName
                                  select check).FirstOrDefault();
            if (checkTarget == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That player is not a valid target" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Geocoordinate targetCoords = null;
            GameWebSocketHandler handler = null;

            if (checkGame.GameType == GameType.FreeForAll)
            {
                handler = new FreeForAllGameWebSocketHandler();
                if (!handler.CheckIfAlive(gameID, checkPlayer.UserName))
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(JArray.FromObject(new List<String>() { "YOU ARE DEAD" }).ToString(), Encoding.UTF8, "application/json")
                    };
                }
                Tuple<bool, Geocoordinate> locationResults = handler.GetPlayerLocation(gameID, null, targetName);
                if (locationResults.Item1)
                {
                    targetCoords = locationResults.Item2;
                }
            }
            else if (checkGame.GameType == GameType.IndividualTargets)
            {
                handler = new IndividualTargetsGameWebSocketHandler();
                if (!handler.CheckIfAlive(gameID, checkPlayer.UserName))
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(JArray.FromObject(new List<String>() { "YOU ARE DEAD" }).ToString(), Encoding.UTF8, "application/json")
                    };
                }
                Tuple<bool, Geocoordinate> locationResults = handler.GetPlayerLocation(gameID, null, targetName);
                if (locationResults.Item1)
                {
                    targetCoords = locationResults.Item2;
                }
            }
            else if (checkGame.GameType == GameType.Team)
            {
                handler = new TeamGameWebSocketHandler();
                if (!handler.CheckIfAlive(gameID, checkPlayer.UserName))
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(JArray.FromObject(new List<String>() { "YOU ARE DEAD" }).ToString(), Encoding.UTF8, "application/json")
                    };
                }
                string killerTeam = checkIfInGame.TeamName;
                string targetTeam = (from check in db.AllPlayerGames
                                     where check.PlayerID == checkTarget.TargetID && check.GameID == gameID
                                     select check.TeamName).FirstOrDefault();

                if (killerTeam == null || targetTeam == null)
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(JArray.FromObject(new List<String>() { "Something went wrong. Those players don't have a team assigned." }).ToString(), Encoding.UTF8, "application/json")
                    };
                }
                else if (killerTeam == targetTeam)
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(JArray.FromObject(new List<String>() { "You are on the same team!." }).ToString(), Encoding.UTF8, "application/json")
                    };
                }

                Tuple<bool, Geocoordinate> locationResults = handler.GetPlayerLocation(gameID, targetName, targetTeam);
                if (locationResults.Item1)
                {
                    targetCoords = locationResults.Item2;
                }
            }

            else
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Something went wrong. That game has not been set up." }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (targetCoords == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Something went wrong. That player is not in that game." }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (targetCoords.Altitude == 0 || coords.Altitude == 0.0)
            {
                targetCoords.Altitude = 0;
                coords.Altitude = 0;
            }

            if (targetCoords.Latitude == 0 && targetCoords.Longitude == 0 && targetCoords.Altitude == 0)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That player has not logged in yet." }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            float killDistance = (from check in db.AllAccounts
                                  where check.PlayerID == playerID
                                  select check.MaxKillRadiusInMeters).FirstOrDefault();
            double targetDistance = Math.Sqrt(Math.Pow((coords.Latitude - targetCoords.Latitude) * 111131.745, 2) + Math.Pow((coords.Longitude - targetCoords.Longitude) * 78846.805720, 2));
            if (targetDistance > killDistance || Math.Abs(targetCoords.Altitude - coords.Altitude) > killDistance)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You are too far away" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkGame.GameType == GameType.IndividualTargets)
            {
                Player nextTarget = (from check in db.AllPlayers
                                     join targetToDelete in db.AllTargets on check.ID equals targetToDelete.TargetID
                                     join oldTargetPG in db.AllPlayerGames on targetToDelete.PlayerGameID equals oldTargetPG.ID
                                     join deadPlayer in db.AllPlayers on oldTargetPG.PlayerID equals deadPlayer.ID
                                     where deadPlayer.UserName == targetName
                                     select check).FirstOrDefault();
                Target newTarget = new Target(checkIfInGame, nextTarget);
                db.AllTargets.Add(newTarget);
            }
            handler.KillPlayer(gameID, targetName);

            PlayerGame update = (from check in db.AllPlayerGames
                                 where check.GameID == gameID && check.PlayerID == checkTarget.TargetID
                                 select check).FirstOrDefault();
            List<Target> allTargets = (from check in db.AllTargets
                                       where check.TargetID == checkTarget.TargetID
                                       select check).ToList();
            update.Alive = false;
            db.Entry(update).State = EntityState.Modified;
            foreach (Target t in allTargets)
            {
                if (t.PlayerGameID != checkIfInGame.ID)
                {
                    db.AllTargets.Remove(t);
                    db.Entry(t).State = EntityState.Deleted;
                }
                else
                {
                    t.Killed = true;
                    db.Entry(t).State = EntityState.Modified;
                }
            }

            bool endGame = false;
            if (checkGame.GameType != GameType.Team)
            {
                int playersLeft = (from check in db.AllPlayerGames
                                   where check.GameID == gameID && check.Alive == true
                                   select check).ToList().Count;
                if (playersLeft < 2)
                {
                    endGame = true;
                }
            }
            else
            {
                endGame = true;
                List<PlayerGame> allPG = (from check in db.AllPlayerGames
                                          where check.GameID == gameID && check.Alive == true
                                          select check).ToList();
                foreach (PlayerGame pg in allPG)
                {
                    if (pg.TeamName != checkIfInGame.TeamName)
                    {
                        endGame = false;
                        break;
                    }
                }
            }

            /*if ((DateTime.Now - checkGame.StartTime).Minutes >= checkGame.GameLengthInMinutes)
            {
                endGame = true;
            }*/

            if (endGame)
            {
                new Archiver().ArchiveGame(gameID, db);
                
                
                db.SaveChanges();
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Game over!" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { "Kill!" }).ToString(), Encoding.UTF8, "application/json")
            };
        }
    }
}