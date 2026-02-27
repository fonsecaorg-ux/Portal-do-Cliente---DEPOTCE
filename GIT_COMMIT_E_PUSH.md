# Commit e push para o GitHub

## ⚠️ Primeira vez: ordem obrigatória

Se **qualquer** comando git der `fatal: not a git repository`, é porque o repositório ainda **não foi criado**. Rode os comandos **nesta ordem** (não pule o passo 2):

```powershell
cd "c:\Users\diego.fonseca\OneDrive - ceslog\Documentos\Portal do Cliente\PortalCliente"
git init
git add .
git status
git commit -m "Portal do Cliente: versão inicial"
git remote add origin https://github.com/SEU_USUARIO/NOME_DO_REPO.git
git branch -M main
git push -u origin main
```

**Troque** `SEU_USUARIO` e `NOME_DO_REPO` pela sua URL do GitHub. O **`git init`** é o que cria a pasta `.git` — sem ele, todos os outros comandos dão "not a git repository".

---

O Git **só funciona** quando o terminal está na pasta certa. O Cursor/VS costuma abrir o terminal na pasta **"Portal do Cliente"** — aí o `git` não acha o repositório. Você precisa **entrar na pasta específica** onde está o repositório.

## Pasta correta para o Git

Use **sempre** esta pasta (onde está a solução `.sln` e o repositório):

```
c:\Users\diego.fonseca\OneDrive - ceslog\Documentos\Portal do Cliente\PortalCliente
```

Ou seja: dentro de **"Portal do Cliente"** existe a pasta **"PortalCliente"** — é **nessa pasta interna** que você deve rodar os comandos git.

## 1. Entrar na pasta correta (obrigatório)

No PowerShell ou CMD, **antes de qualquer comando git**, execute:

```powershell
cd "c:\Users\diego.fonseca\OneDrive - ceslog\Documentos\Portal do Cliente\PortalCliente"
```

Para conferir se está na pasta certa:

```powershell
git status
```

- Se aparecer algo como "On branch ..." ou "nothing to commit" → está na pasta certa.  
- Se aparecer **"fatal: not a git repository"** → o repositório ainda não existe nessa pasta. Siga o **passo 2** e rode `git init` (só uma vez).

## 2. Criar o repositório (se der "fatal: not a git repository")

Se `git status` disser **"not a git repository"**, o repositório ainda não foi criado nessa pasta. Crie com:

```powershell
git init
```

Depois disso, `git status` deve funcionar (vai mostrar "No commits yet" ou arquivos não rastreados).

## 3. Adicionar o remote do GitHub (só na primeira vez)

Substitua `SEU_USUARIO` e `NOME_DO_REPOSITORIO` pela sua conta e pelo nome do repositório no GitHub (ex.: `https://github.com/SEU_USUARIO/portal-cliente.git`):

```powershell
git remote add origin https://github.com/SEU_USUARIO/NOME_DO_REPOSITORIO.git
```

Se o repositório já existir e tiver conteúdo, antes do primeiro push você pode precisar de:

```powershell
git pull origin main --allow-unrelated-histories
```

(ou `master` em vez de `main`, conforme o branch padrão do GitHub.)

## 4. Adicionar tudo, fazer commit e push

```powershell
git add .
git status
git commit -m "Portal do Cliente: Dashboard, Bookings, DataSaida, filtros, Detalhe/Alertas Motivo, layout menu"
git branch -M main
git push -u origin main
```

Se o branch padrão no GitHub for `master`:

```powershell
git push -u origin master
```

## 5. Em casa: atualizar a pasta

Na pasta **PortalCliente**:

```powershell
cd "c:\Users\diego.fonseca\OneDrive - ceslog\Documentos\Portal do Cliente\PortalCliente"
git pull origin main
```

(ou `master` se for o caso.)

---

**Resumo:**  
- O terminal pode abrir em **"Portal do Cliente"**; aí o git não funciona.  
- **Sempre fazer `cd` para a pasta específica** onde está o `.git` (no nosso caso: a pasta **PortalCliente** que fica *dentro* de "Portal do Cliente").  
- **No trabalho:** `cd "c:\...\Portal do Cliente\PortalCliente"` → `git add .` → `git commit -m "mensagem"` → `git push origin main`  
- **Em casa:** o mesmo `cd` para **PortalCliente** → `git pull origin main`.

**Se na sua máquina o repositório foi criado em outra pasta** (por exemplo só em "Portal do Cliente"), use essa pasta no `cd`. O importante é que `git status` não diga "not a git repository".
