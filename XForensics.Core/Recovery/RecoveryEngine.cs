using XForensics.Core.Analyzers.Signatures;
using XForensics.Core.FileSystem;
using XForensics.Core.Database;
using System.IO;

namespace XForensics.Core.Recovery;

public static class RecoveryEngine
{
    private const int BufferSize = 1024 * 1024;

    public static void SaveDatabaseFile(Volume volume, DatabaseFile file, string destinationRoot, CancellationToken token, Action<string>? progress = null)
    {
        var safeName = Sanitize(file.FileName);
        var destination = MakeUniquePath(destinationRoot, safeName);

        if (file.IsDirectory())
        {
            Directory.CreateDirectory(destination);
            foreach (var child in file.Children)
            {
                token.ThrowIfCancellationRequested();
                SaveDatabaseFile(volume, child, destination, token, progress);
            }
            TryRestoreTimestamps(destination, file);
            return;
        }

        WriteClusterChain(volume, file.ClusterChain, file.FileSize, destination, file.FileName, token, progress);
        TryRestoreTimestamps(destination, file);
    }

    public static void SaveDirectoryEntry(Volume volume, DirectoryEntry entry, string destinationRoot, CancellationToken token, Action<string>? progress = null)
    {
        var safeName = Sanitize(entry.FileName);
        var destination = MakeUniquePath(destinationRoot, safeName);

        if (entry.IsDirectory())
        {
            Directory.CreateDirectory(destination);
            foreach (var child in entry.Children)
            {
                token.ThrowIfCancellationRequested();
                SaveDirectoryEntry(volume, child, destination, token, progress);
            }
            TryRestoreTimestamps(destination, entry);
            return;
        }

        WriteClusterChain(volume, volume.GetClusterChain(entry), entry.FileSize, destination, entry.FileName, token, progress);
        TryRestoreTimestamps(destination, entry);
    }

    public static void SaveCarvedFile(Volume volume, FileSignature signature, string destinationRoot, CancellationToken token, Action<string>? progress = null)
    {
        var size = Math.Min(signature.FileSize, Math.Max(0, volume.FileAreaLength - signature.Offset));
        if (size <= 0)
            return;

        var path = MakeUniquePath(destinationRoot, Sanitize(signature.FileName));
        var reader = volume.GetReader();
        volume.SeekFileArea(signature.Offset);

        using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize, FileOptions.SequentialScan);
        var buffer = new byte[BufferSize];
        long remaining = size;
        var chunks = 0;
        while (remaining > 0)
        {
            token.ThrowIfCancellationRequested();
            var count = (int)Math.Min(buffer.Length, remaining);
            reader.Read(buffer, count);
            output.Write(buffer, 0, count);
            remaining -= count;
            if (++chunks % 16 == 0 || remaining == 0)
                progress?.Invoke(signature.FileName);
        }
    }

    private static void WriteClusterChain(Volume volume, IReadOnlyList<uint> chain, long fileSize, string path, string progressName, CancellationToken token, Action<string>? progress)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        path = MakeUniqueFullPath(path);
        using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize, FileOptions.SequentialScan);
        var reader = volume.GetReader();
        var buffer = new byte[BufferSize];
        long remaining = fileSize;
        var clusterSize = Math.Max(1L, volume.BytesPerCluster);

        var clustersProcessed = 0;
        foreach (var cluster in chain)
        {
            token.ThrowIfCancellationRequested();
            if (remaining <= 0)
                break;

            volume.SeekToCluster(cluster);
            var clusterRemaining = Math.Min(clusterSize, remaining);
            while (clusterRemaining > 0)
            {
                token.ThrowIfCancellationRequested();
                var count = (int)Math.Min(buffer.Length, clusterRemaining);
                reader.Read(buffer, count);
                output.Write(buffer, 0, count);
                clusterRemaining -= count;
                remaining -= count;
            }
            if (++clustersProcessed % 64 == 0 || remaining == 0)
                progress?.Invoke(progressName);
        }
    }

    private static void TryRestoreTimestamps(string path, DatabaseFile file)
    {
        try
        {
            if (file.IsDirectory())
            {
                Directory.SetCreationTime(path, file.CreationTime.AsDateTime());
                Directory.SetLastWriteTime(path, file.LastWriteTime.AsDateTime());
                Directory.SetLastAccessTime(path, file.LastAccessTime.AsDateTime());
            }
            else
            {
                File.SetCreationTime(path, file.CreationTime.AsDateTime());
                File.SetLastWriteTime(path, file.LastWriteTime.AsDateTime());
                File.SetLastAccessTime(path, file.LastAccessTime.AsDateTime());
            }
        }
        catch { }
    }

    private static void TryRestoreTimestamps(string path, DirectoryEntry entry)
    {
        try
        {
            if (entry.IsDirectory())
            {
                Directory.SetCreationTime(path, entry.CreationTime.AsDateTime());
                Directory.SetLastWriteTime(path, entry.LastWriteTime.AsDateTime());
                Directory.SetLastAccessTime(path, entry.LastAccessTime.AsDateTime());
            }
            else
            {
                File.SetCreationTime(path, entry.CreationTime.AsDateTime());
                File.SetLastWriteTime(path, entry.LastWriteTime.AsDateTime());
                File.SetLastAccessTime(path, entry.LastAccessTime.AsDateTime());
            }
        }
        catch { }
    }

    private static string MakeUniquePath(string directory, string fileName)
    {
        Directory.CreateDirectory(directory);
        return MakeUniqueFullPath(Path.Combine(directory, fileName));
    }

    private static string MakeUniqueFullPath(string path)
    {
        if (!File.Exists(path)) return path;

        var directory = Path.GetDirectoryName(path)!;
        var file = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);
        for (var i = 1; i < 100000; i++)
        {
            var candidate = Path.Combine(directory, $"{file} ({i}){ext}");
            if (!File.Exists(candidate)) return candidate;
        }
        throw new IOException($"Could not create a unique output path for {path}.");
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "recovered_file";
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        if (cleaned is "." or ".." || cleaned.Length == 0) return "recovered_file";
        return cleaned;
    }
}
