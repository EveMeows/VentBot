using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Timers;
using VentBot.Models;
using VentBot.Services.Databases;
using Timer = System.Timers.Timer;

namespace VentBot.Services;

public class VentBotService(ILogger<VentBotService> logger, DiscordClient client, IServiceProvider services, IDbContextFactory<SQLite> factory) : IHostedService, IDisposable
{
    #region Events
    private async Task SlashErrored(SlashCommandsExtension s, SlashCommandErrorEventArgs e)
	{
		logger.LogError("Failed to Execute command: {}", e.Exception.Message);
		
		try
		{
			await e.Context.CreateResponseAsync(
				"An error occurred while executing the command!", true
			);
		}
		catch (BadRequestException)
		{
			await e.Context.EditResponseAsync(new DiscordWebhookBuilder()
				.WithContent("An error occurred while executing the command!")
			);
		}
	}

    private async void CheckChannels(object? sender, ElapsedEventArgs e)
    {
        await using SQLite context = await factory.CreateDbContextAsync();

        foreach (Channel channel in context.ActiveChannels.Include(c => c.Guild))
        {
            if (!channel.Guild.AutoDelete) continue;

            DiscordChannel discordChannel = await client.GetChannelAsync(channel.ID);
            
            // Get last message timestamp of channel.
            IReadOnlyList<DiscordMessage> messages = await discordChannel.GetMessagesAsync(1);
            DateTime lastMessage = messages[0].Timestamp.DateTime;

            DateTime now = DateTime.Now;
            TimeSpan diff = now - lastMessage;

            if (diff.TotalMinutes > channel.Guild.DeletionTimeout)
            {
                // Erase from discord.
                await discordChannel.DeleteAsync();

                // Erase from database
                channel.Guild.ActiveChannels.Remove(channel);
                context.ActiveChannels.Remove(channel);

                await context.SaveChangesAsync();
            }
        }
    }
    #endregion

    private Timer? _timer;

    private async Task EnsureDatabaseExistence()
    {
        await using SQLite context = await factory.CreateDbContextAsync();

        // Testing purposes
        // Ensure the database is deleted first
        // We do this incase we change how the tables look while developing.
        bool deleted = context.Database.EnsureDeleted();
		logger.LogInformation("DataBase erasure status: {}", deleted ? "success" : "failure");

		bool recreated = context.Database.EnsureCreated();
		logger.LogInformation("DataBase creation status: {}", recreated ? "success" : "failure");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // We use code-first because I can't be asked.
        await EnsureDatabaseExistence();

        // Set the check timer.
        // It will check if any venting channels are ready to be deleted every 50 seconds.
        _timer = new Timer(50000);
        _timer.AutoReset = true;
        _timer.Enabled = true;

        _timer.Elapsed += CheckChannels;

        client.UseInteractivity(new InteractivityConfiguration { Timeout = TimeSpan.FromSeconds(30) });

		SlashCommandsExtension slash = client.UseSlashCommands(new SlashCommandsConfiguration { 
			Services = services
		});

		slash.RegisterCommands(Assembly.GetExecutingAssembly());
		slash.SlashCommandErrored += SlashErrored;

		await client.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Client disconnecting.");
        await client.DisconnectAsync();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
