﻿using Assassination.Models;
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

            Tuple<bool, HttpResponseMessage> deviceValidator = CheckForUniqueDevice(UUID);
            if (deviceValidator.Item1)
            {
                return deviceValidator;
            }

            /*Device checkForUniqueDevice = (from check in db.AllDevices
                                           where check.UUID == UUID
                                           select check).FirstOrDefault();
            if (checkForUniqueDevice != null)
            {
                return new Tuple<bool,HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Device already has an account attached to it" }).ToString(), Encoding.UTF8, "application/json")
                });
            }*/

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }

        public static Tuple<bool, HttpResponseMessage> CheckForUniqueDevice(string UUID)
        {
            Device checkForUniqueDevice = (from check in db.AllDevices
                                           where check.UUID == UUID
                                           select check).FirstOrDefault();
            if (checkForUniqueDevice != null)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
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

        public static Tuple<bool, HttpResponseMessage> ValidatePlayerInformationWithUserName(string name, string password)
        {
            string checkPassword = (from check in db.AllPlayers
                                    where check.UserName == name
                                    select check.Password).FirstOrDefault();

            if (checkPassword == null)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
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

        public static Tuple<bool, HttpResponseMessage> ValidateDevice(int playerID, string password, string UUID)
        {
            Tuple<bool, HttpResponseMessage> validator = ValidatePlayerInformation(playerID, password);
            if (validator.Item1)
            {
                return validator;
            }

            Device checkDevice = (from check in db.AllDevices
                                  where check.UUID == UUID && check.PlayerID == playerID
                                  select check).FirstOrDefault();

            if (checkDevice == null)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid device" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }

        public static Tuple<bool, HttpResponseMessage> ValidateAccount(int playerID, string password)
        {
            Tuple<bool, HttpResponseMessage> validator = ValidatePlayerInformation(playerID, password);
            if (validator.Item1)
            {
                return validator;
            }

            Account checkAccount = (from check in db.AllAccounts
                                    where check.PlayerID == playerID
                                    select check).FirstOrDefault();

            if (checkAccount == null)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You do not have an account set up" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }

        public static Tuple<bool, HttpResponseMessage> ValidateModerator(int playerID, string password, int gameID)
        {
            Tuple<bool, HttpResponseMessage> validator = ValidateIfInGame(playerID, password, gameID);
            if (validator.Item1)
            {
                return validator;
            }

            bool isModerator = (from check in db.AllPlayerGames
                                where check.PlayerID == playerID && check.GameID == gameID
                                select check.IsModerator).FirstOrDefault();
            if (!isModerator)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You are not the moderator of that game" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }

        public static Tuple<bool, HttpResponseMessage> ValidateGame(int gameID)
        {
            Game checkGame = db.AllGames.Find(gameID);
            if (checkGame == null)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid game ID" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }

        public static Tuple<bool, HttpResponseMessage> ValidateAliveInGame(int playerID, string password, int gameID)
        {
            Tuple<bool, HttpResponseMessage> validator = ValidatePlayerInformation(playerID, password);
            if (validator.Item1)
            {
                return validator;
            }

            Tuple<bool, HttpResponseMessage> gameValidator = ValidateGame(gameID);
            if (gameValidator.Item1)
            {
                return gameValidator;
            }

            bool checkActiveGame = db.AllGames.Find(gameID).IsActiveGame;
            if (!checkActiveGame)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "That game has not been set up, yet" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            bool checkAlive = (from check in db.AllPlayerGames
                               where check.GameID == gameID && check.PlayerID == playerID
                               select check.Alive).FirstOrDefault();
            if (!checkAlive)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You are not in that game, or you are dead" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }

        public static Tuple<bool, HttpResponseMessage> ValidateIfInGame(int playerID, string password, int gameID)
        {
            Tuple<bool, HttpResponseMessage> validator = ValidatePlayerInformation(playerID, password);
            if (validator.Item1)
            {
                return validator;
            }

            Tuple<bool, HttpResponseMessage> gameValidator = ValidateGame(gameID);
            if (gameValidator.Item1)
            {
                return gameValidator;
            }

            PlayerGame checkIfInGame = (from check in db.AllPlayerGames
                                        where check.PlayerID == playerID && check.GameID == gameID
                                        select check).FirstOrDefault();
            if (checkIfInGame == null)
            {
                return new Tuple<bool, HttpResponseMessage>(true, new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "You are not in that game" }).ToString(), Encoding.UTF8, "application/json")
                });
            }

            return new Tuple<bool, HttpResponseMessage>(false, new HttpResponseMessage());
        }
    }
}