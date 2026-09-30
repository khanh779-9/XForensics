using XForensics.Core.Analyzers;
using XForensics.Core.FileSystem;
using XForensics.Core.Database;

namespace XForensics.App.Models;

public sealed class PartitionSession
{
    public PartitionSession(PartitionDatabase database)
    {
        Database = database;
        Volume = database.Volume;
        FileDatabase = database.GetFileDatabase();
    }

    public PartitionDatabase Database { get; }
    public Volume Volume { get; }
    public FileDatabase FileDatabase { get; }
    public FileCarver? FileCarver => Database.GetFileCarver();
    public IntegrityAnalyzer? Integrity { get; private set; }
    public bool HasMetadataAnalysis => Database.HasMetadataAnalysis;

    public IntegrityAnalyzer EnsureIntegrity()
        => Integrity ??= new IntegrityAnalyzer(Volume, FileDatabase);

    public void RefreshIntegrity()
    {
        EnsureIntegrity().Update();
    }
}
