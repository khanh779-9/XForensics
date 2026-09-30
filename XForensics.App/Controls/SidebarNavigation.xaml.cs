using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XForensics.Core.Hardware;

namespace XForensics.App.Controls;

public partial class SidebarNavigation : UserControl
{
    public event EventHandler<string>? NavSelected;

    private readonly List<NavItem> _navItems = new();

    public SidebarNavigation()
    {
        InitializeComponent();
        _navItems.AddRange(new[] { NavHome, NavScan, NavCarver, NavPartition, NavCluster, NavImageTools, NavSettings });
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is NavItem clicked)
        {
            foreach (var item in _navItems)
            {
                item.IsSelected = (item == clicked);
            }
            NavSelected?.Invoke(this, clicked.Text);
        }
    }

    public void SetDeviceInfo(PhysicalDeviceInfo info)
    {
        DriveIdText.Text = info.DeviceId;
        DriveModelText.Text = info.Model;
        DriveSizeText.Text = info.FormattedSize;
        DriveSerialText.Text = string.IsNullOrWhiteSpace(info.SerialNumber) ? "N/A" : info.SerialNumber;
        DriveStatusText.Text = info.Status;
    }

    public void SetDeviceInfo(string id, string size, string model, string serial)
    {
        DriveIdText.Text = id;
        DriveSizeText.Text = size;
        DriveModelText.Text = model;
        DriveSerialText.Text = serial;
        DriveStatusText.Text = "Healthy";
    }

    public void SetHostInfo(SystemHostInfo host)
    {
        HostPcText.Text = $"{host.ComputerName} ({host.UserName})";
        HostOsText.Text = $"{host.OsDescription} \u2022 {host.TotalMemoryFormatted}";
    }

    public void SetStatus(string status, bool isReady = true)
    {
        StatusText.Text = status;
        StatusDot.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, isReady ? "SuccessBrush" : "WarningBrush");
    }
}
