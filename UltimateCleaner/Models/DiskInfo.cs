namespace MemoryCleaner.Models;

public class DiskInfo
{
    public string Drive { get; set; } = "";
    public string Root { get; set; } = "";
    public long SizeBytes { get; set; }
    public long FreeBytes { get; set; }
    public long UsedBytes { get; set; }
}
