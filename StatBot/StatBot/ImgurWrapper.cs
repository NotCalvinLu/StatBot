using Imgur.API;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using System.Configuration;
using System.IO;

namespace StatBot
{
    public class ImgurWrapper
    {
        static readonly string ClientId = ConfigurationManager.AppSettings.Get("ImgurApi_ClientId");
        static readonly string ClientSecret = ConfigurationManager.AppSettings.Get("ImgurApi_ClientSecret");
        private readonly ImgurClient _imgurClient;

        public ImgurWrapper()
        {
            _imgurClient = new ImgurClient(ClientId, ClientSecret);
        }

        public string UploadImage(string location)
        {
            try
            {
                var endpoint = new ImageEndpoint(_imgurClient);
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
