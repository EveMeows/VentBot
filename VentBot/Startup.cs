using DSharpPlus;
using System.Reflection;
using VentBot.Services;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// User secrets.
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

// Services.
builder.Services.AddSingleton(x => 
	new DiscordClient(new DiscordConfiguration { 
		Token = x.GetRequiredService<IConfiguration>()["Token"], 
		Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | 
			DiscordIntents.MessageContents
    })
);

// Add the main service.
builder.Services.AddHostedService<VentBotService>();

IHost host = builder.Build();
host.Run();
