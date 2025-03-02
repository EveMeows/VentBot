using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using VentBot.Models;
using VentBot.Services.Databases;

namespace VentBot.Modules;

[RequireUserPermissions(Permissions.Administrator)]
[SlashCommandGroup("admin", "Administrative commands. (Commands will automatically enroll.)")]
public class AdministrativeCommands(IDbContextFactory<SQLite> factory) : ApplicationCommandModule
{
    [SlashCommand("enroll", "Enroll the server into the database.")]
    public async Task EnrollCommand(InteractionContext ctx)
    {
        await using SQLite context = await factory.CreateDbContextAsync();

        Guild? dbGuild = await context.Guilds.FirstOrDefaultAsync(g => g.ID == ctx.Guild.Id);
        if (dbGuild is not null)
        {
            await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Your guild is already enrolled into the database!")
                        .AsEphemeral()
                );

            return;
        }

        Guild enroll = new Guild
        {
            ID = ctx.Guild.Id
        };

        await context.Guilds.AddAsync(enroll);
		await context.SaveChangesAsync();

        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .WithContent("Thank you! You are now part of the family!")
                .AsEphemeral()
        );
    }

    [SlashCommand("setvent", "Set the venting catgeory.")]
    public async Task VentCategoryCommand(
        InteractionContext ctx,
        [Option("category", "The category to set.")] DiscordChannel channel
    )
    {
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
        [Option("timeout", "The new timeout.")] long timeout,
        [Option("autodelete", "Should the channel autodelete?")] bool delete
    )
    {
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
