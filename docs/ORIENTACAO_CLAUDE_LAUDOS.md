# Orientações para o Claude — Gerar 10 laudos de exemplo (EIR / Laudo de Vistoria)

## Contexto

- **Sistema:** Portal do Cliente Depotce — o cliente consulta isotanques e documentos (laudos de vistoria e certificados de lavagem) que vêm do MBM (sistema de gestão do pátio).
- **Objetivo:** Gerar **10 laudos em PDF** (tipo EIR — Equipment Interchange Receipt / Laudo de Vistoria de entrada) para os isotanques listados abaixo, para usarmos como **dados de exemplo** no portal. Os arquivos serão colocados em uma pasta do projeto (ex.: `PortalCliente/wwwroot/docs/laudos/`) e o portal exibirá links para “Abrir / download”.

## Lista dos 10 isotanques (já existentes no nosso MBM)

| # | Código      | Produto        | Cliente        |
|---|-------------|----------------|----------------|
| 1 | DHDU1274480 | Etanol         | DEN HARTOGH    |
| 2 | DHDL2272373 | Metanol        | DEN HARTOGH    |
| 3 | DHDU2273512 | Ácido acético  | DEN HARTOGH    |
| 4 | DHDL3399881 | Ácido acético  | DEN HARTOGH    |
| 5 | DHDU4455667 | Tolueno        | DEN HARTOGH    |
| 6 | EXFU5567363 | Etanol         | Empresa Alpha  |
| 7 | EXFU6422402 | Metanol        | Empresa Alpha  |
| 8 | EXFU7711223 | Hexano         | Empresa Alpha  |
| 9 | SEDU2233445 | Etanol         | Química Beta   |
|10 | SEDU7788990 | Hexano         | Química Beta   |

## O que gerar

- **10 documentos em PDF**, um por isotank acima.
- **Tipo:** Laudo de Vistoria (EIR — Equipment Interchange Receipt), como o que um pátio/terminal gera na entrada do equipamento (condição do isotank, data, responsável, observações).
- **Conteúdo sugerido em cada PDF:**
  - Título: **Laudo de Vistoria (EIR)** — Código do isotank (ex.: DHDU1274480)
  - Cliente e produto (última carga)
  - Data da vistoria (pode ser uma data recente de exemplo)
  - Condição geral (ex.: “Bom estado”, “Lacre íntegro”, “Sem avarias aparentes” ou alguma observação leve)
  - Nome do vistoriador / responsável (ex.: “Depotce – Pátio” ou fictício)
  - Rodapé: “Portal do Cliente Depotce – Documento de exemplo”

## Formato dos arquivos

- **Nome do arquivo:** um PDF por isotank, com nome fixo para podermos referenciar no portal, por exemplo:
  - `DHDU1274480_EIR.pdf`
  - `DHDL2272373_EIR.pdf`
  - `DHDU2273512_EIR.pdf`
  - `DHDL3399881_EIR.pdf`
  - `DHDU4455667_EIR.pdf`
  - `EXFU5567363_EIR.pdf`
  - `EXFU6422402_EIR.pdf`
  - `EXFU7711223_EIR.pdf`
  - `SEDU2233445_EIR.pdf`
  - `SEDU7788990_EIR.pdf`

- **Formato:** PDF (tamanho e layout simples, tipo uma página A4 por laudo).

## Pedido direto ao Claude

**“Gere 10 laudos em PDF (Laudo de Vistoria / EIR) para os 10 isotanques da tabela acima. Cada PDF deve ter o nome exatamente como na lista de nomes de arquivo (ex.: DHDU1274480_EIR.pdf). Inclua no conteúdo do laudo: título EIR, código do isotank, cliente, produto (última carga), data da vistoria, condição geral e responsável. Os arquivos serão usados como exemplo no Portal do Cliente Depotce para exibir links de download na tela de Documentos e no Detalhe do isotank.”**

---

Depois que os 10 PDFs forem gerados, eles podem ser salvos em:
- `PortalCliente/wwwroot/docs/laudos/`

E no seed ou na API do MBM (ou no banco do Portal), os campos **UrlLaudoVistoria** desses isotanques podem apontar para:
- `/docs/laudos/DHDU1274480_EIR.pdf`
- `/docs/laudos/DHDL2272373_EIR.pdf`
- … e assim por diante.

Assim a tela **Documentos** e o card de documentos no **Detalhe** passarão a exibir os links “Abrir / download” para esses laudos de exemplo.
