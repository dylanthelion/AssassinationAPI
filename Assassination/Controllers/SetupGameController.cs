using Assassination.Models;
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
                checkGame.GameType = GameType.IndividualTargets;
                db.Entry(checkGame).State = EntityState.Modified;
            }
            else
            {
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