using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace StatBot
{
    class StatBot
    {
        DiscordClient discord;

        public StatBot()
        {
            discord = new DiscordClient();

            discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
            });

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

        public void print(string msg)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {msg}");
        }

        public void updateCommand(CommandEventArgs e)
        {
            e.Channel.SendMessage("update");
        }

        public void viewCommand(CommandEventArgs e)
        {
            e.Channel.SendMessage("view");
        }
    }
}
