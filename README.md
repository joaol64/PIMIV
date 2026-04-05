# Documentação técnica — Projeto PIM (ambiente virtual acadêmico)

Este documento descreve o sistema desenvolvido como **protótipo de ambiente virtual** para gerenciamento de **eventos acadêmicos**, **atividades**, **inscrições** de participantes e **emissão de certificados**. O trabalho integra **desenvolvimento web**, **programação orientada a objetos**, **banco de dados NoSQL** e preocupações de **acessibilidade** (incluindo conteúdo em **Libras**).

---

## 1. Visão geral do sistema

### 1.1 O que é o sistema

Trata-se de uma aplicação **web cliente-servidor** em que:

- **Eventos** representam períodos ou conjuntos (ex.: semana científica, congresso) com data de início e término.
- **Atividades** pertencem a um evento, possuem intervalo de tempo opcionalmente distinto, nome e **descrição** opcional.
- **Inscrições** associam o **participante** (derivado da conta de usuário) a uma **atividade** específica.
- **Certificados** podem ser emitidos com base nas **inscrições reais** da conta, consolidando quantidade de **eventos distintos** e **atividades** inscritas.

O sistema não substitui uma plataforma comercial completa; funciona como **laboratório acadêmico** com API documentável (Swagger em desenvolvimento) e interface HTML estática.

### 1.2 Perfis de usuário

| Perfil | Descrição |
|--------|-----------|
| **Participante** | Usuário padrão criado pelo **cadastro público** (`TipoUsuario.Participante`). Acessa eventos, atividades, inscreve-se e pode emitir certificado se houver inscrições. |
| **Administrador** | Conta com **permissões elevadas** (`TipoUsuario.Administrador`). Não é criada pelo registro público; pode ser semeada na subida da API (**AdminSeed**) ou ajustada no banco. Utiliza o **painel Gestão (admin)** no frontend para criar eventos e atividades. |

No modelo de dados, **Administrador** não é uma classe separada: o perfil é representado pelo enum **`TipoUsuario`** no documento **`User`**.

### 1.3 Fluxo principal

1. **Cadastro / login** — o participante cria conta ou autentica-se; o retorno da API é armazenado no **localStorage** do navegador.
2. **Área inicial (home)** — acesso a vídeo em Libras (com registro opcional de “já assistiu”), links para demais módulos.
3. **Visualizar eventos** — listagem consumida da API, com período e **contagem pública** de contas com inscrição em alguma atividade do evento.
4. **Visualizar atividades** — seleção do evento, listagem de atividades com datas, **descrição**, contagem de inscritos por atividade e ação **Se inscrever**.
5. **Inscrição** — confirmação na página de inscrição com usuário logado; a API impede **duplicidade** na mesma atividade.
6. **Certificado** — com login, a página solicita **`POST /api/certificados/emitir-resumo`**; sem inscrições válidas, a emissão é **recusada** (HTTP 400).

---

## 2. Tecnologias utilizadas

### 2.1 Frontend

- **HTML5** — páginas semânticas (`main`, `nav`, `section`, `article`, `header`, uso de `aria-*` em pontos relevantes).
- **CSS3** — variáveis para tema claro/escuro, bordas, tipografia; **Flexbox** predominante; **CSS Grid** em trechos da home.
- **JavaScript (ES6+)** — sem frameworks; módulos por arquivo (`api.js`, `eventos.js`, `atividades.js`, etc.); **`fetch`** para HTTP.

### 2.2 Backend

- **C#** com **.NET 8** — projeto **ASP.NET Core Web API**.
- **Controllers** expõem rotas sob o prefixo **`/api/...`**.
- **Services** concentram regras de negócio; **Repositories** isolam acesso ao MongoDB.

### 2.3 Banco de dados

- **MongoDB** — armazenamento documental em **collections** (ex.: `Users`, `Eventos`, `Atividades`, `Participantes`, `Inscricoes`, `Certificados`).
- Driver oficial **MongoDB.Driver** no backend.

### 2.4 Comunicação

