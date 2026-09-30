using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using XForensics.Core.FileSystem;
using XForensics.App.Models;

namespace XForensics.App.Controls;

public partial class FileExplorerView : UserControl
{
    public event EventHandler<FileRow?>? FileSelected;
    public event EventHandler<ExplorerNode?>? FolderSelected;
    public event EventHandler<string>? SubTabSelected;
    public event EventHandler<FileRow>? FileDoubleClicked;

    private readonly ObservableCollection<FileRow> _files = new();
    private List<FileRow> _masterFiles = new();
    private string _activeCategory = "All";

    public FileExplorerView()
    {
        InitializeComponent();
        FileGrid.ItemsSource = _files;
        LoadDefaultDemoHierarchy();
    }

    public void LoadDefaultDemoHierarchy()
    {
        FolderTree.Items.Clear();

        var root = new TreeViewItem { Header = CreateNodeHeader("FATX (C:)", true, true), IsExpanded = true };
        root.Items.Add(new TreeViewItem { Header = CreateNodeHeader("$RECYCLE.BIN", false, false) });
        root.Items.Add(new TreeViewItem { Header = CreateNodeHeader("System Volume Information", false, false) });

        var users = new TreeViewItem { Header = CreateNodeHeader("Users", false, true), IsExpanded = true };
        var admin = new TreeViewItem { Header = CreateNodeHeader("Administrator", false, true), IsExpanded = true };
        var docs = new TreeViewItem { Header = CreateNodeHeader("Documents", false, true), IsExpanded = true, IsSelected = true };
        docs.Items.Add(new TreeViewItem { Header = CreateNodeHeader("Projects", false, false) });
        docs.Items.Add(new TreeViewItem { Header = CreateNodeHeader("Photos", false, false) });
        docs.Items.Add(new TreeViewItem { Header = CreateNodeHeader("Work", false, false) });

        admin.Items.Add(docs);
        admin.Items.Add(new TreeViewItem { Header = CreateNodeHeader("Desktop", false, false) });
        admin.Items.Add(new TreeViewItem { Header = CreateNodeHeader("Downloads", false, false) });
        admin.Items.Add(new TreeViewItem { Header = CreateNodeHeader("Music", false, false) });
        admin.Items.Add(new TreeViewItem { Header = CreateNodeHeader("Pictures", false, false) });
        admin.Items.Add(new TreeViewItem { Header = CreateNodeHeader("Videos", false, false) });
        users.Items.Add(admin);
        root.Items.Add(users);

        var lost = new TreeViewItem { Header = CreateNodeHeader("Lost & Found (Recovered)", false, true), IsExpanded = true };
        lost.Items.Add(new TreeViewItem { Header = CreateNodeHeader("Deleted Files", false, false) });
        lost.Items.Add(new TreeViewItem { Header = CreateNodeHeader("Orphaned Files", false, false) });
        lost.Items.Add(new TreeViewItem { Header = CreateNodeHeader("Unallocated Space", false, false) });
        root.Items.Add(lost);

        FolderTree.Items.Add(root);

        // Populate sample files
        LoadDefaultDemoFiles();
    }

