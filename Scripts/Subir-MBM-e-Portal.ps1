# Sobe o Simulador MBM e depois o Portal do Cliente.
# Execute na pasta raiz do repositório (PortalCliente).

$raiz = $PSScriptRoot | Split-Path -Parent
Set-Location $raiz

Write-Host "=== Portal do Cliente + Simulador MBM ===" -ForegroundColor Cyan
Write-Host ""

# 1) Inicia o Simulador MBM em nova janela
Write-Host "[1/3] Iniciando Simulador MBM (porta 5050)..." -ForegroundColor Yellow
$mbmPath = Join-Path $raiz "SimuladorMBM"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$mbmPath'; Write-Host 'Simulador MBM - feche esta janela para encerrar' -ForegroundColor Green; dotnet run"
Start-Sleep -Seconds 5

# 2) Verifica se a API responde
try {
    $resp = Invoke-WebRequest -Uri "http://localhost:5050/" -UseBasicParsing -TimeoutSec 3
    Write-Host "[2/3] MBM respondendo OK." -ForegroundColor Green
} catch {
    Write-Host "[2/3] AVISO: MBM ainda nao respondeu. Aguarde mais alguns segundos ou verifique a janela do Simulador." -ForegroundColor Yellow
}

Write-Host "[3/3] Iniciando Portal do Cliente (abre no navegador)..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Quando o Portal abrir, confira no menu: deve aparecer 'Dados: MBM' em verde." -ForegroundColor Cyan
Write-Host "Para parar: feche esta janela (Portal) e depois feche a janela do Simulador MBM." -ForegroundColor Gray
Write-Host ""

dotnet run
