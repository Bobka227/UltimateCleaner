namespace MemoryCleaner.Models;

public class FileSystemEntryInfo
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public bool IsDirectory { get; set; }
    public long SizeBytes { get; set; }
    public DateTime LastWriteTime { get; set; }
}
