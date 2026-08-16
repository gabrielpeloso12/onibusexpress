# OniBus Express

> MVP full-stack de vendas de passagens rodoviárias da **OniBus Express**: API em **.NET 8 (ASP.NET Core Web API)** seguindo **DDD** e **SOLID**, e frontend em **React 18 + TypeScript** consumindo essa API.

O backend permite consultar rotas e viagens disponíveis, e criar, consultar e cancelar reservas de passagem, com documentação interativa via **Swagger/OpenAPI**, pronta para consumo por qualquer cliente externo (frontend web, mobile, Postman, etc.). Todos os endpoints são públicos — não há autenticação. O frontend implementa as 4 telas do fluxo de compra sobre essa mesma API — veja a seção [FRONTEND](#frontend) para todos os detalhes.

---

## Sumário

- [Requisitos atendidos](#requisitos-atendidos)
- [Tecnologias utilizadas](#tecnologias-utilizadas)
- [Arquitetura e organização de pastas](#arquitetura-e-organização-de-pastas)
- [Modelo de domínio](#modelo-de-domínio)
- [Regras de negócio](#regras-de-negócio)
- [Como executar](#como-executar)
- [Guia de uso da API — passo a passo](#guia-de-uso-da-api--passo-a-passo)
- [Documentação via Swagger](#documentação-via-swagger)
- [Testes automatizados](#testes-automatizados)
- [Migrações do banco de dados](#migrações-do-banco-de-dados)
- [Decisões de projeto (backend)](#decisões-de-projeto-backend)
- [FRONTEND](#frontend)

---

## Requisitos atendidos

| Requisito do desafio | Status |
|---|:---:|
| .NET 8+ (ASP.NET Core Web API) | ✅ |
| Arquitetura DDD (`domain` / `application` / `infra` / `api`) | ✅ |
| Princípios SOLID | ✅ |
| Entity Framework Core + banco relacional (PostgreSQL) | ✅ |
| Docker + docker-compose | ✅ |
| Testes automatizados (xUnit) | ✅ (43 testes: 27 unitários + 16 de integração) |
| 6 endpoints mínimos (`/rotas`, `/viagens`, `/reservas`) | ✅ |
| Validação de CPF (formato + dígito verificador) | ✅ |
| Assento já ocupado não pode ser reservado | ✅ |
| Viagem já realizada não pode ser reservada | ✅ |
| Código de reserva único e legível (`ABC-12345`) | ✅ |
| Cancelamento só até 2h antes da partida | ✅ |
| React 18+ com TypeScript | ✅ |
| Gerenciador de estado (Context API) | ✅ |
| Testes com React Testing Library + Vitest | ✅ (18 testes) |
| Docker servindo o frontend com Nginx | ✅ |
| Tela 1 — Busca de passagens | ✅ |
| Tela 2 — Seleção de assento | ✅ |
| Tela 3 — Dados do passageiro e confirmação | ✅ |
| Tela 4 (bônus) — Consulta de reserva | ✅ |

---

## Tecnologias utilizadas

| Categoria | Tecnologia |
|---|---|
| Framework | .NET 8 / ASP.NET Core Web API |
| Persistência | Entity Framework Core 8 + PostgreSQL (Npgsql) |
| Documentação da API | Swagger / OpenAPI (Swashbuckle.AspNetCore) |
| Testes unitários | xUnit + Moq |
| Testes de integração | xUnit + `WebApplicationFactory` + SQLite in-memory |
| Containerização | Docker + docker-compose |

---

## Arquitetura e organização de pastas

O projeto segue **Domain-Driven Design** com quatro camadas bem delimitadas por projeto .NET, cada uma com uma única responsabilidade (Single Responsibility) e dependendo apenas de camadas "mais internas" (Dependency Inversion — a API e a Infra dependem do Domínio, nunca o contrário):

```
src/
  backend/
    OniBusExpress.Domain/        # Entidades, value objects, regras de negócio, interfaces de repositório
    OniBusExpress.Application/   # Casos de uso (services), DTOs, interfaces de infraestrutura
    OniBusExpress.Infra/         # EF Core, repositórios, migrations, seed
    OniBusExpress.Api/           # Controllers, Program.cs, middlewares, Swagger, appsettings
  frontend/                      # App React + TypeScript (ver seção FRONTEND)
    src/components/
    src/pages/
    src/services/
    src/__tests__/
    Dockerfile
  tests/
    OniBusExpress.UnitTests/         # Regras de domínio isoladas (sem banco de dados)
    OniBusExpress.IntegrationTests/  # Fluxo HTTP completo, com SQLite in-memory
```

Esta seção cobre a organização do **backend** (`src/backend` + `src/tests`); a organização interna do `src/frontend` está detalhada na seção [FRONTEND](#frontend).

### Por que essa divisão?

- **Domain** não depende de nenhum outro projeto. Contém as regras de negócio "puras" (entidades, value objects, exceções de domínio) e apenas as **interfaces** dos repositórios — nunca a implementação. É o núcleo da aplicação.
- **Application** depende só do Domain. Orquestra os casos de uso (ex.: "criar uma reserva") coordenando repositórios e serviços de domínio, sem saber como eles são implementados (EF Core, etc. são só abstrações aqui).
- **Infra** depende do Domain e do Application. Implementa tudo o que é "detalhe técnico": acesso a dados (EF Core/PostgreSQL), geração do código de reserva.
- **Api** depende de todas as camadas apenas para fazer a composição via injeção de dependência (`Program.cs`). Os controllers são bem finos: só traduzem HTTP ⇄ chamadas de `Application`.

Essa organização permite, por exemplo, trocar PostgreSQL por outro banco, ou EF Core por outro ORM, alterando apenas o projeto `Infra` — nada no `Domain` ou no `Application` muda.

---

## Modelo de domínio

| Entidade | Campos principais |
|---|---|
| **Route** (Rota) | Origem, destino, duração estimada |
| **Trip** (Viagem) | Rota associada, data/hora de partida, preço base, total de assentos |
| **Passenger** (Passageiro) | Nome, CPF (value object validado), e-mail, data de nascimento |
| **Booking** (Reserva/Passagem) | Viagem, passageiro, número do assento, status, código de reserva |

`Cpf` é um **value object** imutável: só é possível existir uma instância `Cpf` válida (dígito verificador correto), pois o único construtor público é `Cpf.Create(...)`, que lança `InvalidCpfException` caso o CPF seja inválido.

As regras de ocupação de assento e prazo de cancelamento vivem **dentro das entidades** `Trip` e `Booking` (`Trip.EnsureCanBeBooked(...)`, `Booking.Cancel(...)`), não em serviços externos — isso garante que essas invariantes nunca possam ser violadas, não importa por onde a entidade seja manipulada.

---

## Regras de negócio

Todas implementadas como exceções de domínio (`OniBusExpress.Domain.Exceptions`), traduzidas para códigos HTTP apropriados por um middleware central:

| Regra | Exceção | HTTP |
|---|---|---|
| Não reservar assento já ocupado | `SeatAlreadyBookedException` | 409 Conflict |
| Não reservar viagem já realizada | `TripAlreadyDepartedException` | 409 Conflict |
| CPF deve ter formato e dígito verificador válidos | `InvalidCpfException` | 400 Bad Request |
| Cancelamento só até 2h antes da partida | `CancellationWindowExpiredException` | 409 Conflict |
| Número de assento fora do intervalo da viagem | `InvalidSeatNumberException` | 400 Bad Request |
| Viagem/reserva inexistente | — (retorno `null`/`false` tratado no controller) | 404 Not Found |

O **código de reserva** (ex.: `ABC-12345`) é gerado por `ReservationCodeGenerator`, que sorteia 3 letras (sem `I`/`O`, para evitar confusão com `1`/`0`) + 5 dígitos e verifica unicidade no banco antes de aceitar o código, tentando novamente em caso de colisão.

---

## Como executar

### Opção 1 — Docker Compose (recomendado)

Sobe a API e o PostgreSQL juntos; a API aplica as migrations e popula os dados de exemplo automaticamente na inicialização.

```bash
docker-compose up --build
```

- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger

### Opção 2 — Local (.NET + PostgreSQL já instalados)

```bash
# 1. Suba um PostgreSQL local (ou aponte appsettings.Development.json para um existente)
# 2. Restaure e rode a API
dotnet restore
dotnet run --project src/backend/OniBusExpress.Api
```

- API: http://localhost:5083
- Swagger UI: http://localhost:5083/swagger

Há um único profile em `launchSettings.json` (HTTP, sem HTTPS) — por isso a API sobe sempre na mesma URL, tanto via `dotnet run` no terminal quanto pelo Visual Studio/Rider (F5), sem certificado de desenvolvimento para configurar.

A API aplica as migrations e faz o seed (rotas e viagens de exemplo) automaticamente no startup. Todos os endpoints são públicos — nenhuma chamada exige autenticação.

---

## Guia de uso da API — passo a passo

Fluxo completo de ponta a ponta, da busca até o cancelamento de uma passagem. Os exemplos usam `curl`, mas os mesmos passos podem ser feitos direto pelo **Swagger UI** (`/swagger`) clicando em *Try it out* em cada endpoint.

> Base URL usada nos exemplos: `http://localhost:8080` (Docker) — troque para `http://localhost:5083` se estiver rodando localmente (`dotnet run` ou Visual Studio).

### 1. Consultar as rotas disponíveis

```bash
curl http://localhost:8080/rotas
```

**Resposta `200 OK`:**

```json
[
  {
    "id": "b3f1c2a4-1234-4a5b-9c3d-000000000001",
    "origin": "São Paulo",
    "destination": "Rio de Janeiro",
    "estimatedDuration": "06:00:00"
  }
]
```

### 2. Buscar viagens por origem, destino e/ou data

Todos os filtros são opcionais e podem ser combinados.

```bash
curl "http://localhost:8080/viagens?origem=Sao Paulo&destino=Rio&data=2026-08-20"
```

**Resposta `200 OK`:**

```json
[
  {
    "id": "c4a1b2d3-1234-4a5b-9c3d-000000000010",
    "routeId": "b3f1c2a4-1234-4a5b-9c3d-000000000001",
    "origin": "São Paulo",
    "destination": "Rio de Janeiro",
    "departureDateTime": "2026-08-20T08:00:00",
    "basePrice": 120.00,
    "availableSeats": 39,
    "totalSeats": 40
  }
]
```

Guarde o `id` da viagem desejada — ele será o `tripId` usado no passo 4.

### 3. Ver detalhes de uma viagem (mapa de assentos)

```bash
curl http://localhost:8080/viagens/{tripId}
```

**Resposta `200 OK`:**

```json
{
  "id": "c4a1b2d3-1234-4a5b-9c3d-000000000010",
  "routeId": "b3f1c2a4-1234-4a5b-9c3d-000000000001",
  "origin": "São Paulo",
  "destination": "Rio de Janeiro",
  "estimatedDuration": "06:00:00",
  "departureDateTime": "2026-08-20T08:00:00",
  "basePrice": 120.00,
  "totalSeats": 40,
  "seats": [
    { "seatNumber": 1, "isOccupied": false },
    { "seatNumber": 2, "isOccupied": true }
  ]
}
```

**`404 Not Found`** se o `tripId` não existir.

### 4. Criar uma reserva

```bash
curl -X POST http://localhost:8080/reservas \
  -H "Content-Type: application/json" \
  -d '{
        "passengerName": "Maria da Silva",
        "passengerCpf": "529.982.247-25",
        "passengerEmail": "maria@example.com",
        "passengerBirthDate": "1990-05-10",
        "tripId": "{tripId}",
        "seatNumber": 5
      }'
```

**Resposta `201 Created`:**

```json
{
  "reservationCode": "ABC-12345",
  "status": "Confirmed",
  "tripId": "c4a1b2d3-1234-4a5b-9c3d-000000000010",
  "origin": "São Paulo",
  "destination": "Rio de Janeiro",
  "departureDateTime": "2026-08-20T08:00:00",
  "basePrice": 120.00,
  "seatNumber": 5,
  "passengerName": "Maria da Silva",
  "passengerCpf": "529.982.247-25",
  "passengerEmail": "maria@example.com",
  "createdAtUtc": "2026-08-15T10:00:00Z",
  "cancelledAtUtc": null
}
```

Guarde o `reservationCode` — é ele que identifica a passagem nos próximos passos.

Possíveis erros: `400` (CPF inválido ou assento fora do intervalo), `404` (viagem inexistente), `409` (assento já ocupado ou viagem já partida).

### 5. Consultar uma reserva pelo código

```bash
curl http://localhost:8080/reservas/ABC-12345
```

Resposta idêntica à do passo 4. **`404 Not Found`** se o código não existir.

### 6. Cancelar uma reserva

Só é aceito **até 2 horas antes** da partida da viagem.

```bash
curl -X DELETE http://localhost:8080/reservas/ABC-12345
```

- **`204 No Content`** — cancelada com sucesso.
- **`404 Not Found`** — código inexistente.
- **`409 Conflict`** — faltam menos de 2h para a partida, ou a reserva já estava cancelada.

---

## Documentação via Swagger

A API expõe documentação interativa OpenAPI/Swagger, pensada para consumo externo:

- **Swagger UI**: `/swagger` — interface navegável, com todos os endpoints, modelos de request/response e exemplos.
- **Especificação OpenAPI (JSON)**: `/swagger/v1/swagger.json` — pode ser importada em Postman, Insomnia, geradores de client (NSwag, OpenAPI Generator), etc.
- Todos os endpoints têm comentários XML documentados (parâmetros, códigos de resposta possíveis) que aparecem diretamente na UI, gerados a partir dos comentários `///` nos controllers e DTOs.

---

## Testes automatizados

### Testes unitários (`OniBusExpress.UnitTests`)

Cobrem as regras de negócio isoladamente, sem tocar banco de dados:

- **`CpfTests`** — validação de formato e dígito verificador do CPF (casos válidos, inválidos, sequências repetidas).
- **`TripBookingRulesTests`** — assento já ocupado, viagem já partida, número de assento fora do intervalo.
- **`BookingCancellationTests`** — cancelamento dentro/fora do prazo de 2h (incluindo o limite exato).
- **`ReservationCodeGeneratorTests`** — formato do código gerado e nova tentativa em caso de colisão.

### Testes de integração (`OniBusExpress.IntegrationTests`)

Sobem a aplicação real (`WebApplicationFactory<Program>`) — controllers, middleware de exceções — usando **SQLite in-memory** no lugar do PostgreSQL, sem depender de infraestrutura externa:

- Busca de rotas/viagens e consulta de detalhes com mapa de assentos.
- Criação de reserva (sucesso, assento duplicado, CPF inválido, viagem inexistente).
- Consulta e cancelamento de reserva (sucesso, fora do prazo, código inexistente).
- Geração da documentação Swagger.

### Executando os testes

```bash
dotnet test
```

---

## Migrações do banco de dados

As migrations do EF Core ficam em `src/backend/OniBusExpress.Infra/Persistence/Migrations`. Para criar uma nova migration após alterar o modelo:

```bash
dotnet tool install --global dotnet-ef   # se ainda não tiver o CLI
dotnet ef migrations add NomeDaMigration \
  --project src/backend/OniBusExpress.Infra \
  --startup-project src/backend/OniBusExpress.Api \
  --output-dir Persistence/Migrations
```

As migrations são aplicadas automaticamente no startup da API (`DbInitializer.InitializeAsync`), tanto local quanto no container Docker.

---

## Decisões de projeto (backend)

- **PostgreSQL** como banco relacional (via Npgsql), por ser gratuito, leve em Docker e amplamente usado com .NET.
- **Regras de negócio nas entidades**, não em serviços externos: `Trip` e `Booking` protegem suas próprias invariantes (assento ocupado, prazo de cancelamento), o que é o cerne de DDD tático — o domínio nunca pode ficar em estado inválido, independentemente de quem o chama.
- **CPF como value object**: evita que um CPF inválido circule pelo sistema como uma `string` qualquer; a validade é garantida pelo próprio tipo.
- **Índice único parcial** em `Bookings (TripId, SeatNumber) WHERE Status = 'Confirmed'`: além da checagem em memória feita pelo agregado `Trip`, o banco também impede duas reservas confirmadas para o mesmo assento sob concorrência.
- **SQLite in-memory nos testes de integração**: evita a necessidade de subir um PostgreSQL (ou TestContainers/Docker) só para rodar a suíte de testes, mantendo-a rápida e portátil, enquanto ainda exercita o pipeline HTTP real de ponta a ponta.

---

# FRONTEND

Aplicação **React 18 + TypeScript** que implementa as 4 telas do fluxo de compra de passagens (item 4 do desafio), consumindo diretamente os endpoints da API descrita acima. Vive em [`src/frontend`](src/frontend).

## Sumário do frontend

- [Tecnologias e por quê](#tecnologias-e-por-quê)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Telas e funcionalidades](#telas-e-funcionalidades)
- [Decisões de projeto (frontend)](#decisões-de-projeto-frontend)
- [Como executar](#como-executar-1)
- [Variáveis de ambiente](#variáveis-de-ambiente)
- [Testes automatizados (frontend)](#testes-automatizados-frontend)
- [Limitações conhecidas](#limitações-conhecidas)

## Tecnologias e por quê

| Categoria | Tecnologia | Por quê |
|---|---|---|
| Base | React 18 + TypeScript, via **Vite** | Vite é o scaffolding padrão atual para SPAs React (Create React App está descontinuado); dev server instantâneo e build de produção otimizado com pouquíssima configuração. |
| Roteamento | React Router 7 | 4 telas com navegação entre elas e parâmetros de URL (`/viagens/:tripId/assentos`) — é o padrão de fato para SPAs React. |
| Estado | Context API (`BookingContext`) | Ver [Decisões de projeto](#decisões-de-projeto-frontend). |
| Estilo | CSS puro + CSS Modules (nativo do Vite, sem config extra) | O desafio pede um "layout simples e funcional" — uma biblioteca de componentes (Material UI, Chakra, etc.) seria peso desnecessário para 4 telas. CSS Modules evita colisão de nomes de classe sem precisar de Tailwind/styled-components. |
| Testes | Vitest + React Testing Library + `@testing-library/user-event` | Vitest reaproveita a config do Vite (mesmo `esbuild`, zero configuração adicional) e tem API compatível com Jest; RTL testa o comportamento visível ao usuário, não detalhes de implementação. |
| Container | Docker multi-stage (Node para build → **Nginx** para servir os arquivos estáticos) | É o padrão para SPAs: o Node só existe durante o build; em produção só roda um servidor estático leve. |

## Estrutura de pastas

```
src/frontend/
  src/
    components/     # Componentes reutilizáveis (Header, SeatMap, TripCard, Stepper, ...)
    pages/           # Uma página por tela do fluxo (Search, SeatSelection, Checkout, BookingLookup)
    services/        # Camada de acesso à API (apiClient, routesService, tripsService, bookingsService)
    context/         # BookingContext: estado compartilhado entre as Telas 1→2→3
    types/           # Tipos TypeScript espelhando os DTOs da API
    utils/           # Validação/formatação de CPF, datas e moeda
    constants/       # Pequenas constantes compartilhadas (rótulos do Stepper)
    __tests__/       # Testes, espelhando a estrutura acima (utils/, components/, pages/, services/)
  Dockerfile
  nginx.conf
  vite.config.ts     # Config do Vite + Vitest (test runner)
  .env.example
```

Essa divisão em `components` / `pages` / `services` segue a separação clássica de SPAs React: **pages** compõem a tela a partir de **components** reutilizáveis, e toda comunicação de rede fica isolada em **services** — nenhum componente chama `fetch` diretamente, o que torna os componentes de UI testáveis sem precisar mockar rede (só mockar o módulo do service).

## Telas e funcionalidades

### Tela 1 — Busca de passagens (`pages/SearchPage.tsx`)
Formulário com origem, destino (com sugestões via `<datalist>` alimentadas por `GET /rotas`) e data de ida; botão buscar chama `GET /viagens`. Mostra spinner de carregamento, mensagem quando não há resultados, e a lista de viagens encontradas em `TripCard` (preço, horário, vagas restantes — com aviso visual de "sem vagas" quando `availableSeats` é 0).

### Tela 2 — Seleção de assento (`pages/SeatSelectionPage.tsx`)
Ao selecionar uma viagem, navega para `/viagens/:tripId/assentos`, que busca `GET /viagens/{id}` e renderiza o `SeatMap`: um grid de botões coloridos por estado (livre / ocupado / selecionado), com legenda e rótulos acessíveis (`aria-label`, `aria-pressed`). Mostra também rota, data/hora, duração e preço da viagem. O botão "Continuar" só habilita depois de um assento livre ser escolhido.

### Tela 3 — Dados do passageiro e confirmação (`pages/CheckoutPage.tsx`)
Formulário com nome completo, CPF (com máscara automática `000.000.000-00` e validação de dígito verificador — mesmo algoritmo do backend, reimplementado em `utils/cpf.ts`), e-mail e data de nascimento (campo adicional exigido pelo contrato de `POST /reservas`, já que o backend modela `Passenger.BirthDate`). Um resumo da compra (rota, data, assento, preço) fica sempre visível acima do formulário. A validação roda no frontend antes de qualquer chamada à API; ao confirmar, chama `POST /reservas` e, em caso de sucesso, troca a tela pela confirmação com o **código da reserva** em destaque.

### Tela 4 (bônus) — Consulta de reserva (`pages/BookingLookupPage.tsx`)
Campo para digitar o código (ex.: `ABC-12345`), que consulta `GET /reservas/{codigo}`. Mostra os detalhes da reserva e seu status (Confirmada/Cancelada); se ainda estiver confirmada, exibe o botão **Cancelar reserva**, que chama `DELETE /reservas/{codigo}` e trata o retorno 409 (fora do prazo de 2h) com uma mensagem de erro amigável em vez de deixar a exceção estourar.

Um `Stepper` (Busca → Assento → Confirmação) é exibido no topo das Telas 1–3 para deixar claro o progresso dentro do fluxo de compra.

## Decisões de projeto (frontend)

- **Context API em vez de Redux/Zustand**: o estado compartilhado entre telas é pequeno (viagem e assento escolhidos) e usado só dentro do fluxo linear Busca→Assento→Confirmação — não precisa ser lido fora da árvore React, nem de time-travel debugging, nem de persistência entre sessões. Uma dependência extra não se pagaria aqui.
- **Uma camada `services/` sem Axios**: um wrapper fino sobre `fetch` (`apiClient.ts`) já cobre tudo que a API precisa (JSON, headers, tratamento de erro via `ProblemDetails`), sem adicionar uma dependência só para isso.
- **Tipos manuais em `types/api.ts` em vez de geração automática (OpenAPI Codegen)**: a API expõe poucos DTOs e estáveis; gerar tipos automaticamente a partir do `swagger.json` adicionaria uma etapa de build extra para um ganho pequeno nesta escala. Ficou documentado no próprio arquivo para reavaliação futura.
- **CPF validado duas vezes (frontend e backend)**: a validação no frontend (`utils/cpf.ts`, mesmo algoritmo do backend) é só para dar feedback imediato ao usuário — a validação que realmente importa continua acontecendo no servidor (`Cpf.Create`), que é a única fonte de verdade.
- **`__tests__/` centralizado em vez de arquivos `*.test.tsx` ao lado de cada componente**: seguindo a estrutura de pastas pedida (`src/__tests__/`), os testes ficam agrupados e espelham a árvore de `components/pages/services/utils`, facilitando localizar a cobertura de uma camada específica.
- **Nginx para servir o build em produção**: um SPA compilado é só HTML/CSS/JS estático — não há motivo para manter um processo Node rodando em produção só para servir arquivos. O `nginx.conf` inclui o fallback de SPA (`try_files ... /index.html`) para as rotas do React Router funcionarem em recarregamentos de página.

## Como executar

### Opção 1 — Docker Compose (junto com API e banco)

```bash
docker-compose up --build
```

- Frontend: http://localhost:3000
- API: http://localhost:8080 (necessária, pois o frontend chama `http://localhost:8080` a partir do navegador)

### Opção 2 — Localmente (desenvolvimento)

```bash
cd src/frontend
npm install
cp .env.example .env.local   # já vem com VITE_API_URL=http://localhost:5083 (API local, fora do Docker)
npm run dev
```

Acesse http://localhost:5173. A API precisa estar rodando em `http://localhost:5083` (`dotnet run --project src/backend/OniBusExpress.Api`, veja [Como executar](#como-executar) do backend) — o CORS já está liberado (`AllowAny`) para o dev server do Vite.

> Rodando via Docker Compose em vez de `npm run dev`? A API fica em `http://localhost:8080`, não `5083` — é por isso que o `docker-compose.yml` passa `VITE_API_URL=http://localhost:8080` como build arg do frontend (ver [Variáveis de ambiente](#variáveis-de-ambiente)). Os dois cenários (local puro vs. Docker) usam portas diferentes de propósito; o importante é que `VITE_API_URL` e a porta em que a API está de fato escutando sempre batam.

### Build de produção (sem Docker)

```bash
npm run build     # gera src/frontend/dist
npm run preview   # serve o build localmente para conferência
```

## Variáveis de ambiente

Todas lidas em tempo de **build** (padrão do Vite — não são variáveis de runtime do container):

| Variável | Padrão (`.env.example` / local) | Docker Compose | Descrição |
|---|---|---|---|
| `VITE_API_URL` | `http://localhost:5083` | `http://localhost:8080` | URL base da API. |

No Docker, `VITE_API_URL` é passada como `build arg` no `docker-compose.yml` (porque precisa apontar para a porta publicada no host, já que quem faz a chamada é o navegador do usuário, não outro container). Fora do Docker, a API roda na porta do `launchSettings.json` (`5083`) — por isso o padrão local é diferente do padrão usado em produção/Docker.

## Testes automatizados (frontend)

18 testes (`vitest run`), cobrindo onde há mais valor em testar uma SPA:

- **`utils/cpf.test.ts`** — validação e formatação de CPF (mesmos casos válidos/inválidos do backend).
- **`components/SeatMap.test.tsx`** — assentos ocupados ficam desabilitados, clique em assento livre dispara a seleção, assento selecionado expõe `aria-pressed`.
- **`pages/SearchPage.test.tsx`** — estado de carregamento, lista de resultados, mensagem de "nenhuma viagem encontrada" e mensagem de erro de rede (services mockados com `vi.mock`).
- **`services/bookingsService.test.ts`** — `createBooking`/`cancelBooking` chamam a API sem enviar nenhum header de autenticação; `getBookingByCode` retorna `null` (não lança) quando a API responde 404.

```bash
cd src/frontend
npm test          # roda uma vez (CI)
npm run test:watch  # modo watch para desenvolvimento
```

> Todo o fluxo (buscar viagem → ver assentos → criar reserva → consultar → cancelar) também foi validado manualmente contra a API real rodando localmente com PostgreSQL, confirmando que os tipos em `types/api.ts` batem exatamente com o JSON retornado pelo backend.

## Limitações conhecidas

- Não há paginação na busca de viagens (a API atual também não pagina `GET /viagens`).
- O mapa de assentos é um grid simples (sem representar corredor/fileiras físicas do ônibus) — suficiente para o requisito de "livre/ocupado/selecionado", mas não é um layout realista de ônibus.
