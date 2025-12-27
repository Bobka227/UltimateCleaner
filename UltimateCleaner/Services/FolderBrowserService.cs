using MemoryCleaner.Models;
using System.IO;

namespace MemoryCleaner.Services;

public class FolderBrowserService
{
    public async Task<List<FileSystemEntryInfo>> GetEntriesAsync(string folderPath, CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();

            var result = new List<FileSystemEntryInfo>();

            foreach (var dir in Directory.EnumerateDirectories(folderPath))
            {
                ct.ThrowIfCancellationRequested();
                var di = new DirectoryInfo(dir);

                result.Add(new FileSystemEntryInfo
                {
                    Name = di.Name,
                    FullPath = di.FullName,
                    IsDirectory = true,
                    SizeBytes = 0,
                    LastWriteTime = di.LastWriteTime
                });
            }

            foreach (var file in Directory.EnumerateFiles(folderPath))
            {
                ct.ThrowIfCancellationRequested();
                var fi = new FileInfo(file);

                result.Add(new FileSystemEntryInfo
                {
                    Name = fi.Name,
                    FullPath = fi.FullName,
                    IsDirectory = false,
                    SizeBytes = fi.Length,
                    LastWriteTime = fi.LastWriteTime
                });
            }

            return result
                .OrderByDescending(x => x.IsDirectory)
                .ThenBy(x => x.Name)
                .ToList();
        }, ct);
    }
}