- **HTTP/HTTPS** — requisições do navegador para o host da API.
- **API REST** — recursos nomeados por substantivos (eventos, atividades, inscrições, certificados, auth).
- **JSON** — corpo das requisições e respostas; **System.Text.Json** com **camelCase** e conversores para **datas em UTC**.

### 2.5 Ferramentas e infraestrutura

| Ferramenta | Uso no projeto |
|------------|----------------|
| **Git** | Controle de versão do repositório. |
| **Docker** | `Dockerfile` multi-stage: build com SDK 8.0, imagem final **aspnet** na porta **10000** (variável `ASPNETCORE_URLS`). |
| **Render (ou similar)** | Hospedagem típica do backend com variáveis **`PORT`**, **`DATABASE_URL`** / MongoDB. |
| **Swagger / OpenAPI** | Habilitado em ambiente **Development** para testar endpoints. |
| **DotNetEnv** | Carregamento opcional de **`backend/.env`** em desenvolvimento. |

---

## 3. Arquitetura do sistema

### 3.1 Cliente-servidor

O **cliente** é o **navegador**, que carrega HTML/CSS/JS estáticos (ou servidos por hospedagem estática). O **servidor** é o processo **Kestrel** da API .NET, que processa rotas, aplica regras e persiste no MongoDB.

### 3.2 Papel do frontend

- Renderizar interfaces e mensagens ao usuário.
- Encapsular chamadas HTTP em **`api.js`** (`apiGet`, `apiPost`, helpers de contagem pública).
- Manter **estado de sessão simplificado** no **`localStorage`** (objeto do usuário após login).
- Exibir **painel administrativo** condicionalmente conforme **`tipoUsuario`** (administrador).

### 3.3 Papel do backend

- Validar entradas, autorizar perfis onde aplicável (ex.: criação de eventos/atividades).
- Orquestrar **serviços** e **repositórios**.
- Retornar códigos HTTP adequados (**200**, **400**, **404**, **409**, **500**, etc.) e mensagens em **`ErrorResponse`**.

### 3.4 Papel do banco de dados

- Persistir documentos **JSON-like** (BSON) com identificadores **ObjectId** onde configurado.
- Garantir consistência lógica via regras na aplicação (ex.: inscrição única por par participante–atividade).

### 3.5 Comunicação via API REST

- O frontend monta URLs a partir de **`API_URL`** (ex.: `http://localhost:5000/api`) + caminho do recurso.
- **CORS** é configurado de forma **permissiva** (`AllowAnyOrigin`, métodos e headers) para facilitar o protótipo com front e back em origens diferentes.
- Datas enviadas ao servidor em **ISO-8601 (UTC)** após conversão no JavaScript do painel admin.

---

## 4. Estrutura do projeto

### 4.1 `/frontend`

| Caminho | Função |
|---------|--------|
| `*.html` | Páginas: login, cadastro, home, eventos, atividades, inscrição, certificado, Libras (`libras.html`). |
| `videos/` | Vídeos do **glossário em Libras**: arquivos `glossario1.mp4` a `glossario13.mp4` usados por `libras.html` (caminhos relativos à pasta `frontend`). Arquivos grandes costumam ficar só na máquina local ou no deploy; a pasta pode existir vazia no Git com `.gitkeep`. |
| `css/style.css` | Estilos globais, componentes de cards, listas, modal admin, responsividade; vídeos “revelados” por botão em algumas páginas vs. glossário sempre visível. |
| `js/api.js` | URL base da API, `fetch`, normalização de usuário, contagens públicas de inscrição. |
| `js/theme.js` | Tema claro/escuro e `data-theme` no `body`. |
| `js/dates.js` | Formatação de datas vindas da API em pt-BR. |
| `js/admin-panel.js` | Modal **Gestão (admin)** — criação de evento/atividade e campos de data em **DD/MM/AAAA HH:mm**. |
| `js/eventos.js`, `atividades.js`, `certificado.js`, `inscricao.js`, `home.js`, `login.js`, `register.js` | Lógica por página. |

### 4.2 `/backend`

