using XForensics.Core.FileSystem;
using System.Collections.ObjectModel;

namespace XForensics.App.Models;

public sealed class ExplorerNode
{
    private bool _childrenLoaded;

    public ExplorerNode(string displayName, DirectoryEntry? entry, bool isRoot = false)
    {
        DisplayName = displayName;
        Entry = entry;
        IsRoot = isRoot;
        Children = new ObservableCollection<ExplorerNode>();
        if (entry?.IsDirectory() == true)
            Children.Add(Placeholder);
    }

    public string DisplayName { get; }
    public DirectoryEntry? Entry { get; }
    public bool IsRoot { get; }
    public bool IsDirectory => IsRoot || Entry?.IsDirectory() == true;
    public ObservableCollection<ExplorerNode> Children { get; }
    public ExplorerNode? Parent { get; set; }
    public bool IsPlaceholder => ReferenceEquals(this, Placeholder);

    private static ExplorerNode Placeholder => new("Loading...", null);

    public void EnsureChildrenLoaded()
    {
        if (_childrenLoaded || Entry is null || !Entry.IsDirectory())
            return;

        _childrenLoaded = true;
        if (Children.Count == 1 && Children[0].IsPlaceholder)
            Children.Clear();

        foreach (var child in Entry.Children.OrderBy(c => !c.IsDirectory()).ThenBy(c => c.FileName, StringComparer.OrdinalIgnoreCase))
        {
            var node = new ExplorerNode(child.FileName, child) { Parent = this };
            Children.Add(node);
        }
    }
}
