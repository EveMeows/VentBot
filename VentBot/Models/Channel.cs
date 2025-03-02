namespace VentBot.Models;

public class Channel
{
    public ulong ID { get; set; }
    public ulong CreatedBy { get; set; }

    public DateTime LastMessage { get; set; }

    public ulong GuildID { get; set; }
    public Guild Guild { get; set; } = null!;
}
