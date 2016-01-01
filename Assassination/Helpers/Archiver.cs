using Assassination.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Assassination.Helpers
{
    public class Archiver
    {
        public void ArchiveGame(int gameID, AssassinationContext db)
        {
            Game checkGame = db.AllGames.Find(gameID);
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
        }
    }
}