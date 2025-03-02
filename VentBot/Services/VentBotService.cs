using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VentBot.Services.Databases;

namespace VentBot.Services;

public class VentBotService(ILogger<VentBotService> logger, DiscordClient client, IServiceProvider services, IDbContextFactory<SQLite> factory) : IHostedService
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
    #endregion

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
}
