using System;
using XForensics.Core.Analyzers.Signatures;
using XForensics.Core.FileSystem;
using XForensics.Core.Database;

namespace XForensics.App.Models;

public sealed record FileRow
{
    public DirectoryEntry? Entry { get; init; }
    public DatabaseFile? DatabaseFile { get; init; }
    public FileSignature? Signature { get; init; }
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public long Size { get; init; }
    public string SizeText { get; init; } = "--";
    public string Modified { get; init; } = "--";
    public string Created { get; init; } = "--";
    public string OffsetText { get; init; } = "--";
    public string Status { get; init; } = "Good";
    public int Ranking { get; init; }
    public int Collisions { get; init; }
    public bool IsChecked { get; set; }

    public bool HasStatusDot => Status is "Good" or "Partial" or "Corrupt";

    public string StatusDotColor => Status switch
    {
        "Good" => "#10B981",
        "Partial" => "#F59E0B",
        "Corrupt" => "#EF4444",
        _ => "#70829B"
    };

    public string IconKey => Type switch
    {
        "Directory" or "Folder" => "IconFolder",
        _ when Name.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) => "IconWord",
        _ when Name.EndsWith(".psd", StringComparison.OrdinalIgnoreCase) => "IconPhotoshop",
        _ when Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase) => "IconDatabase",
        _ when Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
               Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) => "IconFileImage",
        _ when Name.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase) => "IconPpt",
        _ when Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) => "IconZip",
        _ when Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) => "IconVideo",
        _ when Name.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) => "IconGear",
        _ => "IconFile"
    };

    public string IconColor => Type switch
    {
        "Directory" or "Folder" => "#F59E0B",
        _ when Name.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) => "#3B82F6",
        _ when Name.EndsWith(".psd", StringComparison.OrdinalIgnoreCase) => "#38BDF8",
        _ when Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase) => "#06B6D4",
        _ when Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
               Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) => "#818CF8",
        _ when Name.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase) => "#F97316",
        _ when Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) => "#EAB308",
        _ when Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) => "#A855F7",
        _ when Name.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) => "#64748B",
        _ => "#94A3B8"
    };
}
