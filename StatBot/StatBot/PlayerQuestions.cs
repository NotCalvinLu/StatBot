using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StatBot
{
    class PlayerQuestions
    {
        StatBot main;

        public enum CharClass { Berserker, Dark_Knight, Kunoichi, Maehwa, Musa, Ninja, Ranger, Sorceress, Tamer, Valkyrie, Warrior, Witch, Wizard }

        int curQuestion = 0;
        User user;

        //Information for the bot to collect on the user:
        public ulong userID;
        string discordName;
        string familyName;
        string charName;
        CharClass charClass;
        int level;
        int ap;
        int awakenAp;
        int dp;
        string screenshotUrl;
        string nextUpgrade;
        string nextLevel;
        string crateQuestion;
        string otherInfo;
        /////////////////////////////////////////////////

        public PlayerQuestions(User user, StatBot main)
        {
            this.user = user;
            this.main = main;
            userID = user.Id;
            discordName = user.Nickname;

            if (discordName == null || discordName.Equals(""))
            {
                discordName = user.Name;
            }

            user.SendMessage("Hello! I am just going to ask you a few questions about your character in BDO. Just chat back to me your answers.");
            askQuestion();
        }

        public void askQuestion()
        {
            string question = "";

            switch (curQuestion)
            {
                case 0:
                    question = "What is your __Family__ Name?";
                    break;
                case 1:
                    question = "What is your __Main Character__ Name?";
                    break;
                case 2:
                    question = $"What is your __Main Character__ Class? Please type just the number from the following list:\n{getClassList()}";
                    break;
                case 3:
                    question = $"What __level__ is {charName}?";
                    break;
                case 4:
                    question = $"How much __non-awakened AP__ does {charName} have?";
                    break;
                case 5:
                    question = $"How much __awakened AP__ does {charName} have?";
                    break;
                case 6:
                    question = $"How much __DP__ does {charName} have?";
                    break;
                case 7:
                    question = "Please open the __P__ and ___I__ menus in BDO take a screenshot of your stats. Just send me the image in here.";
                    break;
                case 8:
                    question = $"What is the next gear upgrade planned for {charName}? (Type as much as you need)";
                    break;
                case 9:
                    question = $"What is your current level goal for {charName}? (Type as much as you need)";
                    break;
                case 10:
                    question = "Do you have M2 trading on any of your characters or are you working towards it? How many crates do you sell per month?";
                    break;
                case 11:
                    question = "Is there any other information that you think you'd like to include here? (Type as much as you want, type \"no\" if not)";
                    break;
                case 12:
                    question = $"Thanks for taking the time to enter your stats into our database. Stats can be viewed here:\n{main.sheetLocation}";
                    main.updateUser(this);
                    break;
            }

            user.SendMessage(question);
        }

        public void processMessage(string message)
        {
            switch (curQuestion)
            {
                case 0:
                    familyName = message;
                    curQuestion++;
                    break;
                case 1:
                    charName = message;
                    curQuestion++;
                    break;
                case 2:
                    int num;
                    if (int.TryParse(message, out num) && num >= 0 && num < Enum.GetNames(typeof(CharClass)).Length)
                    {
                        charClass = (CharClass)num;
                        curQuestion++;
                    }
                    break;
                case 3:
                    if (int.TryParse(message, out level) && level >= 1)
                    {
                        curQuestion++;
                    }
                    break;
                case 4:
                    if (int.TryParse(message, out ap))
                    {
                        curQuestion++;
                    }
                    break;
                case 5:
                    if (int.TryParse(message, out awakenAp))
                    {
                        curQuestion++;
                    }
                    break;
                case 6:
                    if (int.TryParse(message, out dp))
                    {
                        curQuestion++;
                    }
                    break;
                case 8:
                    nextUpgrade = message;
                    curQuestion++;
                    break;
                case 9:
                    nextLevel = message;
                    curQuestion++;
                    break;
                case 10:
                    crateQuestion = message;
                    curQuestion++;
                    break;
                case 11:
                    otherInfo = message;
                    curQuestion++;
                    break;
            }

            askQuestion();
        }

        public void processFile(Message.Attachment att)
        {
            if (curQuestion == 7)
            {
                string location = $"images/{att.Filename}";

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(att.Url), location);
                }

                screenshotUrl = main.imgur.uploadImage(location);
                screenshotUrl = String.Format("=HYPERLINK(\"{0}\",\"{1}\")", screenshotUrl, "Gear Screenshot!");

                File.Delete(location);

                if (!screenshotUrl.Equals(""))
                {
                    curQuestion++;
                }
            }

            askQuestion();
        }

        public string getClassList()
        {
            string list = "";

            string[] names = Enum.GetNames(typeof(CharClass));

            for (int i = 0; i < names.Length; i++)
            {
                list += i + " - " + names[i].Replace("_", " ") + "\n";
            }

            return list;
        }

        public string getClassString(CharClass charClass)
        {
            return charClass.ToString().Replace("_", " ");
        }

        public List<object> GetList()
        {
            return new List<object> {
                Convert.ToString(userID),
                DateTime.Today.ToString("d"),
                discordName,
                familyName,
                charName,
                getClassString(charClass),
                level,
                ap,
                awakenAp,
                dp,
                screenshotUrl,
                nextUpgrade,
                nextLevel,
                crateQuestion,
                otherInfo
            };
        }
    }
}
