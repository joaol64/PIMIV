# Projeto PIM — Backend + Frontend

Protótipo acadêmico de **eventos e atividades** com foco em **acessibilidade**: cadastro, login, vídeo em **Libras**, inscrições em atividades, certificado e painel administrativo para criar eventos e atividades. O front é **HTML/CSS/JS** (sem framework); o back é **ASP.NET Core 8** com **MongoDB**, exposto como **API REST** (JSON).

**Resumo em uma frase:** páginas estáticas chamam a API com `fetch`; o servidor concentra regras de negócio e persistência; o estado do usuário logado fica no `localStorage`.

---

## Funcionalidades (site)

| Área | O que o usuário vê / faz |
|------|---------------------------|
| **Cadastro e login** | `register.html` / `index.html`; perfil participante ou administrador. |
| **Home logada** | `home.html`: vídeo Libras, marcação de “já assistiu” via API, links para eventos, atividades, Libras e certificado. |
| **Eventos** | `eventos.html`: lista **vinda da API** (`GET /api/eventos`), período (início/término), **contador público** de quantas **contas distintas** têm inscrição em alguma atividade do evento, link para atividades. |
| **Atividades** | `atividades.html`: escolhe o evento, lista atividades (`GET /api/eventos/{id}/atividades`), datas de início/término quando existirem, **contador público** de inscritos **por atividade**, botão **Se inscrever**. |
| **Inscrição** | `inscricao.html`: confirma com usuário logado (`POST /api/inscricoes`). **Não permite** inscrição duplicada na mesma atividade (API responde **409**). |
| **Administrador** | Botão **Gestão (admin)** (em páginas que carregam `admin-panel.js`): modal para **criar evento** (nome + período) e **criar atividade** (evento, nome, período dentro do evento). Datas com máscara numérica ou colagem em formato brasileiro. |
| **Certificado** | `certificado.html`: protótipo de texto/download `.txt` (integração completa com `GET/POST /api/certificados` pode evoluir). |
| **Libras** | `libras.html`: conteúdo ilustrativo / glossário. |
| **Interface** | Tema claro/escuro (`theme.js`), layout responsivo. |

**Contadores públicos:** `GET /api/inscricoes/contagem/evento/{eventoId}` e `GET /api/inscricoes/contagem/atividade/{atividadeId}` — não exigem autenticação; o front usa esses endpoints em eventos e atividades.

---

## Funcionamento geral

1. **Cliente (navegador):** cada página HTML inclui `js/api.js` (URL base da API, `apiGet` / `apiPost`) e, quando necessário, scripts específicos (`eventos.js`, `atividades.js`, `admin-panel.js`, etc.).
2. **Sessão simplificada:** após login/registro, o JSON do usuário (id, nome, e-mail, tipo, etc.) é guardado no **`localStorage`**. Não há JWT nem cookie de sessão nesta versão; rotas “protegidas” no front apenas checam se há usuário salvo.
3. **Fluxo típico:** visitante cadastra → loga → assiste ao vídeo (opcionalmente registrado na API) → navega em **Eventos** → abre **Atividades** de um evento → **Se inscrever** abre a confirmação com `atividadeId` na URL.
4. **Admin:** usuário com `TipoUsuario = Administrador` vê **Gestão (admin)** e cria eventos/atividades; o front dispara `POST /api/eventos` e `POST /api/atividades` com datas em **ISO (UTC)**.
5. **CORS:** o backend permite origens abertas para o front estático poder consumir a API de outro host (ex.: deploy separado).
6. **Configuração:** MongoDB via `appsettings`, `MongoDbSettings__*` ou `DATABASE_URL`; `backend/.env` pode ser carregado em desenvolvimento. Porta: variável `PORT` (nuvem) ou **5000** local.

---

## Camadas do backend (visão em camadas)

Fluxo típico de uma requisição: **`Controller`** → **`Service`** (regras) → **`Repository`** (MongoDB) → **`Models`**.

