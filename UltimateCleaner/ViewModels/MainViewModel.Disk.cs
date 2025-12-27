using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MemoryCleaner.Infrastructure;
using MemoryCleaner.Models;

namespace MemoryCleaner.ViewModels;

public partial class MainViewModel
{
    private int _topN = 20;
    public int TopN
    {
        get => _topN;
        set => Set(ref _topN, value);
    }

    private SizeUnit _selectedUnit = SizeUnit.GB;
    public SizeUnit SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            if (Set(ref _selectedUnit, value))
                RaisePropertyChanged(nameof(DiskSummary));
        }
    }

    public Array Units => Enum.GetValues(typeof(SizeUnit));

    private string? _selectedDrive;
    public string? SelectedDrive
    {
        get => _selectedDrive;
        set
        {
            if (Set(ref _selectedDrive, value))
                AnalyzeCommand?.RaiseCanExecuteChanged();
        }
    }

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

    private partial void InitDisk()
    {
        AnalyzeCommand = new AsyncRelayCommand(AnalyzeAsync, CanAnalyze);
    }

    private bool CanAnalyze()
        => !IsBusy && !string.IsNullOrWhiteSpace(SelectedDrive);

    private void LoadDrives()
    {
        Drives.Clear();
        foreach (var d in DriveInfo.GetDrives().Where(x => x.IsReady))
            Drives.Add(d.Name);

        AnalyzeCommand?.RaiseCanExecuteChanged();
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

            SelectedFolder = TopFolders.FirstOrDefault();
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
