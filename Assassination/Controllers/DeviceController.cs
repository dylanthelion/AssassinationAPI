using Assassination.Helpers;
using Assassination.Models;
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
    public class DeviceController : ApiController
    {
        AssassinationContext db = new AssassinationContext();

        [HttpPost]
        public HttpResponseMessage AddDeviceToAccount(string userName, string password, string UUID)
        {
            Player checkPlayer = (from check in db.AllPlayers
                                  where check.UserName == userName
                                  select check).FirstOrDefault();
            Device checkDevice = (from check in db.AllDevices
                                  where check.UUID == UUID
                                  select check).FirstOrDefault();

            RequestValidators validator = new RequestValidators();

            Tuple<bool, HttpResponseMessage> playerValidator = validator.ValidatePlayerInformationWithUserName(userName, password);
            if(playerValidator.Item1)
            {
                return playerValidator.Item2;
            }

            Tuple<bool, HttpResponseMessage> deviceValidator = validator.CheckForUniqueDevice(UUID);
            if (deviceValidator.Item1)
            {
                return deviceValidator.Item2;
            }

            /*if (checkPlayer == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid user name" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkPlayer.Password != password)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid password" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkDevice != null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Device is already attached to an account" }).ToString(), Encoding.UTF8, "application/json")
                };
            }*/

            Device d = new Device(checkPlayer, UUID);
            db.AllDevices.Add(d);
            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { String.Format("User ID: {0}", checkPlayer.ID.ToString()) }).ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpDelete]
        public HttpResponseMessage RemoveDevice(int playerID, string password, string UUID)
        {
            //Player checkPlayer = db.AllPlayers.Find(playerID);

            Device checkDevice = (from check in db.AllDevices
                                  where check.UUID == UUID
                                  select check).FirstOrDefault();

            RequestValidators validator = new RequestValidators();

            Tuple<bool, HttpResponseMessage> deviceValidator = validator.ValidateDevice(playerID, password, UUID);
            if (deviceValidator.Item1)
            {
                return deviceValidator.Item2;
            }

            /*if (checkPlayer == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid user ID" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkPlayer.Password != password)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid password" }).ToString(), Encoding.UTF8, "application/json")
                };
            }

            if (checkDevice == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Invalid device ID" }).ToString(), Encoding.UTF8, "application/json")
                };
            }*/

            db.AllDevices.Remove(checkDevice);
            db.Entry(checkDevice).State = EntityState.Deleted;
            db.SaveChanges();

            return new HttpResponseMessage()
            {
                Content = new StringContent(JArray.FromObject(new List<String>() { "Deleted!" }).ToString(), Encoding.UTF8, "application/json")
            };
        }
    }
}