| Caminho | Função |
|---------|--------|
| `Program.cs` | Construção do host, **DI**, CORS, Swagger, leitura de configuração e **`PORT`**. |
| `Controllers/` | **Auth**, **Eventos**, **Atividades**, **Inscricoes**, **Certificados** — mapeamento HTTP. |
| `Services/` | **Auth**, **Evento**, **Atividade**, **Inscricao**, **Certificado**, **AdminSeedHostedService**. |
| `Repositories/` | CRUD e consultas MongoDB por agregado. |
| `Models/` | Entidades de domínio e enum **`TipoUsuario`**. |
| `DTOs/` | Contratos de entrada/saída (requests/responses). |
| `Data/MongoDbContext.cs` | Cliente de banco único (`IMongoDatabase`). |
| `Helpers/ApiDateParsing.cs` | Parsing seguro de strings de data na API. |
| `Serialization/` | Conversores JSON para **`DateTime`** em UTC. |
| `Config/MongoDbSettings.cs` | Opções de conexão. |
| `appsettings*.json`, `.env.example` | Configuração e exemplos (sem segredos commitados). |
| `Dockerfile` | Build e publicação da API em container. |

---

## 5. Backend — programação orientada a objetos

### 5.1 Classes e entidades principais

- **`Usuario`** (abstrata) — base com **encapsulamento** de nome e e-mail (campos privados, propriedades com validação e normalização do e-mail em minúsculas).
- **`User`** — herda **`Usuario`**; inclui **hash de senha**, **`TipoUsuario`**, **`JaViuVideo`**; mapeada à collection de usuários de login.
- **`Participante`** — herda **`Usuario`**; representa a pessoa no domínio de eventos/inscrições, vinculada à conta pelo **mesmo e-mail** no fluxo de inscrição.
- **`Evento`** — nome, intervalo de datas (**`DataInicio`**, **`DataFim`**) e campos legados opcionais com propriedades calculadas (**`DataInicioEfetiva`**, **`DataFimEfetiva`**).
- **`Atividade`** — nome, **`Data`** (início), **`DataFim`** opcional, **`Descricao`** opcional, **`EventoId`**.
- **`Inscricao`** — vínculo **`ParticipanteId`** + **`AtividadeId`**.
- **`Certificado`** — registro de emissão; no fluxo **resumo** armazena também **`QuantidadeEventos`** e **`QuantidadeAtividades`**.

### 5.2 Encapsulamento

Propriedades expõem apenas o necessário; validações em setters (ex.: nome não vazio, limites de tamanho) e tipos imutáveis no uso externo típico.

### 5.3 Herança e perfil de administrador

- **`Participante`** e **`User`** **herdam** **`Usuario`** (reuso de regras de nome/e-mail).
- O **administrador** não é subclasse de `Usuario`: o perfil é discriminado pelo **`enum TipoUsuario`** dentro de **`User`**.

### 5.4 Coleções genéricas

Uso de **`List<T>`**, **`HashSet<string>`**, **`Dictionary<string, string>`** em serviços (ex.: mapear atividade → evento no certificado), **`IReadOnlyCollection<string>`** em assinaturas de repositório onde apropriado.

### 5.5 Tratamento de exceções

- **Controllers** e **Services** capturam **`MongoException`** e exceções genéricas, retornando **500** com mensagem genérica quando necessário, evitando vazar detalhes internos.
- Validações de negócio retornam tuplas **`(bool Ok, string? ErrorMessage, ...)`** nos serviços, traduzidas em **400**, **404**, **403** ou **409** nos controllers.

### 5.6 Persistência no MongoDB

- Repositórios utilizam **`IMongoCollection<T>`** com filtros **`FilterDefinition`** e operações **`InsertOne`**, **`Find`**, **`CountDocuments`**, etc.
- Atributos **`[BsonId]`**, **`[BsonIgnoreIfNull]`**, **`[BsonDateTimeOptions]`** quando aplicável.

### 5.7 LINQ

Exemplos no código:

- Filtrar atividades por **`EventoId`** e ordenar por **`Data`** (**`AtividadeService`**).
- Ordenar eventos por **`DataInicioEfetiva`** (**`EventoService`**).
- Ordenar inscrições por participante e atividade (**`InscricaoService`**).
- Projeções com **`Select`**, **`Where`**, **`Distinct`**, **`OrderBy`** em serviços e repositórios.

