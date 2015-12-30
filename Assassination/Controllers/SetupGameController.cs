﻿using Assassination.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Assassination.Helpers;
using System.Data.Entity;

namespace Assassination.Controllers
{
    public class SetupGameController : ApiController
    {
        private AssassinationContext db = new AssassinationContext();

        [HttpPost]
        public HttpResponseMessage SetupGame([FromBody] List<string> teams, int gameID, int playerID, string password)
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

            if (checkGame.GameType == GameType.Default)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Please choose a game type(Team, Free For All, etc...), before setting up the game." }).ToString(), Encoding.UTF8, "application/json")
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

            Account checkAccount = (from check in db.AllAccounts
                                    where check.PlayerID == playerID
                                    select check).FirstOrDefault();
            if (teams.Count > checkAccount.MaxTeams)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { String.Format("You cannot declare more than {0} teams", checkAccount.MaxTeams.ToString()) }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            PlayerGame[] players = (from check in db.AllPlayerGames
                                    where check.GameID == gameID
                                    select check).ToArray();

            if (teams.Count == 0)
            {
                if (checkGame.GameType == GameType.IndividualTargets)
                {
                    PlayerGame[] playersCopy = new PlayerGame[players.Length];
                    players.CopyTo(playersCopy, 0);
                    bool ready = false;
                    while (!ready)
                    {
                        ready = true;
                        new Random().Shuffle(playersCopy);
                        for (int i = 0; i < players.Length; i++)
                        {
                            if (players[i].PlayerID == playersCopy[i].PlayerID)
                            {
                                ready = false;
                            }
                        }
                    }

                    for (int i = 0; i < players.Length; i++)
                    {
                        Player targetPlayer = db.AllPlayers.Find(playersCopy[i].PlayerID);
                        Target t = new Target(players[i], targetPlayer);
                        db.AllTargets.Add(t);
                    }
                }
                else if (checkGame.GameType == GameType.FreeForAll)
                {
                    PlayerGame[] playersCopy = new PlayerGame[players.Length];
                    players.CopyTo(playersCopy, 0);

                    for(int i = 0; i < players.Length; i++)
                    {
                        for (int j = 0; j < players.Length; j++)
                        {
                            if (i != j)
                            {
                                Player targetPlayer = db.AllPlayers.Find(playersCopy[j].PlayerID);
                                Target t = new Target(players[i], targetPlayer);
                                db.AllTargets.Add(t);
                            }
                        }
                    }
                }
                else
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(JArray.FromObject(new List<String>() { "Game type does not match number of teams chosen. Please choose relevant team names, or change the game type." }).ToString(), Encoding.UTF8, "application/json")
                    };
                }
            }
            else
            {
                if (checkGame.GameType != GameType.Team)
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(JArray.FromObject(new List<String>() { "Game type does not match number of teams chosen. Please  change the game type." }).ToString(), Encoding.UTF8, "application/json")
                    };
                }

                string currentTeam = teams[0];
                
                new Random().Shuffle(players);
                for (int i = 0; i < players.Length; i++)
                {
                    players[i].TeamName = teams.Next(currentTeam);
                    players[i].Alive = true;
                    currentTeam = teams.Next(currentTeam);
                    db.Entry(players[i]).State = EntityState.Modified;
                }
                foreach (PlayerGame killer in players)
                {
                    foreach (PlayerGame target in players)
                    {
                        if (killer.TeamName != target.TeamName)
                        {
                            Player targetPlayer = db.AllPlayers.Find(target.PlayerID);
                            Target t = new Target(killer, targetPlayer);
                            db.AllTargets.Add(t);
                        }
                    }
                }
                checkGame.GameType = GameType.Team;
                db.Entry(checkGame).State = EntityState.Modified;
            }

            checkGame.IsActiveGame = true;
            checkGame.StartTime = DateTime.Now;
            db.Entry(checkGame).State = EntityState.Modified;
            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { "Targets set up! Get going!" }).ToString(), Encoding.UTF8, "application/json")
            };
        }
    }
}