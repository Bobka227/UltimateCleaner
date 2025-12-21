using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace MemoryCleaner.Services;

public class PowerShellRunner
{
    public async Task<(string stdout, string stderr, int exitCode)> RunFileAsync(
        string ps1Path,
        IDictionary<string, object?> parameters,
        CancellationToken ct)
    {
        if (!File.Exists(ps1Path))
            throw new FileNotFoundException("Не найден PowerShell-скрипт.", ps1Path);

        return await Task.Run(() =>
        {
            using var ps = PowerShell.Create();

            var scriptText = File.ReadAllText(ps1Path);
            ps.AddScript(scriptText, useLocalScope: true);

            foreach (var p in parameters)
                ps.AddParameter(p.Key, p.Value);

            using var reg = ct.Register(() =>
            {
                try { ps.Stop(); } catch { }
            });

            Collection<PSObject> results = ps.Invoke();

            var stdout = string.Join(Environment.NewLine, results.Select(r => r?.ToString() ?? ""));
            var stderr = string.Join(Environment.NewLine, ps.Streams.Error.Select(e => e.ToString()));
            var exitCode = ps.HadErrors ? 1 : 0;

            return (stdout, stderr, exitCode);
        }, ct);
    }
}
