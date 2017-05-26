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
        string clientID = "82b177039bae94e";
        string clientSecret = "";
        string url = "http://api.imgur.com/2/upload.json";

        public string uploadImage(string location)
        {
            try
            {
                var client = new ImgurClient(clientID, clientSecret);
                var endpoint = new ImageEndpoint(client);
                IImage image;
                using (var fs = new FileStream(location, FileMode.Open))
                {
                    image = endpoint.UploadImageStreamAsync(fs).GetAwaiter().GetResult();
                }
                return image.Link;
            }
            catch (ImgurException)
            {
                return "";
            }
        }
    }
}
