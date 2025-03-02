using Microsoft.EntityFrameworkCore;
using VentBot.Models;

namespace VentBot.Services.Databases;

public class SQLite(IConfiguration config) : DbContext, IGuildTemplate
{
    public DbSet<Guild> Guilds { get; set; }
    public DbSet<Channel> ActiveChannels { get; set; }

    public async Task<Guild> EnsureGuildAsync(ulong id)
    {
        Guild? guild = await Guilds.Include(g => g.ActiveChannels).FirstOrDefaultAsync(g => g.ID == id);
        if (guild is null)
        {
            guild = new Guild
            {
                ID = id
            };

            await Guilds.AddAsync(guild);
            await SaveChangesAsync();
        }

        return guild;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Channel>()
            .HasOne(c => c.Guild)
            .WithMany(g => g.ActiveChannels)
            .HasForeignKey(c => c.GuildID)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string database = config["AppSettings:SQLiteName"] 
            ?? throw new ArgumentNullException("Cannot find SQLite Database name.");

        optionsBuilder.UseSqlite($"Data Source={Path.Join(documents, database)}");
    }
}
