using Assassination.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace Assassination.Helpers
{
    public static class RequestValidators
    {
        public static AssassinationContext db = new AssassinationContext();

        public static Tuple<bool, HttpResponseMessage> ValidateNewPlayer(string userName, string email, string UUID)
        {
            /*Player checkForUniqueName = (from check in db.AllPlayers
                                         where check.UserName == userName
                                         select check).FirstOrDefault();
            if (checkForUniqueName != null)
            {
                return new Tuple<bool,HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Username already taken" }).ToString(), Encoding.UTF8, "application/json")
                });
            }*/

            Tuple<bool, HttpResponseMessage> nameValidator = CheckForUniqueName(userName);

            if (nameValidator.Item1)
            {
                return nameValidator;
            }

            Tuple<bool, HttpResponseMessage> emailValidator = CheckForUniqueEmail(email);

            if (emailValidator.Item1)
            {
                return emailValidator;
            }

            /*Player checkForUniqueEmail = (from check in db.AllPlayers
                                          where check.Email == email
                                          select check).FirstOrDefault();
            if (checkForUniqueEmail != null)
            {
                return new Tuple<bool,HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Email address already taken" }).ToString(), Encoding.UTF8, "application/json")
                });
            }*/

            Device checkForUniqueDevice = (from check in db.AllDevices
                                           where check.UUID == UUID
                                           select check).FirstOrDefault();
            if (checkForUniqueDevice != null)
            {
                return new Tuple<bool,HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Device already has an account attached to it" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }

        public static Tuple<bool, HttpResponseMessage> ValidatePlayerInformation(int id, string password)
        {
            string checkPassword = (from check in db.AllPlayers
                                    where check.ID == id
                                    select check.Password).FirstOrDefault();

            if (checkPassword == null)
            {
                return new Tuple<bool,HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid user ID" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            if (checkPassword != password)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid password" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }

        public static Tuple<bool, HttpResponseMessage> CheckForUniqueName(string name)
        {
            Player checkForUniqueName = (from check in db.AllPlayers
                                         where check.UserName == name
                                         select check).FirstOrDefault();
            if (checkForUniqueName != null)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Username already taken" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }

        public static Tuple<bool, HttpResponseMessage> CheckForUniqueEmail(string email)
        {
            Player checkForUniqueEmail = (from check in db.AllPlayers
                                         where check.UserName == email
                                         select check).FirstOrDefault();
            if (checkForUniqueEmail != null)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Email already taken" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }

        public static Tuple<bool, HttpResponseMessage> ValidatePlayerInformationWithEmail(int id, string password, string email)
        {
            Tuple<bool, HttpResponseMessage> playerValidator = ValidatePlayerInformation(id, password);
            if(playerValidator.Item1)
            {
                return playerValidator;
            }

            string checkEmail = (from check in db.AllPlayers
                                 where check.ID == id
                                 select check.Email).FirstOrDefault();
            if (checkEmail != email)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid email" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }
    }
}