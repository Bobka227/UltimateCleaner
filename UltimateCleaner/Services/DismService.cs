using MemoryCleaner.Models;
using System.IO;
using System.Text.Json;

namespace MemoryCleaner.Services;

public class DismService
{
    private readonly PowerShellRunner _runner = new();

    public async Task<DismCleanupResult> StartComponentCleanupAsync(bool resetBase, CancellationToken ct)
    {
        var ps1 = Path.Combine(AppContext.BaseDirectory, "Scripts", "StartComponentCleanup.ps1");
        if (!File.Exists(ps1))
            throw new FileNotFoundException("Не найден скрипт StartComponentCleanup.ps1.", ps1);

        var parameters = new Dictionary<string, object?>();

        // switch параметр передаем только если true
        if (resetBase)
            parameters["ResetBase"] = true;

        var (stdout, stderr, exitCode) = await _runner.RunFileAsync(ps1, parameters, ct);

        // Скрипт возвращает JSON всегда (даже needsAdmin=true)
        var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<DismCleanupResult>(stdout, opt);

        if (result != null)
            return result;

        // fallback если JSON не распарсился
        return new DismCleanupResult
        {
            Ok = false,
            NeedsAdmin = false,
            ExitCode = exitCode,
            Message = "Не удалось распарсить JSON от DISM-скрипта.",
            Args = "StartComponentCleanup.ps1",
            Stdout = stdout,
            Stderr = string.IsNullOrWhiteSpace(stderr) ? "" : stderr
        };
    }
}
