using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using VentBot.Models;
using VentBot.Services.Databases;

namespace VentBot.Modules;

[SlashCommandGroup("vent", "Commands relating to venting.")]
public class VentCommands(IDbContextFactory<SQLite> factory) : ApplicationCommandModule
{
    [SlashCommand("start", "Start a vent channel.")]
    public async Task StartCommand(InteractionContext ctx)
    {
        await using SQLite context = await factory.CreateDbContextAsync();

        Guild guild = await context.EnsureGuildAsync(ctx.Guild.Id);
        if (guild.VentCategory == 0)
        {
            await ctx.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent($"The server does not have a vent category set up! Please contact the moderators.")
                    .AsEphemeral()
            );

            return;
        }

        DiscordClient client = ctx.Client;

        // Check if the user already has a vent channel open.
        foreach (Channel activeChannel in guild.ActiveChannels)
        {
            if (activeChannel.CreatedBy == ctx.User.Id)
            {
                DiscordChannel discordChannel = await client.GetChannelAsync(activeChannel.ID);
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Hello! You seem to already have a channel active... Please use {discordChannel.Mention} instead.")
                        .AsEphemeral()
                );

                return;
            }
        }

        DiscordChannel ventCategory = await client.GetChannelAsync(guild.VentCategory);
        string channelName = $"safe-space-for-{ctx.Member.DisplayName}";

        DiscordChannel ventChannel = await ctx.Guild.CreateChannelAsync(
            channelName, ChannelType.Text, ventCategory
        );

        // Remove ability for everyone that isn't a mod to see the channel.
        await ventChannel.AddOverwriteAsync(ctx.Guild.EveryoneRole, deny: Permissions.AccessChannels);

        // Add the ability for only the user to see said channel.
        await ventChannel.AddOverwriteAsync(ctx.Member, allow: Permissions.AccessChannels);

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
        { 
            Title = $"Hello, {ctx.Member.DisplayName}!",
            Description = $"This is your safe space, {ctx.Member.Mention}, you are free to let anything off your chest. Don't worry, no one can see this channel, aside the moderators."
        };

        if (guild.AutoDelete)
        { 
            embed.AddField(
                "Auto deletion.",
                $"This channel will delete itself after it goes inactive for {guild.DeletionTimeout} minute(s).",
                true
            );
        }

        embed.AddField(
            "Done venting?",
            "You can type /vent finish.",
            true
        );

        await ventChannel.SendMessageAsync(embed);

        // Add channel data to database.
        Channel channel = new Channel
        {
            ID = ventChannel.Id,
            CreatedBy = ctx.Member.Id,
        };

        guild.ActiveChannels.Add(channel);
        await context.ActiveChannels.AddAsync(channel);

        await context.SaveChangesAsync();

        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .WithContent($"Successfully created {ventChannel.Mention}.")
                .AsEphemeral()
        );
    }

    [SlashCommand("finish", "Delete your vent channel.")]
    public async Task FinishCommand(InteractionContext ctx)
    { 
        await using SQLite context = await factory.CreateDbContextAsync();

        Guild guild = await context.EnsureGuildAsync(ctx.Guild.Id);
        foreach (Channel channel in guild.ActiveChannels)
        {
            if (channel.CreatedBy == ctx.Member.Id)
            {
                DiscordChannel discordChannel = await ctx.Client.GetChannelAsync(channel.ID);
                await discordChannel.DeleteAsync("The user has decided to end their session.");

                // Remove the channel from the database
                guild.ActiveChannels.Remove(channel);
                context.ActiveChannels.Remove(channel);

                await context.SaveChangesAsync();

                if (channel.ID != ctx.Channel.Id)
                { 
                    await ctx.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Your channel has been successfully deleted.")
                            .AsEphemeral()
                    );
                }

                return;
            }
        }

        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .WithContent($"You do not have any vent channel open!")
                .AsEphemeral()
        );

    }
}
