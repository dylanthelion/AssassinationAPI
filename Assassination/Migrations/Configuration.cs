namespace Assassination.Migrations
{
    using Assassination.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Assassination.Models.AssassinationContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Assassination.Models.AssassinationContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            if (context.AllPlayers.ToList().Count < 50)
            {
                List<Player> newPlayers = new List<Player>();
                List<Account> newAccounts = new List<Account>();
                List<AccountArchive> newArchives = new List<AccountArchive>();
                List<AccountArchiveMap> map = new List<AccountArchiveMap>();
                List<Device> newDevices = new List<Device>();

                for (int i = 1; i < 101; i++)
                {
                    Player p = new Player();
                    p.Email = String.Format("test{0}@test{1}.com", i.ToString(), i.ToString());
                    p.UserName = String.Format("Test{0}", i.ToString());
                    p.Password = String.Format("test{0}test{1}", i.ToString(), i.ToString());
                    Account a = new Account(p);
                    a.Experience = 0;
                    a.MaxGamesPerWeek = 10;
                    a.MaxKillRadiusInMeters = 10;
                    a.MaxPlayers = 20;
                    a.MaxRadiusInMeters = 1500;
                    a.MaxTeams = 4;
                    AccountArchive aa = new AccountArchive(p);
                    AccountArchiveMap aam = new AccountArchiveMap(p, aa);
                    Device d = new Device(p, String.Format("Test{0}", i.ToString()));
                    newPlayers.Add(p);
                    newAccounts.Add(a);
                    newArchives.Add(aa);
                    map.Add(aam);
                    newDevices.Add(d);
                }

                foreach (Player p in newPlayers)
                {
                    context.AllPlayers.Add(p);
                }

                foreach (Account a in newAccounts)
                {
                    context.AllAccounts.Add(a);
                }

                foreach (AccountArchive aa in newArchives)
                {
                    context.AllAccountArchives.Add(aa);
                }

                foreach (AccountArchiveMap aam in map)
                {
                    context.AppAccountArchiveMap.Add(aam);
                }

                foreach (Device d in newDevices)
                {
                    context.AllDevices.Add(d);
                }
            }

            foreach (Account a in context.AllAccounts)
            {
                a.MaxGameLengthInMinutes = 45;
                a.MaxGamesPerWeek = 3;
                a.MaxKillRadiusInMeters = 10;
                a.MaxPlayers = 5;
                a.MaxRadiusInMeters = 1500;
                a.MaxTeams = 2;
                context.Entry(a).State = EntityState.Modified;
            }

            foreach (Player p in context.AllPlayers)
            {
                if (p.Password == "")
                {
                    p.Password = "test1test1";
                    context.Entry(p).State = EntityState.Modified;
                }
            }

            context.SaveChanges();
        }
    }
}
