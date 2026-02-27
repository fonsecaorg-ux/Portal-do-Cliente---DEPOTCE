# Simulador MBM

API REST que **simula o sistema base MBM** para o Portal do Cliente Depotce. Os dados reais do portal virão do MBM; este projeto permite desenvolver e testar a integração sem o MBM real.

## Como rodar

```bash
cd SimuladorMBM
dotnet run
```

A API sobe em **http://localhost:5050**.

## Endpoints

| Método | URL | Descrição |
|--------|-----|-----------|
| GET | `/api/isotanques` | Lista isotanques (query: `cliente`, `status`, `busca`) |
| GET | `/api/isotanques/{codigo}` | Isotanque por código |
| GET | `/api/clientes` | Lista de nomes de clientes |
| GET | `/api/status` | Lista de status (Ag. Limpeza, etc.) |
| GET | `/` | Resumo da API em JSON |

## Modelo (MBM “completo”)

- **Clientes** – código, nome, e-mail, ativo  
- **Produtos** – código, nome, classe de risco  
- **StatusIsotanque** – cadastro de etapas (Ag. Off Hire, Ag. Limpeza, etc.)  
- **Isotanque** – código, produto, cliente, status, previsão de liberação, data de entrada, última atualização  

O banco SQLite (`mbm.db`) é criado na primeira execução e populado com dados de exemplo (mesmos clientes/isotanques do seed do Portal).

## Uso com o Portal

1. Inicie o **Simulador MBM** (porta 5050).
2. No Portal, em `appsettings.json`, deixe `MBM:BaseUrl` como `http://localhost:5050`.
3. Inicie o **Portal do Cliente**. Ele passará a consumir os dados da API do MBM.

Para usar só o banco local do Portal (sem MBM), remova ou deixe vazio `MBM:BaseUrl` no `appsettings.json` do Portal.
