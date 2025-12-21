using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MemoryCleaner.Infrastructure;
using MemoryCleaner.Models;
using MemoryCleaner.Services;

namespace MemoryCleaner.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly DiskAnalysisService _analysisService = new();
    private readonly CleanupService _cleanupService = new();

    private CancellationTokenSource? _cts;

    public ObservableCollection<string> Drives { get; } = new();
    public ObservableCollection<FolderSizeInfo> TopFolders { get; } = new();

    // ---- Units ----
    private SizeUnit _selectedUnit = SizeUnit.GB;
    public SizeUnit SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            if (Set(ref _selectedUnit, value))
            {
                // обновить summary + таблицу (таблица обновится через MultiBinding автоматически)
                RaisePropertyChanged(nameof(DiskSummary));
            }
        }
    }

    public Array Units => Enum.GetValues(typeof(SizeUnit));

    // ---- Drive selection ----
    private string? _selectedDrive;
    public string? SelectedDrive
    {
        get => _selectedDrive;
        set
        {
            if (Set(ref _selectedDrive, value))
                AnalyzeCommand.RaiseCanExecuteChanged();
        }
    }

    // ---- Disk info ----
    private DiskInfo? _disk;
    public DiskInfo? Disk
    {
        get => _disk;
        set
        {
            if (Set(ref _disk, value))
                RaisePropertyChanged(nameof(DiskSummary));
        }
    }

    public string DiskSummary =>
        Disk == null
            ? "Диск не выбран"
            : $"{Disk.Drive}  Всего: {Format(Disk.SizeBytes)}  Свободно: {Format(Disk.FreeBytes)}  Занято: {Format(Disk.UsedBytes)}";

    private string Format(long bytes) => SelectedUnit switch
    {
        SizeUnit.Bytes => $"{bytes:N0} B",
        SizeUnit.MB => $"{bytes / 1024d / 1024d:N2} MB",
        SizeUnit.GB => $"{bytes / 1024d / 1024d / 1024d:N2} GB",
        _ => $"{bytes:N0} B"
    };

    // ---- State ----
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (Set(ref _isBusy, value))
            {
                AnalyzeCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                CleanUserTempCommand.RaiseCanExecuteChanged();
                CleanWindowsTempCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string _status = "Готово";
    public string Status
    {
        get => _status;
        set => Set(ref _status, value);
    }

    private int _topN = 20;
    public int TopN
    {
        get => _topN;
        set => Set(ref _topN, value);
    }

    // ---- Commands ----
    public AsyncRelayCommand AnalyzeCommand { get; }
    public RelayCommand CancelCommand { get; }
    public AsyncRelayCommand CleanUserTempCommand { get; }
    public AsyncRelayCommand CleanWindowsTempCommand { get; }

    public MainViewModel()
    {
        AnalyzeCommand = new AsyncRelayCommand(AnalyzeAsync, CanAnalyze);
        CancelCommand = new RelayCommand(() => _cts?.Cancel(), () => IsBusy);

        CleanUserTempCommand = new AsyncRelayCommand(() => CleanTempAsync("User"), () => !IsBusy);
        CleanWindowsTempCommand = new AsyncRelayCommand(() => CleanTempAsync("Windows"), () => !IsBusy);

        LoadDrives();
        SelectedDrive = Drives.FirstOrDefault();
    }

    private bool CanAnalyze() => !IsBusy && !string.IsNullOrWhiteSpace(SelectedDrive);

    private void LoadDrives()
    {
        Drives.Clear();
        foreach (var d in DriveInfo.GetDrives().Where(x => x.IsReady))
            Drives.Add(d.Name);

        AnalyzeCommand.RaiseCanExecuteChanged();
    }

    private async Task AnalyzeAsync()
    {
        if (!CanAnalyze()) return;

        IsBusy = true;
        Status = "Анализ диска (PowerShell)...";
        TopFolders.Clear();
        Disk = null;

        _cts = new CancellationTokenSource();
        try
        {
            var result = await _analysisService.AnalyzeAsync(SelectedDrive!, TopN, _cts.Token);

            Disk = result.Disk;

            foreach (var f in result.Folders.OrderByDescending(x => x.SizeBytes))
                TopFolders.Add(f);

            Status = $"Готово. Обновлено: {result.GeneratedAt}";
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

    private async Task CleanTempAsync(string mode)
    {
        IsBusy = true;
        Status = mode == "User" ? "Очистка TEMP пользователя..." : "Очистка C:\\Windows\\Temp...";

        _cts = new CancellationTokenSource();
        try
        {
            var message = await _cleanupService.CleanTempAsync(mode, _cts.Token);
            Status = message;
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
