using DSharpPlus;
using System.Reflection;
using VentBot.Services;
using VentBot.Services.Databases;

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

// Database Factory
// We use a factory because, unlike ASP.NET
// Normal Worker Services do not automatically handle lifetimes.
builder.Services.AddDbContextFactory<SQLite>();


// Add the main service.
builder.Services.AddHostedService<VentBotService>();

IHost host = builder.Build();
host.Run();
