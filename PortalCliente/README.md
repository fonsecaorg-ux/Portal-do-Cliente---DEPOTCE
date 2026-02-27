# Portal do Cliente — Depotce

Portal web para **clientes da Depotce** consultarem informações sobre isotanques/containers de produtos químicos, **sem precisar acionar o time** (redução de solicitações e uso de WhatsApp).

## Objetivo

- Dar **autonomia ao cliente** para consultar informações básicas.
- Exibir **estoque total** do cliente.
- Exibir **etapa/status** de cada isotanque dentro da Depotce.
- Exibir **previsão de liberação** e demais informações correlatas.

## Origem dos dados

- **Base de dados:** as informações vêm do **MBM** (sistema base).
- O portal **não é** um sistema de cadastro pelo cliente: ele apenas **consulta** dados que serão alimentados/sincronizados a partir do MBM.

### Simulador MBM

O projeto **SimuladorMBM** é uma API REST que simula o MBM para desenvolvimento e testes. Ele expõe os mesmos dados (isotanques, clientes, status) e permite testar o fluxo completo sem o sistema real.

- **Como usar:** suba o SimuladorMBM na porta 5050 (`dotnet run` na pasta `SimuladorMBM`) e deixe `MBM:BaseUrl` em `appsettings.json` como `http://localhost:5050`. O Portal passará a consumir os dados da API.
- **Sem MBM:** remova ou deixe vazio `MBM:BaseUrl` para o Portal usar apenas o banco SQLite local (seed).
- Detalhes em **SimuladorMBM/README.md**.

## Abordagem de visualização

- **Telas “congeladas” do BI:** uso de visões já compiladas no BI, incorporadas ao portal.
- Objetivo: evitar envio de links soltos e manter a informação **padronizada e centralizada** em um único lugar.

## Funcionalidades atuais

- **Consulta de isotanques:** listagem com busca por código ou produto.
- **Filtro por cliente:** dropdown “Ver como cliente” para simular a visão de um cliente (DEN HARTOGH, Empresa Alpha, Química Beta). Quando houver login, o filtro será automático pelo cliente logado.
- **Filtro por status:** Ag. Off Hire, Ag. Envio Estimativa, Ag. Limpeza, Ag. Reparo, Ag. Inspeção.
- **Ordenação:** clique no cabeçalho da tabela para ordenar por Código, Produto, Cliente, Status ou Previsão de liberação (asc/desc).
- **Paginação:** 20 itens por página (padrão); é possível alterar para 10, 20 ou 50. Navegação por Anterior/Próximo e número da página. Filtros e ordenação são mantidos ao trocar de página.
- **Estoque total:** quantidade de isotanques (respeitando os filtros).
- **Detalhe do isotanque:** visualização por código (cliente, produto, etapa/status, previsão de liberação).
- **Relatórios BI:** página que incorpora uma visão do BI em iframe (ex.: Isotank Vazio - Dias no Status). A URL é configurável em `appsettings.json`.

### Configurar a visão do BI

No `appsettings.json`, na seção `"Bi"`:

- **IsotankVazioUrl:** URL pública do relatório para incorporação (ex.: link de incorporação do Power BI, Tableau ou do BI em uso). Se vazio, a página exibe instruções.
- **TituloIsotankVazio:** Título exibido acima do iframe (ex.: "Isotank Vazio - Dias no Status").

O relatório precisa permitir incorporação em iframe (configuração do próprio BI).

## Stack

- ASP.NET Core MVC
- Entity Framework Core
- SQLite (banco local; em produção os dados virão do MBM)

**Observação:** O banco é criado automaticamente na primeira execução (`EnsureCreated()`). Se você alterar o modelo (ex.: trocar colunas) e já tiver um `portalcliente.db` antigo, **apague o arquivo** `portalcliente.db` na pasta do projeto para que ele seja recriado com a estrutura nova, ou use migrations.

## Próximos passos sugeridos

1. **Integração MBM:** definir como os dados do MBM serão enviados ao portal (API, exportação, sincronização periódica).
2. **Incorporação do BI:** definir como as telas/relatórios do BI serão “congeladas” e exibidas no portal (iframe, exportação estática, etc.).
3. **Login e isolamento por cliente (futuro):**
   - **Login:** autenticação para que apenas usuários autorizados acessem o portal.
   - **Filtro por cliente:** cada cliente logado deve ver **apenas os seus isotanques** (e estoque total referente a ele). Os dados precisarão estar associados a um cliente (ex.: coluna ou tabela Cliente no modelo / no MBM) para filtrar por usuário logado.
4. **Fase 2 — Suporte a Containers (além de Isotanks):**
   - Hoje o portal trata apenas **isotanques** (ISO tanks para líquidos). Num segundo momento pode-se incluir **containers** (conteineres secos / carga geral).
   - **Abordagem sugerida:** mesmo portal, com menu ou abas “Isotanques” e “Containers”; cada tipo com sua lista e detalhe (e no MBM, suas tabelas ou endpoints). O MBM passaria a expor também containers (ex.: `GET /api/containers` ou um `tipo` em um endpoint único); o portal só consome e exibe.
   - Nada precisa ser alterado na implementação atual; isso fica como evolução futura.
