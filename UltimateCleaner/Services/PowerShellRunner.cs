using System.Diagnostics;
using System.Text;

namespace MemoryCleaner.Services;

public class PowerShellRunner
{
    public async Task<(string stdout, string stderr, int exitCode)> RunFileAsync(
        string ps1Path,
        IEnumerable<string> args,
        CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        psi.ArgumentList.Add("-NoProfile");
        psi.ArgumentList.Add("-ExecutionPolicy");
        psi.ArgumentList.Add("Bypass");
        psi.ArgumentList.Add("-File");
        psi.ArgumentList.Add(ps1Path);

        foreach (var a in args)
            psi.ArgumentList.Add(a);

        using var p = new Process { StartInfo = psi };

        var outSb = new StringBuilder();
        var errSb = new StringBuilder();

        p.OutputDataReceived += (_, e) => { if (e.Data != null) outSb.AppendLine(e.Data); };
        p.ErrorDataReceived += (_, e) => { if (e.Data != null) errSb.AppendLine(e.Data); };

        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        await p.WaitForExitAsync(ct);

        return (outSb.ToString(), errSb.ToString(), p.ExitCode);
    }
}
