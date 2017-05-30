using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatBot
{
    class StatBot
    {
        DiscordClient discord;

        ulong serverID = 155415749962366976;
        public string sheetLocation = "https://docs.google.com/spreadsheets/d/1QjRNBh9_2SOdPQPN2JAjW_2TWl_LOGAGGsFD3ZNqNG0/edit?usp=sharing";

        Dictionary<ulong, PlayerQuestions> questions = new Dictionary<ulong, PlayerQuestions>();

        public ImgurWrapper imgur;
        SheetsWrapper sheets;

        public StatBot()
        {
            imgur = new ImgurWrapper(this);
            sheets = new SheetsWrapper(this);

            discord = new DiscordClient();

            discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
            });

            discord.MessageReceived += (s, e) =>
            {
                messageReceived(e);
            };

            var commands = discord.GetService<CommandService>();

            commands.CreateCommand("update")
                .Do((e) =>
                {
                    updateCommand(e);
                });

            commands.CreateCommand("view")
                .Do((e) =>
                {
                    viewCommand(e);
                });

            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect("Mjg2NjIwMjcyMDI5ODU5ODQw.DAjonA.z6aD4yhDPFJWzY2bOgP7Zxb0mcc", TokenType.Bot);
            });
        }

        public void printCurrentUserCount()
        {
            print($"Current Users Doing Questions: {questions.Count}");
        }

        public void print(string msg)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {msg}");
        }

        public void updateCommand(CommandEventArgs e)
        {
            if (e.Channel.Id != 280444392756609024) return;

            PlayerQuestions playerQuestions;
            if (questions.TryGetValue(e.User.Id, out playerQuestions))
            {
                playerQuestions.askQuestion();
            }
            else
            {
                questions.Add(e.User.Id, new PlayerQuestions(e.User, this));
            }

            e.Channel.SendMessage($"{e.User.Mention} Check your PMs :)");
        }

        public void viewCommand(CommandEventArgs e)
        {
            if (e.Channel.Id != 280444392756609024) return;

            e.Channel.SendMessage($"View guild stats here:\n{sheetLocation}");
        }

        public void updateUser(PlayerQuestions player)
        {
            questions.Remove(player.userID);
            sheets.UpdateUser(player);
            printCurrentUserCount();
        }

        public void messageReceived(MessageEventArgs e)
        {
            if (!e.Channel.IsPrivate || e.User.IsBot) return;

            if (!isMember(e.User.Id))
            {
                e.User.SendMessage("Oh no! It looks like you don't have the Member role on Element's discord.\n\n" +
                    $"If this is a mistake and you do actually have the member role, just shoot {discord.GetServer(serverID).GetUser(132697256900952064).Mention} " +
                    "a quick PM. This is a bug that sometimes happens to new members who recently joined the Discord. I blame the current Discord API >:[");
                return;
            }

            PlayerQuestions playerQuestions;
            if (questions.TryGetValue(e.User.Id, out playerQuestions))
            {
                if (e.Message.Attachments.Length > 0)
                {
                    playerQuestions.processFile(e.Message.Attachments[0]);
                }
                else
                {
                    playerQuestions.processMessage(e.Message.RawText);
                }
            }
            else
            {
                questions.Add(e.User.Id, new PlayerQuestions(e.User, this));
            }
        }

        public bool isMember(ulong id)
        {
            Server server = discord.GetServer(serverID);
            IEnumerable<Role> roles = server.GetUser(id).Roles;

            foreach(Role role in roles)
            {
                if (role.Name.ToLower().Equals("member") || role.Name.ToLower().Equals("lead group♠"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
