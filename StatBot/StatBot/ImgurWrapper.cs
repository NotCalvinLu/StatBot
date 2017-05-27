using Imgur.API;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatBot
{
    class ImgurWrapper
    {
        StatBot main;

        static string clientID = "82b177039bae94e";
        static string clientSecret = "a1c41a501a279f07f9d8d1f68d2433b428cb02a1";

        ImgurClient client = new ImgurClient(clientID, clientSecret);

        public ImgurWrapper(StatBot main)
        {
            this.main = main;
        }

        public string uploadImage(string location)
        {
            main.print("Uploading image.");
            try
            {
                var endpoint = new ImageEndpoint(client);
                IImage image;
                using (var fs = new FileStream(location, FileMode.Open))
                {
                    image = endpoint.UploadImageStreamAsync(fs).GetAwaiter().GetResult();
                }
                main.print("Image uploaded.");
                return image.Link;
            }
            catch (ImgurException)
            {
                main.print("Error Uploading image.");
                return "";
            }
        }
    }
}
