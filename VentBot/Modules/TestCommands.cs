using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace VentBot.Modules;

public class TestCommands : ApplicationCommandModule
{
    [SlashCommand("ping", "pong.")]
    public async Task PingCommand(
        InteractionContext ctx
    )
    {
        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .WithContent("Pong.")
        );
    }
}
