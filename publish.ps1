# Script đóng gói ứng dụng CRAI tự động trên Windows (PowerShell)
Write-Host "Dang dong goi ung dung CRAI..." -ForegroundColor Cyan

# 1. Tat tien trinh cu dang chay neu co de tranh lock file
Write-Host "Dang kiem tra va dong cac tien trinh dang chay..." -ForegroundColor Gray
Stop-Process -Name "Crai.Desktop" -ErrorAction SilentlyContinue

# 2. Publish duoi dang Single-File (tat ca gom vao 1 file .exe duy nhat)
dotnet publish src/Crai.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -o ./app

if ($LASTEXITCODE -eq 0) {
    # 3. Copy logo ra ngoai thu muc app de nguoi dung de nhan dien
    Copy-Item "src/Crai.Desktop/Assets/avalonia-logo.ico" -Destination "./app/logo.ico" -Force
    # 4. Don dep cac file .pdb thua
    Remove-Item -Path "./app/*.pdb" -Force -ErrorAction SilentlyContinue
    Write-Host "`n[SUCCESS] Ung dung da duoc dong goi thanh cong tai thu muc ./app" -ForegroundColor Green
    Write-Host "-> File chay duy nhat: app/Crai.Desktop.exe (Da duoc nhung logo)" -ForegroundColor Yellow
    Write-Host "-> File logo sao chep: app/logo.ico" -ForegroundColor Yellow
} else {
    Write-Host "`n[ERROR] Dong goi that bai. Vui long kiem tra loi phia tren." -ForegroundColor Red
}
