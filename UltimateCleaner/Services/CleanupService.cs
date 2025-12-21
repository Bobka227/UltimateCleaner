using System.IO;
using System.Text.Json;

namespace MemoryCleaner.Services;

public class CleanupService
{
    private readonly PowerShellRunner _runner = new();

    public async Task<string> CleanTempAsync(string mode, CancellationToken ct)
    {
        var ps1 = Path.Combine(AppContext.BaseDirectory, "Scripts", "CleanTemp.ps1");
        if (!File.Exists(ps1))
            throw new FileNotFoundException($"Не найден скрипт: {ps1}", ps1);

        var args = new[] { "-Mode", mode };
        var (stdout, stderr, exitCode) = await _runner.RunFileAsync(ps1, args, ct);

        if (exitCode != 0)
            throw new InvalidOperationException($"PowerShell код {exitCode}: {stderr}");

        using var doc = JsonDocument.Parse(stdout);
        var msg = doc.RootElement.GetProperty("message").GetString();
        return msg ?? "Готово.";
    }
}
