using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XForensics.Core.Hardware;

namespace XForensics.App.Controls;

public partial class DriveToolbar : UserControl
{
    public event EventHandler? StartScanRequested;
    public event EventHandler? ScanOptionsRequested;
    public event EventHandler? CloseTabRequested;
    public event EventHandler<PhysicalDeviceInfo>? DriveSelected;

    private bool _isScanning;

    public DriveToolbar()
    {
        InitializeComponent();
    }

    public void SetDrives(IEnumerable<PhysicalDeviceInfo> drives, PhysicalDeviceInfo? selected = null)
    {
        DriveComboBox.ItemsSource = drives;
        if (selected != null)
        {
            DriveComboBox.SelectedItem = selected;
        }
        else if (DriveComboBox.Items.Count > 0)
        {
            DriveComboBox.SelectedIndex = 0;
        }
    }

    public PhysicalDeviceInfo? GetSelectedDrive()
    {
        return DriveComboBox.SelectedItem as PhysicalDeviceInfo;
    }

    public void SetMetrics(string scanned, string found)
    {
        ScannedSizeText.Text = scanned;
        FoundFilesText.Text = found;
    }

    public void SetScanningState(bool isScanning)
    {
        _isScanning = isScanning;
        if (isScanning)
        {
            BtnText.Text = "Stop Scan";
            StartScanIcon.Data = TryFindResource("IconClose") as Geometry;
            SetStatus("Scanning...", false);
        }
        else
        {
            BtnText.Text = "Start Scan";
            StartScanIcon.Data = TryFindResource("IconPlay") as Geometry;
            SetStatus("Scan completed", true);
        }
    }

    public void SetStatus(string status, bool isCompleted = true)
    {
        StatusBadgeText.Text = status;
        if (isCompleted)
        {
            ScanStatusBadge.SetResourceReference(Border.BackgroundProperty, "StatusGoodBgBrush");
            ScanStatusBadge.SetResourceReference(Border.BorderBrushProperty, "StatusGoodBorderBrush");
            StatusBadgeIcon.SetResourceReference(System.Windows.Shapes.Path.FillProperty, "SuccessBrush");
            StatusBadgeIcon.Data = TryFindResource("IconCheckCircle") as Geometry;
        }
        else
        {
            ScanStatusBadge.SetResourceReference(Border.BackgroundProperty, "StatusWarningBgBrush");
            ScanStatusBadge.SetResourceReference(Border.BorderBrushProperty, "StatusWarningBorderBrush");
            StatusBadgeIcon.SetResourceReference(System.Windows.Shapes.Path.FillProperty, "WarningBrush");
            StatusBadgeIcon.Data = TryFindResource("IconCluster") as Geometry;
        }
    }

    private void StartScan_Click(object sender, RoutedEventArgs e)
    {
        StartScanRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ScanOptions_Click(object sender, RoutedEventArgs e)
    {
        ScanOptionsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CloseTab_Click(object sender, RoutedEventArgs e)
    {
        CloseTabRequested?.Invoke(this, EventArgs.Empty);
    }

    private void DriveComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DriveComboBox.SelectedItem is PhysicalDeviceInfo drive)
        {
            DriveSelected?.Invoke(this, drive);
        }
    }
}
