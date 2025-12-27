using System;
using System.Threading;
using System.Threading.Tasks;

using MemoryCleaner.Infrastructure;

namespace MemoryCleaner.ViewModels;

public partial class MainViewModel
{
    private bool _dismResetBase;
    public bool DismResetBase
    {
        get => _dismResetBase;
        set => Set(ref _dismResetBase, value);
    }

    private string _dismLog = "";
    public string DismLog
    {
        get => _dismLog;
        set => Set(ref _dismLog, value);
    }

    private partial void InitCleanup()
    {
        CleanUserTempCommand = new AsyncRelayCommand(() => CleanTempAsync("User"), () => !IsBusy);
        CleanWindowsTempCommand = new AsyncRelayCommand(() => CleanTempAsync("Windows"), () => !IsBusy);
        DismCleanupCommand = new AsyncRelayCommand(DismCleanupAsync, () => !IsBusy);
    }

    private async Task CleanTempAsync(string mode)
    {
        IsBusy = true;
        Status = mode == "User"
            ? "Очистка TEMP пользователя..."
            : "Очистка C:\\Windows\\Temp...";

        _cts = new CancellationTokenSource();
        try
        {
            var res = await _cleanupService.CleanTempAsync(mode, _cts.Token);

            Status = $"{res.Message} Удалено: {res.Deleted}, Ошибок: {res.Failed}";
        }
        catch (OperationCanceledException)
        {
            Status = "Отменено.";
        }
        catch (Exception ex)
        {
            Status = "Ошибка: " + ex.Message;
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            IsBusy = false;
        }
    }

    private async Task DismCleanupAsync()
    {
        IsBusy = true;
        Status = "DISM: StartComponentCleanup...";
        DismLog = "";

        _cts = new CancellationTokenSource();
        try
        {
            var res = await _dismService.StartComponentCleanupAsync(DismResetBase, _cts.Token);

            Status = res.NeedsAdmin
                ? "Нужны права администратора. Запусти приложение от администратора."
                : res.Message;

            DismLog =
                $"Args: {res.Args}\n" +
                $"Ok: {res.Ok}\n" +
                $"NeedsAdmin: {res.NeedsAdmin}\n" +
                $"ExitCode: {res.ExitCode}\n\n" +
                (string.IsNullOrWhiteSpace(res.Stdout) ? "" : $"STDOUT:\n{res.Stdout}\n\n") +
                (string.IsNullOrWhiteSpace(res.Stderr) ? "" : $"STDERR:\n{res.Stderr}\n");
        }
        catch (OperationCanceledException)
        {
            Status = "Отменено.";
        }
        catch (Exception ex)
        {
            Status = "Ошибка: " + ex.Message;
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            IsBusy = false;
        }
    }
}