```
backend/
 ├── Controllers/     → Rotas HTTP, validação básica de entrada, códigos HTTP (200, 400, 404, 409, …)
 ├── Services/        → Lógica de negócio (auth, eventos, atividades, inscrições, certificados, seed de admin)
 ├── Repositories/    → Acesso às collections MongoDB (CRUD, contagens, filtros)
 ├── Models/          → Entidades persistidas (User, Evento, Atividade, Participante, Inscricao, Certificado, …)
 ├── DTOs/            → Contratos de entrada/saída da API (requests/responses)
 ├── Data/            → MongoDbContext (uma instância de banco compartilhada)
 ├── Helpers/         → Utilitários (ex.: parsing de datas ISO / formatos da API)
 ├── Config/          → Classes de configuração (ex.: MongoDB)
 ├── Serialization/   → Ajustes JSON (datas em UTC)
 └── Program.cs       → DI, pipeline HTTP, CORS, Swagger (dev)
```

**Front (por página):** HTML estrutura + `style.css` + `api.js` + JS da página; **admin-panel.js** injeta o modal só se o usuário for administrador.

---

## Estrutura do repositório

| Pasta | Conteúdo |
|-------|----------|
| `backend/` | ASP.NET Core Web API (.NET 8) + MongoDB |
| `frontend/` | HTML, CSS e JavaScript puro |

---

## Frontend

### Páginas

| Arquivo | Função |
|---------|--------|
| `index.html` | Login |
| `register.html` | Cadastro |
| `home.html` | Início logado: vídeo Libras, CTAs |
| `eventos.html` | Lista eventos da API + contagem de contas inscritas no evento |
| `atividades.html` | Escolhe evento, lista atividades da API + contagem por atividade |
| `inscricao.html` | Confirma inscrição (`POST /api/inscricoes`) |
| `certificado.html` | Certificado simulado + download `.txt` |
| `libras.html` | Vídeo / glossário |

Scripts centrais: `js/api.js` (URL da API, helpers de contagem pública), `js/theme.js`, `js/dates.js` (formatação de datas), `js/admin-panel.js` (gestão admin).

### URL da API

Em `frontend/js/api.js`, ajuste **`API_URL`** (ex.: `http://localhost:5000/api` em desenvolvimento ou a URL pública do backend em produção).

### Rodando o frontend

Abra os `.html` via servidor estático na pasta `frontend` (recomendado, para `fetch` e CORS funcionarem de forma previsível).

---

## Backend — Endpoints principais

| Método | Rota | Observação |
|--------|------|------------|
| `POST` | `/api/auth/register` | Cadastro (participante) |
| `POST` | `/api/auth/login` | Login |
| `POST` | `/api/auth/video-seen` | Marca vídeo como visto |
| `GET` | `/api/eventos` | Lista eventos |
| `GET` | `/api/eventos/{id}` | Detalhe do evento |
| `POST` | `/api/eventos` | Criar evento (admin) |
| `GET` | `/api/eventos/{eventoId}/atividades` | Atividades do evento |
| `GET` / `POST` | `/api/atividades` | Listar / criar atividade (admin) |
| `GET` | `/api/inscricoes/contagem/evento/{eventoId}` | Contagem pública (participantes distintos no evento) |
| `GET` | `/api/inscricoes/contagem/atividade/{atividadeId}` | Contagem pública (inscrições na atividade) |
| `GET` / `POST` | `/api/inscricoes` | Listar / inscrever (duplicata → **409**) |
| `GET` / `POST` | `/api/certificados` | Listar / gerar |

Em desenvolvimento, **Swagger** documenta as rotas (`/swagger`).

### Variáveis de ambiente (MongoDB)

- `MongoDbSettings__ConnectionString`
- `MongoDbSettings__DatabaseName`

### Conta administrador (seed)

`appsettings.Development.json` pode definir `AdminSeed:Email` e `AdminSeed:Password` para criar um admin na subida da API (altere senhas em ambientes reais).

### Rodar o backend

```powershell
$env:MongoDbSettings__ConnectionString = "mongodb://localhost:27017"
$env:MongoDbSettings__DatabaseName = "pim"
dotnet run --project .\backend\Backend.csproj
```

---

## Modelo de usuários (POO) — resumo

- **`Usuario`:** classe base com validação de nome/e-mail.
- **`User`:** herda `Usuario`; autenticação, hash de senha, `TipoUsuario` (Participante / Administrador).
- **`Participante`:** herda `Usuario`; domínio de inscrições (ligado à conta pelo e-mail na hora de inscrever).

---

## Deploy (exemplos)

- **Backend:** Render (ou similar), com variáveis MongoDB e `PORT`.
- **Frontend:** hospedagem estática; alinhe `API_URL` à URL pública da API.

Este repositório é um **protótipo acadêmico** (PIM), propositalmente simples, não um produto final.
