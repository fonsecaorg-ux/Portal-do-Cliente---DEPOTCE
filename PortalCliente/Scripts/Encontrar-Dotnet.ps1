# Procura dotnet.exe e mostra como usar ou adicionar ao PATH
$caminhos = @(
    "${env:ProgramFiles}\dotnet\dotnet.exe",
    "${env:ProgramFiles(x86)}\dotnet\dotnet.exe",
    "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
)
foreach ($p in $caminhos) {
    if (Test-Path $p) {
        Write-Host "Encontrado: $p" -ForegroundColor Green
        Write-Host ""
        Write-Host "Para usar NESTA sessao do PowerShell, execute:" -ForegroundColor Cyan
        Write-Host "  `$env:Path = `"$(Split-Path $p);`$env:Path`"" -ForegroundColor White
        Write-Host ""
        Write-Host "Depois rode: dotnet run" -ForegroundColor Cyan
        exit 0
    }
}
Write-Host "dotnet.exe nao encontrado nos caminhos padrao." -ForegroundColor Yellow
Write-Host ""
Write-Host "Opcoes:" -ForegroundColor Cyan
Write-Host "1) Instale o .NET SDK: https://dotnet.microsoft.com/download" -ForegroundColor White
Write-Host "2) Se usa Visual Studio, abra o projeto pela solucao (PortalCliente.sln) e execute por la (F5)." -ForegroundColor White
Write-Host "3) Apos instalar, feche e reabra o PowerShell (ou reinicie o PC) para o PATH ser atualizado." -ForegroundColor White
