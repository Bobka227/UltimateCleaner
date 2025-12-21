namespace MemoryCleaner.Models;

public class CleanTempResult
{
    public bool Ok { get; set; }
    public string Mode { get; set; } = "";
    public string Target { get; set; } = "";
    public int Deleted { get; set; }
    public int Failed { get; set; }
    public string Message { get; set; } = "";
}
