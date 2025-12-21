using MemoryCleaner.Models;
using System.IO;
using System.Text.Json;

namespace MemoryCleaner.Services;

public class DiskAnalysisService
{
    private readonly PowerShellRunner _runner = new();

    public async Task<DiskAnalysisResult> AnalyzeAsync(string driveRoot, int top, CancellationToken ct)
    {
        var ps1 = Path.Combine(AppContext.BaseDirectory, "Scripts", "AnalyzeDisk.ps1");

        if (!File.Exists(ps1))
            throw new FileNotFoundException("Не найден скрипт AnalyzeDisk.ps1 в папке Scripts рядом с приложением.", ps1);

        var args = new[]
        {
            "-DriveRoot", driveRoot,
            "-Top", top.ToString()
        };

        var (stdout, stderr, exitCode) = await _runner.RunFileAsync(ps1, args, ct);

        if (exitCode != 0)
            throw new InvalidOperationException($"PowerShell завершился с кодом {exitCode}.\n{stderr}");

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<DiskAnalysisResult>(stdout, options);

        if (result == null)
            throw new InvalidOperationException("Не удалось распарсить JSON от PowerShell.");

        return result;
    }
}
