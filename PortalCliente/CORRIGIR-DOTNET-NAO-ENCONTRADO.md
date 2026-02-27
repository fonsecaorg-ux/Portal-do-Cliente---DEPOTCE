# "dotnet" não é reconhecido — como corrigir

O erro **"O termo 'dotnet' não é reconhecido"** significa que o .NET SDK não está no PATH do PowerShell (ou não está instalado).

---

## Opção 1: Descobrir onde está o dotnet

Na pasta do projeto, execute:

```powershell
.\Scripts\Encontrar-Dotnet.ps1
```

Se o script encontrar `dotnet.exe`, ele mostra o caminho e um comando para usar **nesta sessão**:

```powershell
$env:Path = "C:\Program Files\dotnet;$env:Path"
dotnet run
```

(Use o caminho que o script mostrar.)

---

## Opção 2: Adicionar ao PATH permanentemente

1. **Encontre a pasta do dotnet** (geralmente `C:\Program Files\dotnet`).
2. **Configurar PATH:**
   - Tecla Windows → digite "variáveis de ambiente" → **Editar as variáveis de ambiente do sistema**
   - Em **Variáveis do sistema**, selecione **Path** → **Editar** → **Novo**
   - Adicione: `C:\Program Files\dotnet`
   - OK em todas as janelas.
3. **Feche e abra de novo** o PowerShell (ou o terminal do Cursor) e teste: `dotnet --version`.

---

## Opção 3: Instalar o .NET SDK

Se o dotnet não existir no PC:

1. Acesse: **https://dotnet.microsoft.com/download**
2. Baixe e instale o **.NET SDK** (não só o Runtime) para a versão que o projeto usa (ex.: .NET 10 ou 8).
3. Reinicie o PowerShell (ou o PC) e teste: `dotnet --version`.

---

## Opção 4: Rodar pelo Visual Studio / Cursor

- Abra a **solução** `PortalCliente.sln` no Visual Studio ou no Cursor.
- Para o **Simulador MBM:** clique com o botão direito no projeto **SimuladorMBM** → **Definir como Projeto de Inicialização** → F5 (ou Run).
- Para o **Portal:** defina **PortalCliente** como projeto de inicialização e execute.

Assim o ambiente da IDE usa o dotnet que ela já conhece, sem depender do PATH do terminal.
