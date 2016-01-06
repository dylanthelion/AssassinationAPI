using Assassination.Helpers;
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

            RequestValidators validator = new RequestValidators();

            Tuple<bool, HttpResponseMessage> playerValidator = validator.ValidateNewPlayer(player.UserName, player.Email, UUID);

            if (playerValidator.Item1)
            {
                return playerValidator.Item2;
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

            RequestValidators validator = new RequestValidators();

            Tuple<bool, HttpResponseMessage> playerValidator = validator.ValidatePlayerInformation(id, player.Password);

            if (playerValidator.Item1)
            {
                return playerValidator.Item2;
            }

            if (checkPlayer.UserName != player.UserName)
            {
                Tuple<bool, HttpResponseMessage> nameValidator = validator.CheckForUniqueName(player.UserName);
                if (nameValidator.Item1)
                {
                    return nameValidator.Item2;
                }
                else
                {
                    checkPlayer.UserName = player.UserName;
                    db.Entry(checkPlayer).State = EntityState.Modified;
                }
            }

            if (checkPlayer.Email != player.Email)
            {
                Tuple<bool, HttpResponseMessage> emailValidator = validator.CheckForUniqueEmail(player.Email);
                if (emailValidator.Item1)
                {
                    return emailValidator.Item2;
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
            RequestValidators validator = new RequestValidators();

            Tuple<bool, HttpResponseMessage> playerValidator = validator.ValidatePlayerInformationWithEmail(playerID, password, email);
            if (playerValidator.Item1)
            {
                return playerValidator.Item2;
            }


            Player checkPlayer = db.AllPlayers.Find(playerID);

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
    }
}