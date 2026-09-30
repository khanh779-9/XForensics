# Build and publish script for XForensics Suite
param (
    [string]$Configuration = "Release"
)

Write-Host "Building XForensics Solution ($Configuration)..." -ForegroundColor Cyan
dotnet build XForensics.slnx -c $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nPublishing XForensics.App (AnyCPU)..." -ForegroundColor Cyan
dotnet publish XForensics.App\XForensics.App.csproj -c $Configuration -o "publish" --self-contained false

Write-Host "`n[SUCCESS] XForensics build and publish completed successfully in ./publish" -ForegroundColor Green
