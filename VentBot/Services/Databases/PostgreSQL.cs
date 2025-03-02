using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VentBot.Models;

namespace VentBot.Services.Databases;

[Obsolete("Use SQLite.")]
public class PostgreSQL : DbContext, IGuildTemplate
{
    public DbSet<Guild> Guilds { get; set; }
}
