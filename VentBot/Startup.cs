using VentBot.Services;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<VentBotService>();

IHost host = builder.Build();
host.Run();
