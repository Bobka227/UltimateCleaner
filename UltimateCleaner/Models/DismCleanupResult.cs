namespace MemoryCleaner.Models;

public class DismCleanupResult
{
    public bool Ok { get; set; }
    public bool NeedsAdmin { get; set; }
    public int ExitCode { get; set; }
    public string Message { get; set; } = "";
    public string Args { get; set; } = "";
    public string Stdout { get; set; } = "";
    public string Stderr { get; set; } = "";
}
