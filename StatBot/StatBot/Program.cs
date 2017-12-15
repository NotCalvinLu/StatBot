using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Net.Providers.WS4Net;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Contexts;

namespace StatBot
{
    public class Program
    {
        #region Config values

        private ulong _guildServerId = Convert.ToUInt64(ConfigurationManager.AppSettings.Get("Discord_GuildServerId"));
        private readonly string _discordBotToken = ConfigurationManager.AppSettings.Get("Discord_BotToken");
        private List<string> _discordAdminRoles = ConfigurationManager.AppSettings.Get("Discord_AdminRoles").Split(',').ToList();

        #endregion

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private StatSessionService _statSessionService;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            DiscordSocketConfig config;
            if (bool.Parse(ConfigurationManager.AppSettings.Get("Win7")))
            {
                config = new DiscordSocketConfig
                {
                    WebSocketProvider = WS4NetProvider.Instance
                };
            }
            else
            {
                config = new DiscordSocketConfig();
            }
            _commands = new CommandService();
            _client = new DiscordSocketClient(config);
            _statSessionService = new StatSessionService();
            await _client.LoginAsync(TokenType.Bot, _discordBotToken);
            await _client.StartAsync();
            InstallServices(_client);
            await InstallCommands();
            Console.WriteLine("Stat bot up and running, awaiting commands.");
            await Task.Delay(-1);
        }

        private void InstallServices(DiscordSocketClient client)
        {
            var serviceCollection = new ServiceCollection();
            // Here, we will inject the ServiceProvider with
            // all of the services our client will use.
            serviceCollection.AddSingleton(client);
            serviceCollection.AddSingleton(_statSessionService);
            serviceCollection.AddSingleton(new SheetsWrapper());
            _services = serviceCollection.BuildServiceProvider();
        }

        private async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            _client.MessageReceived += HandleCommandAsync;
            _client.MessageReceived += HandlePrivateMessageAsync;
            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;
            // Create a Command Context
            var context = new SocketCommandContext(_client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            //Commented out code handles command failures
            //if (!result.IsSuccess)
            //    await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        private async Task HandlePrivateMessageAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            var context = new SocketCommandContext(_client, message);

            //Don't process messages that arn't private or are from bots
            if (!context.IsPrivate || context.User.IsBot) return;
            var user = _client.GetGuild(_guildServerId).GetUser(context.User.Id);
            if (!HasPrivilege(user))
            {
                await context.User.SendMessageAsync("Oh no! It looks like you don't have the Member role on Element's discord.\n\n" +
                    $"If this is a mistake and you do actually have the member role, just shoot {_client.GetGuild(_guildServerId).GetUser(132697256900952064).Mention} " +
                    "a quick PM. This is a bug that sometimes happens to new members who recently joined the Discord. I blame the current Discord API >:[");
                return;
            }
            if (context.Message.Attachments.Count > 0)
            {
                await _statSessionService.ProcessAttachmentAsync(user, context.Message.Attachments.FirstOrDefault());
            }
            else
            {
                await _statSessionService.ProcessMessageAsync(user, context.Message.Content);
            }
        }

        private bool HasPrivilege(SocketGuildUser user)
        {
            return user?.Roles.Any(x => _discordAdminRoles.Contains(x.Name) || x.Name.ToLower() == "member") ?? false;
        }
    }
}