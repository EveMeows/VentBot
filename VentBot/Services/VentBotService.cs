using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using System.Reflection;

namespace VentBot.Services;

public class VentBotService(ILogger<VentBotService> logger, DiscordClient client, IServiceProvider services) : IHostedService
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
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
