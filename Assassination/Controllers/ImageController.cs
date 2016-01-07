using Assassination.Helpers;
using Assassination.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace Assassination.Controllers
{
    public class ImageController : ApiController
    {
        AssassinationContext db = new AssassinationContext();

        [System.Web.Http.HttpPost]
        public HttpResponseMessage SetImage(int playerID, string password)
        {
            RequestValidators validator = new RequestValidators();
            Tuple<bool, HttpResponseMessage> validateplayer = validator.ValidatePlayerInformation(playerID, password);
            if (validateplayer.Item1)
            {
                return validateplayer.Item2;
            }

            var result = new HttpResponseMessage(HttpStatusCode.OK);
            if (Request.Content.IsMimeMultipartContent())
            {
                string userName = db.AllPlayers.Find(playerID).UserName;
                Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((task) =>
                {
                    MultipartMemoryStreamProvider provider = task.Result;
                    foreach (HttpContent content in provider.Contents)
                    {
                        Stream stream = content.ReadAsStreamAsync().Result;
                        Image image = Image.FromStream(stream);
                        String filePath = HostingEnvironment.MapPath("~/Content/Images/");
                        String fileName = userName + ".jpg";
                        String fullPath = Path.Combine(filePath, fileName);
                        image.Save(fullPath);
                    }
                });
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Image uploaded!" }).ToString(), Encoding.UTF8, "application/json")
                };
            }
            else
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "Unacceptable format" }).ToString(), Encoding.UTF8, "application/json")
                };
            }
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage GetImage(string playerName)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            String filePath = HostingEnvironment.MapPath(String.Format("~/Content/Images/{0}", playerName));
            if (!File.Exists(filePath))
            {
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JArray.FromObject(new List<String>() { "No image uploaded for that user" }).ToString(), Encoding.UTF8, "application/json")
                };
            }
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            Image image = Image.FromStream(fileStream);
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageFormat.Jpeg);
            result.Content = new ByteArrayContent(memoryStream.ToArray());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            return result;
        }
    }
}