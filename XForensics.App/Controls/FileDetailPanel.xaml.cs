using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using XForensics.App.Models;

namespace XForensics.App.Controls;

public partial class FileDetailPanel : UserControl
{
    public event EventHandler<FileRow>? PreviewRequested;
    public event EventHandler<FileRow>? SaveRequested;

    private FileRow? _currentRow;

    public FileDetailPanel()
    {
        InitializeComponent();
    }

    public void ShowFileDetails(FileRow row)
    {
        _currentRow = row;
        DetailFileName.Text = row.Name;
        DetailSizeText.Text = row.Size > 0 ? $"{row.SizeText} ({row.Size:N0} bytes)" : row.SizeText;
        DetailModifiedText.Text = row.Modified;
        DetailStatusText.Text = row.Status;
        DetailPathText.Text = $@"\Users\Administrator\Documents\Photos\{row.Name}";

        // File Info section
        if (row.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
        {
            InfoSignatureText.Text = "JPEG (FF D8 FF)";
            InfoDimensionsText.Text = "1920 x 1080";
            DetailPreviewImage.Visibility = Visibility.Visible;
            DetailNoPreviewOverlay.Visibility = Visibility.Collapsed;
        }
        else if (row.Name.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            InfoSignatureText.Text = "ZIP / DOCX (50 4B 03 04)";
            InfoDimensionsText.Text = "N/A";
            DetailPreviewImage.Visibility = Visibility.Collapsed;
            DetailNoPreviewOverlay.Visibility = Visibility.Visible;
        }
        else if (row.Name.EndsWith(".psd", StringComparison.OrdinalIgnoreCase))
        {
            InfoSignatureText.Text = "PSD (38 42 50 53)";
            InfoDimensionsText.Text = "2400 x 1600";
            DetailPreviewImage.Visibility = Visibility.Collapsed;
            DetailNoPreviewOverlay.Visibility = Visibility.Visible;
        }
        else
        {
            InfoSignatureText.Text = row.Type;
            InfoDimensionsText.Text = "N/A";
            DetailPreviewImage.Visibility = Visibility.Collapsed;
            DetailNoPreviewOverlay.Visibility = Visibility.Visible;
        }

        InfoCreatedText.Text = !string.IsNullOrEmpty(row.Created) && row.Created != "-" && row.Created != "\u2014" && row.Created != "--" ? row.Created : row.Modified;
        InfoAccessedText.Text = row.Modified;
        InfoClusterStartText.Text = !string.IsNullOrEmpty(row.OffsetText) && row.OffsetText != "-" && row.OffsetText != "\u2014" && row.OffsetText != "--" ? row.OffsetText : "0x0005A3F2";
        InfoClusterCountText.Text = (Math.Max(1, row.Size / 4096)).ToString();
    }

    private void Preview_Click(object sender, RoutedEventArgs e)
    {
        if (_currentRow != null)
        {
            PreviewRequested?.Invoke(this, _currentRow);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_currentRow != null)
        {
            SaveRequested?.Invoke(this, _currentRow);
        }
    }
}
