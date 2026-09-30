using System;

namespace XForensics.Core.Hardware;

public class PhysicalDeviceInfo
{
    public string DeviceId { get; set; } = string.Empty;
    public string Model { get; set; } = "Generic Storage Device";
    public long SizeInBytes { get; set; }
    public string FormattedSize { get; set; } = "0 GB";
    public string SerialNumber { get; set; } = "N/A";
    public string InterfaceType { get; set; } = "Physical";
    public int PartitionsCount { get; set; }
    public string Status { get; set; } = "Healthy";
    public bool IsImageFile { get; set; }
    public string? ImagePath { get; set; }

    public string DisplayName => IsImageFile
        ? $"[Image] {Model} ({FormattedSize})"
        : $"{DeviceId} - {Model} ({FormattedSize})";

    public override string ToString() => DisplayName;
}

public class SystemHostInfo
{
    public string ComputerName { get; set; } = Environment.MachineName;
    public string UserName { get; set; } = Environment.UserName;
    public string OsDescription { get; set; } = "Windows";
    public int ProcessorCount { get; set; } = Environment.ProcessorCount;
    public string TotalMemoryFormatted { get; set; } = "Unknown RAM";
}
