# Projeto PIM (Backend + Frontend separados)

Este projeto consiste no desenvolvimento de uma aplicação web simples com foco em acessibilidade, permitindo que usuários realizem cadastro, autenticação e acesso a um conteúdo em vídeo na Língua Brasileira de Sinais (Libras).

A aplicação foi estruturada com separação entre frontend e backend, adotando uma arquitetura em camadas no servidor e comunicação via protocolo HTTP.

Objetivo

Desenvolver um sistema web funcional que:

Permita o cadastro e autenticação de usuários

Disponibilize conteúdo acessível em Libras após o login

Utilize tecnologias modernas de desenvolvimento web

Aplique boas práticas de organização e separação de responsabilidades

Sistema simples com:

- Cadastro de usuário
- Login
- Após login: página com botão para assistir vídeo em Libras

## Estrutura

```
backend/   -> ASP.NET Core Web API (.NET 8) + MongoDB
frontend/  -> HTML, CSS e JavaScript puro (sem frameworks)
```

Importante:

- A comunicação entre frontend e backend é realizada por meio de requisições HTTP utilizando a API desenvolvida.

## Backend (.NET 8 + MongoDB)

### Camadas

```
backend/
 ├── Controllers/   -> recebe requisições HTTP (AuthController)
 ├── Services/      -> lógica (AuthService)
 ├── Repositories/  -> acesso ao MongoDB (UserRepository)
 ├── Models/        -> entidades (User)
 ├── DTOs/          -> objetos de entrada/saída (RegisterRequest, LoginRequest, AuthResponse)
 ├── Config/        -> configurações (MongoDbSettings)
 ├── Program.cs
```

### Endpoints

- `POST /api/auth/register`
- `POST /api/auth/login`

### Variáveis de ambiente (OBRIGATÓRIO)

Este projeto **não** usa connection string fixa.

Use estas variáveis:

- `MongoDbSettings__ConnectionString`
- `MongoDbSettings__DatabaseName`

Por que tem `__` (dois underscores)?

- No .NET, `MongoDbSettings__ConnectionString` mapeia para `MongoDbSettings:ConnectionString` na configuração.

### Rodando localmente

1) Garanta que você tem um MongoDB rodando (local ou Atlas).

2) Configure as variáveis de ambiente (NÃO coloque usuário/senha dentro do código nem em arquivos commitados):

```powershell
# Exemplo Mongo local:
$env:MongoDbSettings__ConnectionString = "mongodb://localhost:27017"
$env:MongoDbSettings__DatabaseName = "pim"

# Exemplo MongoDB Atlas (não commite essa string em repo público):
# $env:MongoDbSettings__ConnectionString = "mongodb+srv://USUARIO:SENHA@CLUSTER.../pim?retryWrites=true&w=majority"
# $env:MongoDbSettings__DatabaseName = "pim"
```

3) Rode o backend:

```powershell
dotnet run --project .\backend\Backend.csproj
```

A API fica em `http://localhost:5000`.

### Deploy no Render (produção)

- Configure as variáveis de ambiente no painel do Render:
  - `MongoDbSettings__ConnectionString`
  - `MongoDbSettings__DatabaseName`
- O Render fornece a variável `PORT`.
- O `Program.cs` já usa porta dinâmica:
  - `var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";`
  - `app.Urls.Add($"http://*:{port}");`

## Frontend (HTML/CSS/JS)

### Arquivos

```
frontend/
 ├── index.html      -> login
 ├── register.html   -> cadastro
 ├── home.html       -> página logada + vídeo
 ├── css/style.css
 ├── js/api.js       -> URL centralizada da API
 ├── js/login.js
 ├── js/register.js
 ├── js/home.js
```

### API URL

Em `frontend/js/api.js`:

```js
const API_URL = "http://localhost:5000/api";
```

### Rodando o frontend

Você pode simplesmente abrir `frontend/index.html` no navegador.

Observação: alguns navegadores podem limitar `fetch` quando o HTML é aberto via `file://`.
Se acontecer, rode um servidor local simples (opcional).

### Deploy no Vercel

Como é HTML/CSS/JS estático, você pode colocar a pasta `frontend` como projeto no Vercel.

Em produção:

- Ajuste o `API_URL` para apontar para a URL do seu backend no Render.
- Se quiser, você pode trocar `API_URL` para ler de uma variável/arquivo de config.

