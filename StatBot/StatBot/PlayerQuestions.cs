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

            askQuestion();
        }

        public void askQuestion()
        {
            string question = "";

            switch (curQuestion)
            {
                case 0:
                    //TODO: Add actual questions.
                    break;
            }

            user.SendMessage(question);
        }
    }
}
