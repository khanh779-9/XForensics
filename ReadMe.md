# XForensics

**XForensics** is a modern, high-performance storage forensics and data recovery suite designed for analyzing, mounting, and carving filesystems, disk images, and physical storage hardware.

---

## Architecture Overview

The solution is divided into 2 clean, decoupled projects:

1. **`XForensics.Core`**:
   - Filesystem Parser (`Volume`, `DirectoryEntry`, cluster chains, allocation tables).
   - Deep Carvers & Analyzers (`FileCarver`, `MetadataAnalyzer`, `IntegrityAnalyzer`, file signature database).
   - Disk IO Handlers (`PhysicalDisk`, `RawImage`, `CompressedImage`, `WinApi`).
   - Hardware & System Diagnostics (`SystemDeviceService`, WMI drive probes, PC host diagnostics).
   - Recovery Pipeline (`RecoveryEngine`).

2. **`XForensics.App`**:
   - Windows 11 Fluent chromeless window (`XForensicsWindow`).
   - Light & Dark theme support with runtime hot-switching.
   - Live hardware selection, storage metrics, interactive cluster heatmap, and file explorer.

---

## Building the Solution

### Prerequisites
- .NET 8.0 SDK or later
- Windows 10 / 11

### Build via Command Line
```powershell
dotnet build XForensics.slnx -c Release
```

### Publish
```powershell
./build-xforensics.ps1
```
The published binaries will be generated inside the `./publish` directory.
