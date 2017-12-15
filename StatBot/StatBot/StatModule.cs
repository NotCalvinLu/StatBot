using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using StatBot;


namespace PartyBot
{
    public class StatModule : ModuleBase<SocketCommandContext>
    {
        private ulong _discordBotChannelId = Convert.ToUInt64(ConfigurationManager.AppSettings.Get("Discord_BotChannelId"));
        private List<string> _discordAdminRoles = ConfigurationManager.AppSettings.Get("Discord_AdminRoles").Split(',').ToList();
        private string _googlePublicStatSheet = ConfigurationManager.AppSettings.Get("GoogleApi_PublicStatSheet");
        private StatSessionService _statSessionService;
        private SheetsWrapper _sheetWrapper;

        public StatModule(StatSessionService statSessionService, SheetsWrapper sheetWrapper)
        {
            _statSessionService = statSessionService;
            _sheetWrapper = sheetWrapper;
        }

        [Command("update")]
        public async Task UpdateAsync()
        {
            if (IsMessageSentFromBotChannel())
            {
                var user = Context.Guild.GetUser(Context.User.Id);
                await _statSessionService.StartStatSessionAsync(user);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Check your PMs :)");
            }
        }

        [Command("view")]
        public async Task NotifyPartyAsync()
        {
            if (IsMessageSentFromBotChannel())
            {
                if (await HasPrivilegeAsync())
                {
                    await Context.Channel.SendMessageAsync($"View guild stats here:\n{_googlePublicStatSheet}");
                }
            }
        }

        [Command("delete")]
        public async Task DeleteAsync(ulong playerId)
        {
            if (IsMessageSentFromBotChannel())
            {
                if (await HasPrivilegeAsync())
                {
                    if (_sheetWrapper.DeleteUser(playerId))
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} Deleted the user ID: {playerId}");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} Could not find user with ID: {playerId}");
                    }
                }
            }
        }

        //TODO: Convert to Require​Context​Attribute or Custom
        private bool IsMessageSentFromBotChannel()
        {
            return Context.Channel.Id == _discordBotChannelId;
        }

        //TODO: Convert to RequireUserPermissionAttribute or Custom
        private async Task<bool> HasPrivilegeAsync()
        {
            var user = Context.Guild.GetUser(Context.User.Id);
            var isAdmin = user.Roles.Any(x => _discordAdminRoles.Contains(x.Name));
            if (!isAdmin)
            {
                await Context.Channel.SendMessageAsync("You don't have the privileges to perform this command");
            }
            return isAdmin;
        }

    }
}