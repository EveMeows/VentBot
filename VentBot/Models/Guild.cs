namespace VentBot.Models;

public class Guild
{
    public ulong ID { get; set; }

    public ulong VentCategory { get; set; }
    public List<Channel> ActiveChannels { get; set; } = [];

    public int DeletionTimeout { get; set; } = 10;
    public bool AutoDelete { get; set; } = true;
}
