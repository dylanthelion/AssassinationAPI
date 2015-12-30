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
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That game has not started" }).ToString(), Encoding.UTF8, "application/json")
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

            if (!checkIfInGame.Alive)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You are dead" }).ToString(), Encoding.UTF8, "application/json")
                };
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
            results.Add("Targets", JArray.FromObject(Targets.ToString()));
            results.Add("NonTargets", JArray.FromObject(NonTargets.ToString()));

            return new HttpResponseMessage()
            {
                Content = new StringContent(results.ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        public HttpResponseMessage UpdateLocation([FromBody] Geocoordinate coords, int gameID, int playerID, string password)
        {
            if(!ModelState.IsValid)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid location information" }).ToString(), Encoding.UTF8, "application/json")
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
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That game has not started" }).ToString(), Encoding.UTF8, "application/json")
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

            if (!checkIfInGame.Alive)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You are dead" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

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
            if (!ModelState.IsValid)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid location information" }).ToString(), Encoding.UTF8, "application/json")
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
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That game has not started" }).ToString(), Encoding.UTF8, "application/json")
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

            if (!checkIfInGame.Alive)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You are dead" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

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

            if (checkGame.GameType == GameType.FreeForAll)
            {
                FreeForAllGameWebSocketHandler handler = new FreeForAllGameWebSocketHandler();
                Tuple<bool, Geocoordinate> locationResults = handler.GetPlayerLocation(gameID, targetName);
                if (locationResults.Item1)
                {
                    targetCoords = locationResults.Item2;
                }
            }
            else if (checkGame.GameType == GameType.IndividualTargets)
            {
                IndividualTargetsGameWebSocketHandler handler = new IndividualTargetsGameWebSocketHandler();
                Tuple<bool, Geocoordinate> locationResults = handler.GetPlayerLocation(gameID, targetName);
                if (locationResults.Item1)
                {
                    targetCoords = locationResults.Item2;
                }
            }
            else if (checkGame.GameType == GameType.Team)
            {
                TeamGameWebSocketHandler handler = new TeamGameWebSocketHandler();
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

            var targetLocation = (from check in db.AllPlayerGames
                                 where check.GameID == gameID && check.PlayerID == checkTarget.TargetID
                                 select new { lat = check.Latitude, longi = check.Longitude, alt = check.Altitude }).FirstOrDefault();
            //Geocoordinate targetCoords = new Geocoordinate(targetLocation.lat, targetLocation.longi);
            if (targetCoords.Altitude == 0 || coords.Altitude == 0.0)
            {
                targetCoords.Altitude = 0;
                coords.Altitude = 0;
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

            if (endGame)
            {
                PlayerGame[] allPlayerGames = (from check in db.AllPlayerGames
                                                   where check.GameID == gameID
                                                   orderby check.PlayerID
                                                   select check).ToArray();
                List<Target> allKills = (from check in db.AllTargets
                                           join playerGame in db.AllPlayerGames on check.PlayerGameID equals playerGame.ID
                                           where playerGame.GameID == gameID
                                           select check).ToList();
                AccountArchive[] allAccounts = (from check in db.AllAccountArchives
                                                    join map in db.AppAccountArchiveMap on check.ID equals map.AccountArchiveID
                                                    join players in db.AllPlayers on map.PlayerID equals players.ID
                                                    join playerGames in db.AllPlayerGames on players.ID equals playerGames.PlayerID
                                                    where playerGames.GameID == gameID
                                                    orderby players.ID
                                                    select check).ToArray();
                Account[] stats = (from check in db.AllAccounts
                                  join players in db.AllPlayers on check.PlayerID equals players.ID
                                  join playerGames in db.AllPlayerGames on players.ID equals playerGames.PlayerID
                                                    where playerGames.GameID == gameID
                                                    orderby players.ID
                                                    select check).ToArray();
                GameArchive ga = new GameArchive();
                Geocoordinate gameLocation = (from check in db.AllGameCoords
                                              join games in db.AllGames on check.ID equals games.LocationID
                                              select check).FirstOrDefault();
                db.AllGameCoords.Remove(gameLocation);
                db.Entry(gameLocation).State = EntityState.Deleted;
                db.AllGames.Remove(checkGame);
                db.Entry(checkGame).State = EntityState.Deleted;
                ga.EndTime = DateTime.Now;
                db.AllGameArchives.Add(ga);

                for (int i = 0; i < allPlayerGames.Length; i++)
                {
                    PlayerGameArchive pga = new PlayerGameArchive(allAccounts[i], ga, allPlayerGames[i]);
                    db.AllPlayerGameArchives.Add(pga);
                    if (allPlayerGames[i].Alive)
                    {
                        stats[i].Experience += 3;
                    }
                    else
                    {
                        stats[i].Experience += 1;
                    }
                    foreach (Target t in allKills)
                    {
                        if (t.PlayerGameID == allPlayerGames[i].ID)
                        {
                            AccountArchive killed = (from check in db.AllAccountArchives
                                                    join map in db.AppAccountArchiveMap on check.ID equals map.AccountArchiveID
                                                    join allPlayers in db.AllPlayers on map.PlayerID equals allPlayers.ID
                                                    where allPlayers.ID == t.TargetID
                                                    select check).FirstOrDefault();
                            TargetArchive ta = new TargetArchive(pga, killed, t);
                            db.AllTargetArchives.Add(ta);
                            stats[i].Experience += 1;
                        }
                    }

                    db.AllPlayerGames.Remove(allPlayerGames[i]);
                    db.Entry(allPlayerGames[i]).State = EntityState.Deleted;
                    db.Entry(stats[i]).State = EntityState.Modified;
                }

                foreach (Target t in allKills)
                {
                    db.AllTargets.Remove(t);
                    db.Entry(t).State = EntityState.Deleted;
                }

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