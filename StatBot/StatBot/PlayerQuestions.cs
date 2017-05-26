using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatBot
{
    class PlayerQuestions
    {
        public enum CharClass { BERSERKER, DARK_KNIGHT, KUNOICHI, MAEHWA, MUSA, NINJA, RANGER, SORCERESS, TAMER, VALKYRIE, WARRIOR, WITCH, WIZARD }

        int curQuestion = 0;
        User user;

        //Information for the bot to collect on the user:
        ulong userID;
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

        public PlayerQuestions(User user)
        {
            this.user = user;
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
                    question = $"What __class__ is {charName}?";
                    break;
                case 4:
                    question = $"What __level__ is {charName}?";
                    break;
                case 5:
                    question = $"How much __non-awakened AP__ does {charName} have?";
                    break;
                case 6:
                    question = $"How much __awakened AP__ does {charName} have?";
                    break;
                case 7:
                    question = $"How much __DP__ does {charName} have?";
                    break;
                case 8:
                    question = "Please open the __P__ and ___I__ menus in BDO take a screenshot of your stats. Just send me the image in here.";
                    break;
                case 9:
                    question = $"What is the next gear upgrade planned for {charName}? (Type as much as you need)";
                    break;
                case 10:
                    question = $"What is your current level goal for {charName}? (Type as much as you need)";
                    break;
                case 11:
                    question = "Do you have M2 trading on any of your characters or are you working towards it? How many crates do you sell per month?";
                    break;
                case 12:
                    question = "Is there any other information that you think you'd like to include here? (Type as much as you want, type \"no\" if not)";
                    break;
            }

            user.SendMessage(question);
        }

        public void processMessage(string message)
        {
            switch (curQuestion)
            {
                //TODO question answer logic.
            }

            askQuestion();
        }

        public void processFile(Message.Attachment file)
        {
            if (curQuestion == 8)
            {
                //TODO Imgur stuff.
            }

            askQuestion();
        }

        public string getClassList()
        {
            return "";
        }
    }
}
