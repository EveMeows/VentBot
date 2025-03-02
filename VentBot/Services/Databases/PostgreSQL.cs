using Microsoft.EntityFrameworkCore;
using VentBot.Models;

namespace VentBot.Services.Databases;

[Obsolete("Use SQLite.")]
public class PostgreSQL : DbContext, IGuildTemplate
{
    public DbSet<Guild> Guilds { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public DbSet<Channel> ActiveChannels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public async Task<Guild> EnsureGuildAsync(ulong id)
    {
        throw new NotImplementedException();
    }
}
