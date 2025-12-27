using System.Collections.ObjectModel;
using System.Threading;

using MemoryCleaner.Infrastructure;
using MemoryCleaner.Models;
using MemoryCleaner.Services;

namespace MemoryCleaner.ViewModels;

public partial class MainViewModel : ObservableObject
{
   
    private readonly DiskAnalysisService _analysisService = new();
    private readonly CleanupService _cleanupService = new();
    private readonly DismService _dismService = new();
    private readonly FolderBrowserService _folderBrowserService = new();

    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _browseCts;

    public ObservableCollection<string> Drives { get; } = new();
    public ObservableCollection<FolderSizeInfo> TopFolders { get; } = new();
    public ObservableCollection<FileSystemEntryInfo> FolderEntries { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (Set(ref _isBusy, value))
            {
                AnalyzeCommand?.RaiseCanExecuteChanged();
                CancelCommand?.RaiseCanExecuteChanged();

                CleanUserTempCommand?.RaiseCanExecuteChanged();
                CleanWindowsTempCommand?.RaiseCanExecuteChanged();
                DismCleanupCommand?.RaiseCanExecuteChanged();

                OpenSelectedEntryCommand?.RaiseCanExecuteChanged();
                BackCommand?.RaiseCanExecuteChanged();
                OpenInExplorerCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    private string _status = "Готово";
    public string Status
    {
        get => _status;
        set => Set(ref _status, value);
    }

    public AsyncRelayCommand? AnalyzeCommand { get; private set; }
    public RelayCommand? CancelCommand { get; private set; }

    public AsyncRelayCommand? CleanUserTempCommand { get; private set; }
    public AsyncRelayCommand? CleanWindowsTempCommand { get; private set; }
    public AsyncRelayCommand? DismCleanupCommand { get; private set; }

    public AsyncRelayCommand? OpenSelectedEntryCommand { get; private set; }
    public RelayCommand? BackCommand { get; private set; }
    public RelayCommand? OpenInExplorerCommand { get; private set; }

    public MainViewModel()
    {
      
        InitDisk();
        InitCleanup();
        InitExplorer();

        CancelCommand = new RelayCommand(CancelAll, () => IsBusy);

        LoadDrives();
        SelectedDrive = Drives.FirstOrDefault();
    }

    private void CancelAll()
    {
        try { _cts?.Cancel(); } catch { }
        try { _browseCts?.Cancel(); } catch { }
    }

    private partial void InitDisk();
    private partial void InitCleanup();
    private partial void InitExplorer();
}
