using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using MemoryCleaner.Infrastructure;
using MemoryCleaner.Models;

namespace MemoryCleaner.ViewModels;

public partial class MainViewModel
{
    private readonly Stack<string> _navBack = new();

    private FolderSizeInfo? _selectedFolder;
    public FolderSizeInfo? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (Set(ref _selectedFolder, value))
            {
                if (_selectedFolder != null)
                {
                    _navBack.Clear();
                    BackCommand?.RaiseCanExecuteChanged();
                    _ = OpenFolderAsync(_selectedFolder.Path);
                }
            }
        }
    }

    private FileSystemEntryInfo? _selectedEntry;
    public FileSystemEntryInfo? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (Set(ref _selectedEntry, value))
                OpenSelectedEntryCommand?.RaiseCanExecuteChanged();
        }
    }

    private string _currentFolderPath = "";
    public string CurrentFolderPath
    {
        get => _currentFolderPath;
        set
        {
            if (Set(ref _currentFolderPath, value))
                OpenInExplorerCommand?.RaiseCanExecuteChanged();
        }
    }

    private partial void InitExplorer()
    {
        OpenSelectedEntryCommand = new AsyncRelayCommand(OpenSelectedEntryAsync, () => !IsBusy && SelectedEntry != null);
        BackCommand = new RelayCommand(GoBack, () => !IsBusy && _navBack.Count > 0);
        OpenInExplorerCommand = new RelayCommand(OpenCurrentInExplorer, () => !string.IsNullOrWhiteSpace(CurrentFolderPath));
    }

    private async Task OpenFolderAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        _browseCts?.Cancel();
        _browseCts?.Dispose();
        _browseCts = new CancellationTokenSource();

        try
        {
            Status = $"Открываю: {path}";
            CurrentFolderPath = path;

            FolderEntries.Clear();
            var entries = await _folderBrowserService.GetEntriesAsync(path, _browseCts.Token);

            foreach (var e in entries)
                FolderEntries.Add(e);

            Status = "Готово";
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Status = "Ошибка: " + ex.Message;
        }
    }

    private async Task OpenSelectedEntryAsync()
    {
        if (SelectedEntry == null) return;

        if (SelectedEntry.IsDirectory)
        {
            if (!string.IsNullOrWhiteSpace(CurrentFolderPath))
            {
                _navBack.Push(CurrentFolderPath);
                BackCommand?.RaiseCanExecuteChanged();
            }

            await OpenFolderAsync(SelectedEntry.FullPath);
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = SelectedEntry.FullPath,
                UseShellExecute = true
            });
        }
    }

    private void GoBack()
    {
        if (_navBack.Count == 0) return;

        var prev = _navBack.Pop();
        BackCommand?.RaiseCanExecuteChanged();

        _ = OpenFolderAsync(prev);
    }

    private void OpenCurrentInExplorer()
    {
        if (string.IsNullOrWhiteSpace(CurrentFolderPath)) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{CurrentFolderPath}\"",
            UseShellExecute = true
        });
    }
}
