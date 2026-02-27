# Como testar o MBM e ver o Portal refletindo ele

## Objetivo

Subir o **Simulador MBM** (API na porta 5050) e o **Portal do Cliente** (porta 5187). O Portal consome os dados da API e você vê tudo refletido na tela.

---

## Opção 1: Script (recomendado)

Na **pasta do projeto** (onde está `PortalCliente.csproj`), execute:

```powershell
.\Scripts\Subir-MBM-e-Portal.ps1
```

- Abre uma **nova janela** com o Simulador MBM.
- Depois de alguns segundos, inicia o **Portal** nesta janela e abre o navegador.
- No menu do Portal deve aparecer **● Dados: MBM** em verde = dados vindo da API do MBM.
- Para encerrar: feche o Portal (Ctrl+C nesta janela) e depois feche a janela do Simulador.

Se der erro de execução de script, rode antes:

```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

---

## Opção 2: Manual (dois terminais)

### Terminal 1 – Simulador MBM

```powershell
cd SimuladorMBM
dotnet run
```

Deixe rodando. A API fica em **http://localhost:5050**.

Para testar a API direto: abra no navegador ou use:

```powershell
Invoke-RestMethod -Uri "http://localhost:5050/api/isotanques" | ConvertTo-Json -Depth 5
```

### Terminal 2 – Portal do Cliente

Na **raiz do projeto** (pasta `PortalCliente`):

```powershell
dotnet run
```

Abra **http://localhost:5187** (ou a URL que aparecer no terminal).

---

## Como saber se o Portal está usando o MBM

1. **Indicador no menu:** no topo da página, ao lado de “Isotanques”, deve aparecer **● Dados: MBM** em verde. Se aparecer **● Dados: Local** em cinza, o Portal está usando o banco local (e o `MBM:BaseUrl` no `appsettings.json` está vazio ou o Simulador não está rodando).
2. **Teste de reflexo:** na lista de isotanques, use o filtro “Ver como cliente” (ex.: DEN HARTOGH). Os dados vêm do MBM; são os mesmos do seed do Simulador (22 isotanques, 3 clientes). Abra um detalhe por código (ex.: `DHDU1274480`) e confira se as informações batem com o que a API retorna em **http://localhost:5050/api/isotanques/DHDU1274480**.

---

## Configuração

- **Portal usando MBM:** em `appsettings.json` do Portal, a seção `MBM` deve ter `"BaseUrl": "http://localhost:5050"`.
- **Portal só com banco local:** remova o valor ou deixe `"BaseUrl": ""`. O indicador no menu mostrará **● Dados: Local**.

---

## Banco do MBM (novos campos)

Se você **já rodou o Simulador MBM antes** e foram adicionados campos novos (foto, descarregado no pátio, carregado no veículo, previsão terminal), apague o arquivo **`SimuladorMBM/mbm.db`** e rode o Simulador de novo. O banco será recriado com a estrutura atual.

## Resumo rápido

| O que              | URL / Ação                          |
|--------------------|-------------------------------------|
| API do MBM         | http://localhost:5050               |
| Painel MBM         | http://localhost:5050/painel        |
| Lista isotanques   | http://localhost:5050/api/isotanques |
| Portal             | http://localhost:5187              |
| Indicador de origem| Menu: “● Dados: MBM” ou “● Dados: Local” |
