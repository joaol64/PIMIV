# Projeto PIM (Backend + Frontend separados)

Este projeto é uma aplicação web simples com foco em **acessibilidade**: cadastro, autenticação, eventos acadêmicos (protótipo), vídeo em **Libras** e fluxo básico de inscrição e certificado.

A solução separa **frontend** (HTML/CSS/JS, sem frameworks) e **backend** (ASP.NET Core + MongoDB), com comunicação por HTTP (`fetch`).

## Objetivo

- Cadastro e login de usuários (participante / administrador no modelo de dados).
- Área logada com vídeo em Libras e links para **eventos**, **atividades**, **Libras** e **certificado** (protótipo acadêmico).
- API para eventos, atividades, inscrições e certificados (quando configurada).

## Estrutura do repositório

| Pasta       | Descrição |
|------------|-----------|
| `backend/` | ASP.NET Core Web API (.NET 8) + MongoDB |
| `frontend/` | HTML, CSS e JavaScript puro |

---

## Frontend

### Páginas

| Arquivo | Função |
|---------|--------|
| `index.html` | Login |
| `register.html` | Cadastro |
| `home.html` | Início logado: vídeo Libras, CTAs e seções de programação / inscrições |
| `eventos.html` | Lista de eventos (exemplo estático; pode evoluir para API) |
| `atividades.html` | Lista atividades do evento via `GET /api/eventos/{eventoId}/atividades` |
| `inscricao.html` | Confirma inscrição (`POST /api/inscricoes`) |
| `certificado.html` | Certificado simulado + download `.txt` |
| `libras.html` | Vídeo placeholder + glossário (Evento, Inscrição, Certificado) |

Scripts: `js/api.js` (URL da API, `apiGet` / `apiPost`), `js/theme.js`, e um JS por página quando necessário.

### Navegação (links simples)

- **Barra `site-nav`** (mesma ideia em várias páginas): Início · Eventos · Atividades · Libras · Certificado.
- **Home:** botões **Ver programação** → `eventos.html`; **Ver atividades** → `atividades.html`.
- **Home → Libras / certificado:** também por links no texto da seção de inscrições e pela `site-nav`.
- **Atividades → Inscrição:** em cada item da lista, o link **Se inscrever** aponta para `inscricao.html?atividadeId=...&nome=...`.

### Layout e responsividade (CSS)

- **`body`:** `display: flex`, `flex-direction: column`, conteúdo centralizado na horizontal; em telas **≤ 520px** o conteúdo alinha ao topo (`justify-content: flex-start`) para rolagem confortável no celular.
- **`main.container`:** `flex: 1 0 auto`, largura total até `max-width` do container (460px ou 640px na variante `container--home`).
- **Listas e CTAs:** `eventos-item` em coluna com **flexbox**; `home-cta-row` em **flex** com quebra; no mobile, CTAs em coluna.
- **Media queries:** bloco `@media (max-width: 520px)` para padding, tipografia e navegação; `@media (min-width: 521px)` mantém a página mais centralizada na vertical.

### URL da API

Em `frontend/js/api.js`, ajuste `API_URL` para o seu backend (ex.: `http://localhost:5000/api` em desenvolvimento ou a URL do Render em produção).

### Rodando o frontend

Abra os `.html` no navegador ou use um servidor estático na pasta `frontend` (evita limites de `file://` com `fetch`).

---

## Backend (.NET 8 + MongoDB)

### Camadas (resumo)

```
backend/
 ├── Controllers/   → Auth, Eventos, Atividades, Inscricoes, Certificados
 ├── Services/
 ├── Repositories/
 ├── Models/        → Usuario, User, Evento, Atividade, Participante, Inscricao, Certificado, …
 ├── Data/          → MongoDbContext
 ├── DTOs/
 └── Program.cs
```

### Endpoints principais (referência)

| Método | Rota | Observação |
|--------|------|------------|
| `POST` | `/api/auth/register` | Cadastro (participante) |
| `POST` | `/api/auth/login` | Login |
| `POST` | `/api/auth/video-seen` | Marca vídeo como visto |
| `GET` | `/api/eventos` | Lista eventos |
| `GET` | `/api/eventos/{id}` | Detalhe do evento |
| `POST` | `/api/eventos` | Criar (admin) |
| `GET` | `/api/eventos/{eventoId}/atividades` | Atividades do evento |
| `GET` / `POST` | `/api/atividades` | Listar / criar (admin) |
| `GET` / `POST` | `/api/inscricoes` | Listar / inscrever |
| `GET` / `POST` | `/api/certificados` | Listar / gerar |

### Variáveis de ambiente (MongoDB)

- `MongoDbSettings__ConnectionString`
- `MongoDbSettings__DatabaseName`

### Conta administrador (seed)

Em desenvolvimento, `appsettings.Development.json` pode definir `AdminSeed:Email` e `AdminSeed:Password` para criar um admin na subida da API (troque a senha em ambientes reais).

### Rodar o backend

```powershell
$env:MongoDbSettings__ConnectionString = "mongodb://localhost:27017"
$env:MongoDbSettings__DatabaseName = "pim"
dotnet run --project .\backend\Backend.csproj
```

---

## Modelo de usuários (POO) — resumo

- **`Usuario`:** classe base abstrata com campos privados e propriedades validadas (`Nome`, `Email`).
- **`User`:** herda `Usuario`; login, hash de senha, `TipoUsuario` (Participante / Administrador).
- **`Participante`:** herda `Usuario`; usado no domínio de eventos/inscrições.

---

## Deploy (exemplos)

- **Backend:** Render (ou similar), com variáveis MongoDB e `PORT`.
- **Frontend:** Vercel ou hospedagem estática; atualize `API_URL` para a URL pública da API.

Este repositório é um **protótipo acadêmico** (PIM), propositalmente simples, não um produto final.
