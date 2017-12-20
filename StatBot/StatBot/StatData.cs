using System;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;

namespace StatBot
{
    public class StatData
    {
        public enum CharClass
        {
            Berserker,
            Dark_Knight,
            Kunoichi,
            Maehwa,
            Musa,
            Ninja,
            Ranger,
            Sorceress,
            Striker,
            Tamer,
            Valkyrie,
            Warrior,
            Witch,
            Wizard,
            Mystic
        }

        public ulong UserId;
        public SocketGuildUser User;
        public string DiscordName;
        public string FamilyName;
        public string CharName;
        public CharClass Class;
        public int Level;
        public int AP;
        public int AwakenedAp;
        public int DP;
        public string ScreenshotUrl;
        public string NextUpgrade;
        public string NextLevel;
        public string CrateQuestion;
        public string OtherInfo;
        public int QuestionNumber;

        public StatData(SocketGuildUser user)
        {
            User = user;
            UserId = user.Id;
            DiscordName = user.Nickname;

            if (DiscordName == null || DiscordName.Equals(""))
            {
                DiscordName = user.Username;
            }
        }

        public List<Object> GetDataToPrint()
        {
            return new List<object> {
                Convert.ToString(UserId),
                DateTime.Today.ToString("d"),
                DiscordName,
                FamilyName,
                CharName,
                Class.ToString().Replace("_"," "),
                Level,
                AP,
                AwakenedAp,
                DP,
                ScreenshotUrl,
                NextUpgrade,
                NextLevel,
                CrateQuestion,
                OtherInfo
            };
        }
    }
}