---

## 6. Frontend — desenvolvimento web responsivo

### 6.1 Estrutura HTML

- Páginas com **`main`**, **`nav`** (`site-nav`), seções com títulos hierárquicos, formulários acessíveis com **`label`** associado a **`input`**, **`dialog`** para o modal admin.

### 6.2 CSS3

- Variáveis CSS para cores, bordas e fundos em tema claro/escuro.
- Tipografia e espaçamento consistentes em cards e listas.

### 6.3 Flexbox e Grid

- **Flexbox**: layout da página (`body`, `container`), barras superiores, linhas de botões, itens de evento/atividade.
- **Grid**: uso pontual (ex.: áreas da home com **`grid-template-columns`** e **`auto-fit`**).

### 6.4 Responsividade

- **Media queries** (ex.: largura máxima **520px**) ajustam padding, navegação e campos (incluindo textarea do admin).

### 6.5 Navegação

- Links entre páginas `.html`; parâmetros de query (`?eventoId=`, `?atividadeId=`) para fluxos específicos.

### 6.6 Integração com o backend

- **`fetch`** via **`apiGet`** / **`apiPost`** em **`api.js`**.
- Tratamento de erro lendo **`message`** do JSON retornado pela API.

### 6.7 Sessão com `localStorage`

- Objeto do usuário após login/registro; funções **`getUserFromLocalStorage`**, **`saveUserToLocalStorage`**, **`isAdministrator`**.

### 6.8 Acessibilidade

- **Home** — vídeo em Libras (classe CSS `home-video`, sempre visível) e registro opcional de “já assistiu” via API.
- **`libras.html`** — glossário estático: para cada termo há um elemento **`<video>`** (`controls`, `preload="metadata"`) apontando para `videos/glossario1.mp4` … `videos/glossario13.mp4`, com **`aria-label`** por termo e descrição em português ao lado.
- Uso de **`aria-label`**, **`aria-current`**, **`role`** onde cabível; alternância de tema para contraste; textos descritivos no fluxo de inscrição e certificado; link **“Ir para o conteúdo principal”** (`skip-link`) nas páginas do protótipo.

---

## 7. Funcionalidades do sistema

| Funcionalidade | Descrição técnica resumida |
|----------------|----------------------------|
| **Cadastro / login** | `POST /api/auth/register`, `POST /api/auth/login`; resposta normalizada no cliente. |
| **Diferenciação de perfil** | Campo **`tipoUsuario`** (0 participante, 1 administrador); painel admin só se administrador. |
| **Criação de eventos** | `POST /api/eventos` com **`usuarioId`** de admin, nome, **`dataInicio`**, **`dataFim`** em ISO. |
| **Criação de atividades** | `POST /api/atividades` com intervalo dentro do evento, **`descricao`** opcional. |
| **Visualização de eventos** | `GET /api/eventos`; contagem pública `GET /api/inscricoes/contagem/evento/{id}`. |
| **Visualização de atividades** | `GET /api/eventos/{eventoId}/atividades`; contagem por atividade; exibição de descrição. |
| **Inscrição** | `POST /api/inscricoes`; verificação de duplicidade (**409**). |
| **Certificado (resumo)** | `POST /api/certificados/emitir-resumo`; exige inscrições; legado `POST /api/certificados` (participante + evento fixos). |
| **Libras** | Página **`libras.html`** com glossário (13 termos + vídeo + texto); na home, vídeo introdutório e fluxo de “vídeo visto”. |
| **Vídeo visto** | `POST /api/auth/video-seen` para persistir flag no usuário. |

---

## 8. Como executar o projeto

### 8.1 Backend (desenvolvimento local)

```powershell
$env:MongoDbSettings__ConnectionString = "mongodb://localhost:27017"
$env:MongoDbSettings__DatabaseName = "pim"
dotnet run --project .\backend\Backend.csproj
```

Por padrão a API escuta **`http://*:5000`** (ou o valor da variável **`PORT`**).

### 8.2 Frontend

Servir a pasta **`frontend`** com um **servidor HTTP estático** (evita limitações de **`file://`** com **`fetch`** e garante carregamento correto dos **`.mp4`** do glossário). Exemplos: extensão “Live Server”, `npx serve frontend`, ou IIS/Apache apontando para a pasta.

