# Resumo do projeto — Portal do Cliente Depotce (para contexto ao Claude)

Documento para explicar **desde o ponto zero** o que estamos desenvolvendo e o que já foi feito. Use este texto para dar contexto ao Claude ou a qualquer pessoa que for continuar o projeto.

---

## 1. Objetivo do produto

- **Portal do Cliente** é um site para **clientes da Depotce** consultarem informações sobre **isotanques/containers** de produtos químicos.
- Objetivo de negócio: o cliente **não precisa acionar o time** (menos WhatsApp, menos solicitações). Ele acessa o portal e vê sozinho: estoque, status de cada isotanque, previsão de liberação, etc.
- O portal é **somente leitura**: não há cadastro/edição/exclusão pelo cliente. Os dados vêm de um sistema base chamado **MBM**.

---

## 2. Origem dos dados (MBM)

- **MBM** = sistema base da operação (onde ficam os dados reais de isotanques, clientes, status).
- O portal **consome** esses dados (no futuro via API ou integração). Hoje não temos o MBM real, então criamos um **Simulador MBM** em forma de API REST para desenvolver e testar.

---

## 3. O que foi feito desde o ponto zero

### 3.1 Portal do Cliente (ASP.NET Core MVC)

- **Stack:** .NET 10, ASP.NET Core MVC, Entity Framework Core, SQLite (banco local quando não usa MBM).
- **Telas:**
  - **Lista de isotanques:** filtros por cliente, status, busca por código/produto; ordenação por coluna; paginação (10/20/50 itens); exibição do **estoque total**.
  - **Detalhe do isotanque:** código, cliente, produto, status, previsão de liberação, **dias no status** (como no BI), e quando existir: descarregado no pátio, carregado no veículo, previsão no terminal, foto.
  - **Relatórios BI:** página que incorpora um iframe do Power BI (ex.: “Isotank Vazio - Dias no Status”); URL configurável em `appsettings.json`.
- **Indicador de origem:** no menu aparece “● Dados: MBM” (verde) quando o portal está consumindo a API do MBM, ou “● Dados: Local” (cinza) quando usa só o banco SQLite local.
- **Modelo de dados (Container):** Id, Codigo, Produto, Cliente, Status, DiasNoStatus, PrevisaoLiberacao, DataHoraDescarregadoPatio, DataHoraCarregadoVeiculo, PrevisaoChegadaTerminal, UrlFoto. Tudo compatível com o que o MBM (ou simulador) pode enviar.

### 3.2 Simulador MBM (API REST)

- **Objetivo:** simular o sistema base MBM para desenvolvimento e testes sem o MBM real.
- **Stack:** .NET 10, ASP.NET Core Web API, EF Core, SQLite (`mbm.db`).
- **Modelo “completo”:** Clientes, Produtos, StatusIsotanque, Isotanque (com DataInicioStatus para calcular “dias no status”, além de campos opcionais de logística como descarregado no pátio, carregado no veículo, previsão terminal, URL da foto).
- **Endpoints:**  
  `GET /api/isotanques`, `GET /api/isotanques/{codigo}`, `GET /api/clientes`, `GET /api/status`.  
  Raiz `/` e `/painel` em HTML para explicar a API e mostrar um painel resumido (totais e amostra de isotanques).
- **Porta:** 5050. O portal é configurado em `appsettings.json` com `MBM:BaseUrl = http://localhost:5050` para consumir essa API.

### 3.3 Integração Portal ↔ MBM

- **Serviço de origem dos dados:** `IOrigenDadosService` com duas implementações:
  - **LocalOrigenDadosService:** lê do SQLite do portal (seed local). Usado quando `MBM:BaseUrl` está vazio.
  - **MbmOrigenDadosService:** chama a API do MBM (simulador ou real) via `HttpClient`. Usado quando `MBM:BaseUrl` está preenchido.
- O portal não “sabe” se está falando com o simulador ou com o MBM real; só consome a mesma API.

### 3.4 Alinhamento com o BI (Power BI)

- Foi usada como referência a visão **“ISOTANK VAZIO - Dias no Status”** do Power BI (Depotce/Cedata).
- O que espelhamos no portal: **status** (Off Hire, Envio Estimativa, Limpeza, Reparo, Inspeção), **dias no status** por isotanque, contagem por cliente. No portal: coluna “Dias no status” na lista, com destaque (verde 0 dias, vermelho 10+ dias) e mesma informação no detalhe.

### 3.5 Ajustes e correções feitos no caminho

- **.NET SDK:** o `dotnet` não era reconhecido no PowerShell; orientação para instalar o SDK e/ou usar novo terminal após a instalação.
- **Simulador MBM:** página inicial em HTML (em vez de só JSON) e painel `/painel` para visualizar os dados.
- **Raw string no Program.cs (Simulador):** erro de compilação com `$"""` e muitas `{{` no CSS; corrigido com `$$"""` e chaves ajustadas.
- **Portal compilando a pasta SimuladorMBM:** erro de “apenas uma unidade com top-level statements”. Resolvido excluindo `SimuladorMBM\**` da compilação no `PortalCliente.csproj`.
- **Views Create/Edit/Delete:** referência a `Model.Data` (propriedade inexistente em `Container`); corrigido para `PrevisaoLiberacao`.
- **Erro 500 ao chamar o MBM:** quando a API do MBM falha (ex.: simulador parado ou banco antigo), o portal passou a exibir mensagem clara: verificar se o Simulador MBM está rodando e, se já rodou antes, apagar `mbm.db` e rodar de novo.

---

## 4. Como rodar hoje

1. **Simulador MBM:**  
   `cd SimuladorMBM` → `dotnet run`.  
   Fica em http://localhost:5050. Se já existir `mbm.db` de uma versão antiga, apague para recriar com as colunas novas (ex.: DataInicioStatus).
2. **Portal do Cliente:**  
   Na raiz do repositório (pasta `PortalCliente`): `dotnet run`.  
   Abre em http://localhost:5187. Com `MBM:BaseUrl` em `appsettings.json` apontando para 5050, o portal usa os dados do simulador e mostra “● Dados: MBM”.

---

## 5. Próximos passos (sugeridos no README)

- Integração com o **MBM real** (quando houver API ou exportação).
- Definir como as telas/relatórios do BI serão incorporados (iframe, exportação estática, etc.).
- **Login** e isolamento por cliente (cada cliente logado vê só seus isotanques).
- **Fase 2 — Suporte a Containers:** hoje só isotanques; num segundo momento incluir **containers** (conteineres secos/carga geral). Mesmo portal, menu ou abas “Isotanques” e “Containers”; MBM expõe também containers (ex.: `GET /api/containers`); portal consome e exibe. Nada a alterar na implementação atual.

---

## 6. Estrutura resumida do repositório

- **PortalCliente/** – projeto MVC do portal (Controllers, Views, Models, Services, Data).
- **SimuladorMBM/** – projeto da API que simula o MBM (Controllers, Models, Data, Program.cs com rotas e painel).
- **Scripts/** – ex.: script PowerShell para subir MBM e Portal.
- **README.md**, **COMO-TESTAR-MBM.md**, **CORRIGIR-DOTNET-NAO-ENCONTRADO.md** – instruções e troubleshooting.

Use este resumo para dar ao Claude (ou a outro dev) o contexto completo do que é o produto e o que já foi implementado.
