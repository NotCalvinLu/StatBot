using Discord;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace StatBot
{
    public class StatSessionService
    {
        Dictionary<ulong, StatData> _statSessions = new Dictionary<ulong, StatData>();
        private string _invalidNumberMessage = "Please provide a valid number";
        private ImgurWrapper _imgurWrapper;
        private SheetsWrapper _sheetsWrapper;
        private string _googlePublicStatSheet = ConfigurationManager.AppSettings.Get("GoogleApi_PublicStatSheet");

        public StatSessionService()
        {
            _imgurWrapper = new ImgurWrapper();
            _sheetsWrapper = new SheetsWrapper();
        }

        public async Task StartStatSessionAsync(SocketGuildUser user)
        {
            await user.SendMessageAsync("Hello! I am just going to ask you a few questions about your character in BDO. Just chat back to me your answers.");
            var statData = new StatData(user);
            _statSessions.Add(user.Id, statData);
            LogMessage($"{user.Username} began updating their stats");
            LogMessage($"Current users updating stats: {_statSessions.Count}");
            await ContinueStatSessionAsync(statData);
        }

        public void LogMessage(string msg)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {msg}");
        }

        public async Task ContinueStatSessionAsync(StatData data)
        {

            string question = "";

            switch (data.QuestionNumber)
            {
                case 0:
                    question = "What is your __Family__ Name?";
                    break;
                case 1:
                    question = "What is your __Main Character__ Name?";
                    break;
                case 2:
                    question = $"What is your __Main Character__ Class? Please type just the number from the following list:\n{GetClassList()}";
                    break;
                case 3:
                    question = $"What __level__ is {data.CharName}?";
                    break;
                case 4:
                    question = $"How much __non-awakened AP__ does {data.CharName} have?";
                    break;
                case 5:
                    question = $"How much __awakened AP__ does {data.CharName} have?";
                    break;
                case 6:
                    question = $"How much __DP__ does {data.CharName} have?";
                    break;
                case 7:
                    question = "Please open the __I__ and __P__ menus in BDO and take a screenshot of your stats. Then send it to me right here.";
                    break;
                case 8:
                    question = $"What is the next gear upgrade planned for {data.CharName}? (Type as much as you need)";
                    break;
                case 9:
                    question = $"What is your current level goal for {data.CharName}? (Type as much as you need)";
                    break;
                case 10:
                    question = "Do you have M2 trading on any of your characters or are you working towards it? How many crates do you sell per month?";
                    break;
                case 11:
                    question = "Is there any other information that you think you'd like to include here? (Type as much as you want, type \"no\" if not)";
                    break;
                case 12:
                    question = $"Thanks for taking the time to enter your stats into our database. Stats can be viewed here:\n{_googlePublicStatSheet}";
                    _sheetsWrapper.UpdateUser(data);
                    _statSessions.Remove(data.UserId);
                    LogMessage($"{data.User.Username} finished updating their stats");
                    LogMessage($"Current users updating stats: {_statSessions.Count}");
                    break;
            }

            await data.User.SendMessageAsync(question);
        }

        public async Task ProcessMessageAsync(SocketGuildUser user, string message)
        {
            //Check if user already started updating their stats
            if (!_statSessions.ContainsKey(user.Id))
            {
                await StartStatSessionAsync(user);
            }
            else
            {
                var statData = _statSessions[user.Id];
                switch (statData.QuestionNumber)
                {
                    case 0:
                        statData.FamilyName = message;
                        statData.QuestionNumber++;
                        break;
                    case 1:
                        statData.CharName = message;
                        statData.QuestionNumber++;
                        break;
                    case 2:
                        int num;
                        if (int.TryParse(message, out num) && num >= 0 &&
                            num < Enum.GetNames(typeof(StatData.CharClass)).Length)
                        {
                            statData.Class = (StatData.CharClass) num;
                            statData.QuestionNumber++;
                        }
                        else
                        {
                            await user.SendMessageAsync("Please provide the number corresponding to your class");
                        }
                        break;
                    case 3:
                        if (int.TryParse(message, out statData.Level) && statData.Level >= 1)
                        {
                            statData.QuestionNumber++;
                        }
                        else
                        {
                            await user.SendMessageAsync(_invalidNumberMessage);
                        }
                        break;
                    case 4:
                        if (int.TryParse(message, out statData.AP))
                        {
                            statData.QuestionNumber++;
                        }
                        else
                        {
                            await user.SendMessageAsync(_invalidNumberMessage);
                        }
                        break;
                    case 5:
                        if (int.TryParse(message, out statData.AwakenedAp))
                        {
                            statData.QuestionNumber++;
                        }
                        else
                        {
                            await user.SendMessageAsync(_invalidNumberMessage);
                        }
                        break;
                    case 6:
                        if (int.TryParse(message, out statData.DP))
                        {
                            statData.QuestionNumber++;
                        }
                        else
                        {
                            await user.SendMessageAsync(_invalidNumberMessage);
                        }
                        break;
                    case 7:
                        await user.SendMessageAsync("Please give me an attachment/screenshot and not a link!");
                        break;
                    case 8:
                        statData.NextUpgrade = message;
                        statData.QuestionNumber++;
                        break;
                    case 9:
                        statData.NextLevel = message;
                        statData.QuestionNumber++;
                        break;
                    case 10:
                        statData.CrateQuestion = message;
                        statData.QuestionNumber++;
                        break;
                    case 11:
                        statData.OtherInfo = message;
                        statData.QuestionNumber++;
                        break;
                }
            
                await ContinueStatSessionAsync(statData);
            }
        }

        public async Task ProcessAttachmentAsync(SocketGuildUser user, Attachment attachment)
        {
            if (!_statSessions.ContainsKey(user.Id))
            {
                await StartStatSessionAsync(user);
            }
            else
            {
                var statData = _statSessions[user.Id];

                if (statData.QuestionNumber == 7)
                {
                    await user.SendMessageAsync("Please wait while I upload this image to Imgur...");

                    string location = $"images/{attachment.Filename}";

                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(new Uri(attachment.Url), location);
                    }

                    statData.ScreenshotUrl = _imgurWrapper.UploadImage(location);
                    statData.ScreenshotUrl = String.Format("=HYPERLINK(\"{0}\",\"{1}\")", statData.ScreenshotUrl,
                        "Gear Screenshot!");

                    File.Delete(location);

                    if (!statData.ScreenshotUrl.Equals(""))
                    {
                        statData.QuestionNumber++;
                    }
                    else
                    {
                        await user.SendMessageAsync(
                            "Oh no! I couldn't upload the image. Please try it again and contact Iso if the problem persists");
                    }
                }
                else
                {
                    await user.SendMessageAsync("Please answer the question using text please :)");
                }

                await ContinueStatSessionAsync(statData);
            }
        }

        public string GetClassList()
        {
            string list = "";

            string[] names = Enum.GetNames(typeof(StatData.CharClass));

            for (int i = 0; i < names.Length; i++)
            {
                list += i + " - " + names[i].Replace("_", " ") + "\n";
            }

            return list;
        }
    }
}
