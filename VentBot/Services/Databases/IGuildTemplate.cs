using Microsoft.EntityFrameworkCore;
using VentBot.Models;

namespace VentBot.Services.Databases;

public interface IGuildTemplate
{
    DbSet<Guild> Guilds { get; set; }
}