Coloque os arquivos **`glossario1.mp4`–`glossario13.mp4`** em **`frontend/videos/`** para a página Libras exibir os sinais. Os nomes devem coincidir exatamente com os usados no HTML (incluindo minúsculas).

### 8.3 Conexão com o MongoDB

- Definir **`MongoDbSettings__ConnectionString`** e **`MongoDbSettings__DatabaseName`** (ou **`DATABASE_URL`** no ambiente, lido pelo `Program.cs` quando a string no appsettings está vazia).
- Opcional: copiar **`backend/.env.example`** para **`.env`** e ajustar (não versionar segredos).

### 8.4 Configurar `API_URL`

Em **`frontend/js/api.js`**, alterar a constante **`API_URL`** para a base da API (ex.: `http://localhost:5000/api` em desenvolvimento ou URL pública em produção).

### 8.5 Docker

Build a partir da pasta **`backend`** (contexto onde está o **`Dockerfile`**):

```bash
docker build -t pim-api ./backend
docker run -p 10000:10000 -e DATABASE_URL="mongodb+srv://..." pim-api
```

A imagem expõe a porta **10000** conforme **`ASPNETCORE_URLS`** no Dockerfile; alinhe variáveis de ambiente (MongoDB, etc.) ao provedor.

### 8.6 Deploy (ex.: Render)

- **Serviço web** executando o container ou **`dotnet publish`**.
- Variáveis: **`PORT`**, string de conexão MongoDB (**`DATABASE_URL`** ou equivalente).
- Frontend em hospedagem estática com **`API_URL`** apontando para a URL pública da API.

### 8.7 Conta administrador (seed)

Em **`appsettings.Development.json`** (ou **`appsettings.json`**) a seção **`AdminSeed`** pode definir e-mail e senha para criação automática de administrador na subida (**`AdminSeedHostedService`**). **Alterar credenciais padrão em ambientes reais.**

---

## 9. Testes do sistema

Não há suíte automatizada de testes unitários/integrados neste repositório; recomenda-se **teste manual** sistemático:

1. **Login / cadastro** — registrar participante, autenticar, verificar persistência no `localStorage` e chamadas à API.
2. **Administrador** — login com conta admin (seed); abrir **Gestão (admin)**; criar evento e atividade com datas em **DD/MM/AAAA**; conferir listagens.
3. **Eventos e atividades** — carregar listas, contadores públicos, descrição da atividade.
4. **Inscrição** — inscrever-se, tentar inscrever de novo na mesma atividade (esperado **409**).
5. **Certificado** — com inscrições, abrir página do certificado; sem inscrições, verificar mensagem de erro da API (**400**).
6. **Responsividade** — **Ferramentas de desenvolvedor** do navegador (modo dispositivo), larguras abaixo e acima de **520px**, leitura e botões utilizáveis.

Em desenvolvimento, **Swagger** (`/swagger`) auxilia testes diretos nos endpoints.

---

## 10. Considerações finais

Este projeto foi concebido como **protótipo interdisciplinar**, reunindo:

- **Desenvolvimento web responsivo** — HTML5, CSS3 (Flexbox/Grid, media queries), JavaScript e consumo de API.
- **Programação orientada a objetos** — modelagem em C#, herança (`Usuario` → `User` / `Participante`), encapsulamento, serviços e repositórios.
- **Banco de dados** — MongoDB, persistência documental, consultas e agregações no código com **LINQ**.
- **Acessibilidade e Libras** — vídeo e páginas de apoio, preocupação com contraste e estrutura semântica.
- **Comunicação e documentação técnica** — API REST/JSON, CORS, README e comentários no código como suporte ao relatório acadêmico.

Trata-se de **base pedagógica**, não de produto final homologado para produção em larga escala (segurança, autenticação avançada e testes automatizados podem ser aprofundados em trabalhos futuros).

---

*Documento alinhado ao estado do repositório na elaboração desta documentação. Ajuste variáveis de ambiente, URLs e credenciais conforme o ambiente de cada instituição ou deploy.*
