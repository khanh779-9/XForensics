using XForensics.Core;
using XForensics.Core.FileSystem;
using XForensics.Core.Database;
using System.Collections.ObjectModel;

namespace XForensics.App.Models;

public sealed class DriveSession : IDisposable
{
    public DriveSession(string name, DriveReader reader)
    {
        Name = name;
        Reader = reader;
        Database = new DriveDatabase(name, reader);
        Partitions = new ObservableCollection<PartitionSession>();
    }

    public string Name { get; }
    public DriveReader Reader { get; }
    public DriveDatabase Database { get; }
    public ObservableCollection<PartitionSession> Partitions { get; }
    public PartitionSession? SelectedPartition { get; set; }
    public event Action<PartitionSession>? DatabaseAnalysisLoaded;

    public PartitionSession AddPartition(Volume volume)
    {
        var existing = Partitions.FirstOrDefault(p => p.Volume.Offset == volume.Offset);
        if (existing != null)
            return existing;

        var db = Database.AddPartition(volume);
        var session = new PartitionSession(db);
        db.OnLoadRecoveryFromDatabase += (_, _) => DatabaseAnalysisLoaded?.Invoke(session);
        Partitions.Add(session);
        return session;
    }

    public PartitionSession AddMountedPartition(Volume volume)
    {
        if (!volume.Mounted)
            volume.Mount();
        return AddPartition(volume);
    }

    public PartitionSession? FindPartition(long offset)
        => Partitions.FirstOrDefault(p => p.Volume.Offset == offset);

    public void Dispose()
    {
        try { Reader.Dispose(); } catch { }
    }
}
