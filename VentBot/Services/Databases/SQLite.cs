using Microsoft.EntityFrameworkCore;
using VentBot.Models;

namespace VentBot.Services.Databases;

public class SQLite(IConfiguration config) : DbContext, IGuildTemplate
{
    public DbSet<Guild> Guilds { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string database = config["AppSettings:SQLiteName"] 
            ?? throw new ArgumentNullException("Cannot find SQLite Database name.");

        optionsBuilder.UseSqlite($"Data Source={Path.Join(documents, database)}");
    }
}
