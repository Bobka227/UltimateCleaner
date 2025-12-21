namespace MemoryCleaner.Models;

public class DiskAnalysisResult
{
    public DiskInfo Disk { get; set; } = new();
    public List<FolderSizeInfo> Folders { get; set; } = new();
    public string GeneratedAt { get; set; } = "";
}
