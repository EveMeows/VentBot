using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using VentBot.Models;
using VentBot.Services.Databases;

namespace VentBot.Modules;

[SlashCommandGroup("admin", "Administrative commands.")]
[RequirePermissions(Permissions.Administrator)]
public class AdministrativeCommands(IDbContextFactory<SQLite> factory) : ApplicationCommandModule
{
    #region Helpers
    private async Task<bool> CheckAdmin(InteractionContext ctx)
	{
		if (!ctx.Member.Permissions.HasPermission(Permissions.Administrator)) 
		{
			await ctx.CreateResponseAsync(
				InteractionResponseType.ChannelMessageWithSource,
				new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
					.WithContent("You do not have permission to do that!\nAsk your server administrator to do this...")
					.AsEphemeral()
			);
			return false;
		}

		return true;
	}
	#endregion

    [SlashCommand("setvent", "Set the venting catgeory.")]
    public async Task VentCategoryCommand(
        InteractionContext ctx,
        [Option("category", "The category to set.")] DiscordChannel channel
    )
    {
        if (!await CheckAdmin(ctx)) return;

        await using SQLite context = await factory.CreateDbContextAsync();

        Guild guild = await context.EnsureGuildAsync(ctx.Guild.Id);

        if (!channel.IsCategory)
        {
            await ctx.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent("Channel must be a category!")
                    .AsEphemeral()
            );

            return;
        }

        guild.VentCategory = channel.Id;

        context.Update(guild);
        await context.SaveChangesAsync();

        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .WithContent($"Successfully set {channel.Mention} as the main venting category!")
                .AsEphemeral()
        );
    }

    [SlashCommand("deletion", "Set the vent deletion settings.")]
    public async Task TimeoutCommad(
        InteractionContext ctx,
        [Option("timeout", "The new timeout in minutes.")] long timeout,
        [Option("autodelete", "Should the channel autodelete?")] bool delete
    )
    {
        if (!await CheckAdmin(ctx)) return;
        
        if (timeout < 1 || timeout > 60)
        {
            await ctx.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent("The timeout must be between 1 and 60 minutes!")
                    .AsEphemeral()
            );

            return;
        }

        await using SQLite context = await factory.CreateDbContextAsync();

        Guild guild = await context.EnsureGuildAsync(ctx.Guild.Id);

        guild.AutoDelete = delete;
        guild.DeletionTimeout = (int)timeout;

        context.Update(guild);
        await context.SaveChangesAsync();

        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .WithContent("Successfully updated deletion settings!")
                .AsEphemeral()
        );
    }
}
