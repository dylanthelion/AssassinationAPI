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
using System.Web.Mvc;

namespace Assassination.Controllers
{
    public class AccountController : ApiController
    {
        private AssassinationContext db = new AssassinationContext();

        [System.Web.Http.HttpPost]
        public HttpResponseMessage CreateUser(string UUID, [FromBody] Player player)
        {
            if (!ModelState.IsValid)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<string>() { "Invalid player object. Make sure password is at least 10 characters long, and that email address is valid, and that player name is present." }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Player checkForUniqueName = (from check in db.AllPlayers
                                         where check.UserName == player.UserName
                                         select check).FirstOrDefault();
            if (checkForUniqueName != null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Username already taken" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Player checkForUniqueEmail = (from check in db.AllPlayers
                                         where check.Email == player.Email
                                         select check).FirstOrDefault();
            if (checkForUniqueEmail != null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Email address already taken" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Device checkForUniqueDevice = (from check in db.AllDevices
                                           where check.UUID == UUID
                                           select check).FirstOrDefault();
            if (checkForUniqueDevice != null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Device already has an account attached to it" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            

            Player p = new Player(player.UserName, player.Email, player.Password);
            Account a = new Account(p);
            Device d = new Device(p, UUID);
            AccountArchive aa = new AccountArchive(p);
            AccountArchiveMap map = new AccountArchiveMap(p, aa);

            db.AllPlayers.Add(p);
            db.AllAccounts.Add(a);
            db.AllAccountArchives.Add(aa);
            db.AppAccountArchiveMap.Add(map);
            db.AllDevices.Add(d);

            db.SaveChanges();

            db.Entry(p).GetDatabaseValues();

                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { String.Format("User ID: {0}", p.ID.ToString())  }).ToString(), Encoding.UTF8, "application/json")
                };
        }

        [System.Web.Http.HttpPut]
        public HttpResponseMessage EditUser([FromBody] Player player, int id)
        {
            if (!ModelState.IsValid)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid user information" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Debug.WriteLine("ID: " + id.ToString());
            Debug.WriteLine("Name: " + player.UserName);

            Player checkPlayer = db.AllPlayers.Find(id);
            if (checkPlayer == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid user ID" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkPlayer.Password != player.Password)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid password" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkPlayer.UserName != player.UserName)
            {
                Player checkForUniqueName = (from check in db.AllPlayers
                                             where check.UserName == player.UserName
                                             select check).FirstOrDefault();
                if (checkForUniqueName != null)
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(JArray.FromObject(new List<String>() { "Username already taken" }).ToString(), Encoding.UTF8, "application/json")
                    };
                }
                else
                {
                    checkPlayer.UserName = player.UserName;
                    db.Entry(checkPlayer).State = EntityState.Modified;
                }
            }

            if (checkPlayer.Email != player.Email)
            {
                Player checkForUniqueEmail = (from check in db.AllPlayers
                                              where check.Email == player.Email
                                              select check).FirstOrDefault();
                if (checkForUniqueEmail != null)
                {
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent(JArray.FromObject(new List<String>() { "Email address already taken" }).ToString(), Encoding.UTF8, "application/json")
                    };
                }
                else
                {
                    checkPlayer.Email = player.Email;
                    db.Entry(checkPlayer).State = EntityState.Modified;
                }
            }

            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { "Changed!" }).ToString(), Encoding.UTF8, "application/json")
            };
        }

        [System.Web.Http.HttpDelete]
        public HttpResponseMessage DeletePlayer(int playerID, string email, string password)
        {
            Player checkPlayer = db.AllPlayers.Find(playerID);

            if (checkPlayer == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid user ID" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkPlayer.Email != email)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid email" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkPlayer.Password != password)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid pasword" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            Account a = (from check in db.AllAccounts
                         where check.PlayerID == playerID
                         select check).FirstOrDefault();
            List<Device> devices = (from check in db.AllDevices
                                    where check.PlayerID == playerID
                                    select check).ToList();

            db.AllPlayers.Remove(checkPlayer);
            db.Entry(checkPlayer).State = EntityState.Deleted;
            db.AllAccounts.Remove(a);
            db.Entry(a).State = EntityState.Deleted;
            foreach (Device d in devices)
            {
                db.AllDevices.Remove(d);
                db.Entry(d).State = EntityState.Deleted;
            }

            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { "Deleted!" }).ToString(), Encoding.UTF8, "application/json")
            };
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage AllPlayers()
        {
            List<Player> allPlayers = db.AllPlayers.ToList();
            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(allPlayers).ToString(), Encoding.UTF8, "application/json")
            };
        }

        /*[System.Web.Http.HttpGet]
        public HttpResponseMessage GetPlayer(int id)
        {
            Player checkPlayer = db.AllPlayers.Find(id);

            if (checkPlayer == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid user ID" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<Player>() { checkPlayer }).ToString(), Encoding.UTF8, "application/json")
            };
        }*/
    }
}