    private static StackPanel CreateNodeHeader(string text, bool isDrive, bool isFolder)
    {
        var sp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        if (isDrive)
        {
            var chk = new CheckBox { Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center, IsChecked = true };
            sp.Children.Add(chk);
        }
        var icon = new Path
        {
            Data = Application.Current.TryFindResource(isDrive ? "IconHardDrive" : "IconFolder") as Geometry,
            Width = 14, Height = 14, Stretch = Stretch.Uniform,
            Margin = new Thickness(0, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        icon.SetResourceReference(Path.FillProperty, isDrive ? "AccentBrush" : "FileFolderBrush");

        var tb = new TextBlock
        {
            Text = text,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };

        sp.Children.Add(icon);
        sp.Children.Add(tb);
        return sp;
    }

    public void LoadDefaultDemoFiles()
    {
        _masterFiles = new List<FileRow>
        {
            new() { Name = "Projects", Type = "Folder", SizeText = "--", Modified = "2025-06-12 14:32", Status = "Folder" },
            new() { Name = "Photos", Type = "Folder", SizeText = "--", Modified = "2025-05-21 09:12", Status = "Folder" },
            new() { Name = "Work", Type = "Folder", SizeText = "--", Modified = "2025-04-10 18:44", Status = "Folder" },
            new() { Name = "financial_report_2025.docx", Type = "Microsoft Word Document", SizeText = "2.4 MB", Size = 2480112, Modified = "2025-06-10 11:24", Status = "Good", OffsetText = "0x0001B000" },
            new() { Name = "banner_design_final.psd", Type = "Photoshop Document", SizeText = "48.2 MB", Size = 50541363, Modified = "2025-06-08 17:15", Status = "Good", OffsetText = "0x00240000" },
            new() { Name = "database_backup.sql", Type = "SQL Database File", SizeText = "128.5 MB", Size = 134742016, Modified = "2025-06-05 03:00", Status = "Partial", OffsetText = "0x01820000" },
            new() { Name = "photo_20240518.jpg", Type = "JPEG Image", SizeText = "4.8 MB", Size = 5033216, Modified = "2025-05-18 20:31", Status = "Good", OffsetText = "0x0005A000" },
            new() { Name = "presentation_deck.pptx", Type = "PowerPoint Presentation", SizeText = "14.1 MB", Size = 14784921, Modified = "2025-05-15 14:02", Status = "Good", OffsetText = "0x00412000" },
            new() { Name = "client_archive.zip", Type = "Compressed Archive", SizeText = "86.7 MB", Size = 90912358, Modified = "2025-05-10 16:45", Status = "Good", OffsetText = "0x00A50000" },
            new() { Name = "screen_recording_01.mp4", Type = "MPEG-4 Video", SizeText = "342.0 MB", Size = 358612992, Modified = "2025-05-02 22:11", Status = "Corrupt", OffsetText = "0x03100000" },
            new() { Name = "system_cache.dat", Type = "System Data File", SizeText = "512 KB", Size = 524288, Modified = "2025-04-28 08:19", Status = "Good", OffsetText = "0x00010000" },
            new() { Name = "notes_meeting.txt", Type = "Text Document", SizeText = "12 KB", Size = 12288, Modified = "2025-04-20 10:05", Status = "Good", OffsetText = "0x00008000" }
        };

        ApplyFilter();

        // Select the JPEG image by default to match screenshot detail preview
        var defaultSelected = _files.FirstOrDefault(f => f.Name == "photo_20240518.jpg") ?? _files.FirstOrDefault();
        if (defaultSelected != null)
        {
            FileGrid.SelectedItem = defaultSelected;
        }
    }

    public void ApplyFilter()
    {
        _files.Clear();
        IEnumerable<FileRow> query = _masterFiles;

        if (_activeCategory == "Recovery Candidates")
        {
            query = query.Where(f => f.Status == "Partial" || f.Status == "Corrupt" || f.Name.Contains("backup") || f.Name.Contains("recording"));
        }
        else if (_activeCategory == "File Carver")
        {
            query = query.Where(f => f.Type != "Folder");
        }

        foreach (var item in query)
        {
            _files.Add(item);
        }

        PaginationInfoText.Text = $"1 - {_files.Count} of 1,248 files (312.6 GB)";
    }

    public void SetFiles(IEnumerable<FileRow> files)
    {
        _masterFiles = files.ToList();
        ApplyFilter();
    }

    private void FileGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FileGrid.SelectedItem is FileRow row)
        {
            FileSelected?.Invoke(this, row);
        }
    }

    private void FileGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FileGrid.SelectedItem is FileRow row)
        {
            FileDoubleClicked?.Invoke(this, row);
        }
    }

    private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem tvi && tvi.Header is StackPanel sp)
        {
            var text = sp.Children.OfType<TextBlock>().FirstOrDefault()?.Text;
            FolderSelected?.Invoke(this, new ExplorerNode(text ?? "Folder", null, false));
        }
    }

    private void FolderTree_Expanded(object sender, RoutedEventArgs e)
    {
    }

    private void SubTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Content is string tabName)
        {
            _activeCategory = tabName;
            ApplyFilter();
            SubTabSelected?.Invoke(this, tabName);
        }
    }

    private void PrevPage_Click(object sender, RoutedEventArgs e)
    {
    }

    private void NextPage_Click(object sender, RoutedEventArgs e)
    {
    }

    private void LastPage_Click(object sender, RoutedEventArgs e)
    {
    }
}
