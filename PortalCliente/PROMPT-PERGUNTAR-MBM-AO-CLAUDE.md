# Prompt para perguntar ao Claude sobre o que o MBM oferece

Copie o texto abaixo e cole no Claude quando quiser saber se o MBM costuma fornecer essas informações (ou como descobrir).

---

## Texto do prompt (copie da linha em diante)

Estou desenvolvendo um **Portal do Cliente** para a Depotce, onde clientes consultam informações sobre **isotanques** (e no futuro containers). Os dados vêm de um sistema base chamado **MBM** (sistema operacional da operação). O portal é somente leitura: só exibe o que o MBM (ou sua API/exportação) fornece.

Há **informações que eu gostaria de mostrar no portal**, mas **não sei se o MBM que vamos integrar oferece**. Preciso de ajuda para:

1. **Listar o que sistemas tipo MBM (gestão de pátio/isotanques/logística química) costumam expor** — seja por API, relatórios ou integração — sobre cada isotanque ou processo.
2. **Dizer quais dos itens abaixo são comuns nesse tipo de sistema** e quais costumam ser raros ou inexistentes.
3. **Sugerir como descobrir na prática** o que o nosso MBM oferece (perguntas para o fornecedor, documentação, relatórios do BI, etc.).

**Itens que eu gostaria de ter no portal (e quero saber se o MBM costuma fornecer):**

- **Foto do isotanque** (registro fotográfico do equipamento no pátio ou em etapa).
- **Logs do processo** — ex.: data/hora em que foi descarregado no pátio, data/hora em que foi carregado no veículo, mudanças de status com timestamp.
- **Previsão de chegada no terminal portuário** (ou em outro destino).
- **Histórico de status** — há quanto tempo está em cada etapa (já temos “dias no status” no portal; o que mais costuma existir?).
- **Documentos/laudos** — certificados, inspeções, laudos de limpeza (links ou referências).
- **Localização no pátio** — posição/fila (se o sistema controla isso).
- **Qualquer outro dado operacional** que esse tipo de MBM costuma ter e que faria sentido mostrar no portal do cliente.

Por favor:

- Indique **o que costuma ser padrão** nesse tipo de sistema e **o que costuma ser opcional ou raro**.
- Se souber de **nomes comerciais de MBMs ou sistemas similares** (gestão de pátio, isotanques, logística química), cite exemplos apenas para eu ter referência ao pesquisar.
- Dê **sugestões objetivas de perguntas** que eu possa fazer ao time que opera o MBM ou ao fornecedor do sistema (ex.: “O MBM registra data/hora de descarregamento no pátio?” “Há API ou exportação que inclua foto do equipamento?”).

O objetivo é eu saber o que **pedir ou esperar** do MBM na integração e o que **já podemos preparar no portal** (campos opcionais que mostramos quando o dado existir).

---

*Fim do prompt. Copie todo o bloco acima (a partir de "Estou desenvolvendo") e cole no Claude.*
