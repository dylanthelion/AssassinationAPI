using Assassination.Helpers;
using Assassination.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace Assassination.Controllers
{
    public class ManageAccountController : ApiController
    {
        private AssassinationContext db = new AssassinationContext();

        [HttpGet]
        public HttpResponseMessage GetUserDataWithEmail(string email, string password)
        {
            Player checkPlayer = (from check in db.AllPlayers
                                  where check.Email == email
                                  select check).FirstOrDefault();

            if (checkPlayer == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid email address" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkPlayer.Password != password)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid password" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            RequestValidators validator = new RequestValidators();
            Tuple<bool, HttpResponseMessage> playerValidator = validator.ValidatePlayerWithEmail(email, password);
            if (playerValidator.Item1)
            {
                return playerValidator.Item2;
            }

            List<String> devices = (from check in db.AllDevices
                                    where check.PlayerID == checkPlayer.ID
                                    select check.UUID).ToList();

            JObject results = new JObject();
            results.Add("ID", checkPlayer.ID.ToString());
            results.Add("UserName", checkPlayer.UserName);
            results.Add("Devices", JArray.FromObject(devices).ToString());

            return new HttpResponseMessage()
            {
                Content = new StringContent(results.ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        public HttpResponseMessage ChangeEmail(int playerID, string userName, string password, string oldEmail, string newEmail)
        {
            RegexUtilities util = new RegexUtilities();
            if (!util.IsValidEmail(newEmail))
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "New email address is not formatted correctly" }).ToString(), Encoding.UTF8, "application/json")
                };
            }
            Player checkPlayer = db.AllPlayers.Find(playerID);

            RequestValidators validator = new RequestValidators();
            Tuple<bool, HttpResponseMessage> accountValidator = validator.ValidatePlayerForChange(playerID, userName, oldEmail, password);
            if (accountValidator.Item1)
            {
                return accountValidator.Item2;
            }

            

            EmailAddressAttribute attr = new EmailAddressAttribute();
            bool isValid = attr.IsValid(newEmail);

            if (!isValid)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "New email is invalid" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            checkPlayer.Email = newEmail;
            db.Entry(checkPlayer).State = EntityState.Modified;
            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { "Changed!" }).ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpPut]
        public HttpResponseMessage ChangePassword(int playerID, string userName, string email, string oldPassword, string newPassword)
        {
            if(newPassword.Length < 10)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Password must be at least 10 characters" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            RequestValidators validator = new RequestValidators();
            Tuple<bool, HttpResponseMessage> accountValidator = validator.ValidatePlayerForChange(playerID, userName, email, oldPassword);
            if (accountValidator.Item1)
            {
                return accountValidator.Item2;
            }


            Player checkPlayer = db.AllPlayers.Find(playerID);

            

            checkPlayer.Password = newPassword;
            db.Entry(checkPlayer).State = EntityState.Modified;
            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { "Changed!" }).ToString(), Encoding.UTF8, "application/json")
            };
        }
    }
}