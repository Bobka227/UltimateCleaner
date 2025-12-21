using MemoryCleaner.Models;
using System.IO;
using System.Text.Json;

namespace MemoryCleaner.Services;

public class CleanupService
{
    private readonly PowerShellRunner _runner = new();

    public async Task<CleanTempResult> CleanTempAsync(string mode, CancellationToken ct)
    {
        var ps1 = Path.Combine(AppContext.BaseDirectory, "Scripts", "CleanTemp.ps1");
        if (!File.Exists(ps1))
            throw new FileNotFoundException("Не найден скрипт CleanTemp.ps1.", ps1);

        var (stdout, stderr, exitCode) = await _runner.RunFileAsync(
            ps1,
            new Dictionary<string, object?>
            {
                ["Mode"] = mode
            },
            ct);

        if (exitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(stderr) ? "Ошибка очистки TEMP." : stderr);

        var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<CleanTempResult>(stdout, opt);

        return result ?? throw new InvalidOperationException("Не удалось распарсить JSON от CleanTemp.ps1.");
    }
}
