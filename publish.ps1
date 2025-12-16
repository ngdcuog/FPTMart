# Script Publish FPTMart Desktop App
# Chay script nay trong PowerShell de tao file .exe

# Buoc 1: Build va Publish
Write-Host "Building FPTMart..."
dotnet publish -c Release --self-contained true -p:PublishSingleFile=true -r win-x64

# Buoc 2: Hien thi duong dan output
$publishPath = "FPTMart\bin\Release\net9.0-windows\win-x64\publish"
Write-Host ""
Write-Host "====================================="
Write-Host "PUBLISH THANH CONG!"
Write-Host "====================================="
Write-Host "File .exe tai: $publishPath"
Write-Host ""
Write-Host "Cac buoc tiep theo:"
Write-Host "1. Copy file FPTMart.exe ra Desktop"
Write-Host "2. Click phai -> Tao Shortcut"
Write-Host "3. Done! Double-click de chay app"
Write-Host ""

# Mo folder publish
explorer $publishPath
