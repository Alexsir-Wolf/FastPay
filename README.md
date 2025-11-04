# FastPay API

API financeira.

## Visão Geral

- Arquitetura em camadas com DDD leve: `Domain`, `Application`, `Infra.Data`, `Infra.IoC` e `Api`.
- Persistência com EF Core + PostgreSQL; migrações versionadas.
- Endpoints minimalistas com Carter (Minimal APIs) + OpenAPI/Swagger em Development.
- Consistência transacional (isolation level Serializable) e idempotência por `referenceId`.
- Logging estruturado com Serilog (console e opcionalmente PostgreSQL) e correlação via `X-Correlation-Id`.
- Métricas HTTP com `prometheus-net`, painel pronto no Grafana.

## Decisões Técnicas e Arquiteturais

- Camadas e responsabilidades:
  - Domain: entidades, invariantes e regras de negócio (`Account`, `Transaction`, `Money`, `DomainException`).
  - Application: casos de uso com CQRS simples (Commands/Queries + Handlers), validação (FluentValidation), logging (pipeline MediatR), eventos internos.
  - Infra.Data: EF Core (DbContext, mapeamentos, repositórios, migrações) e `UnitOfWork` com retry/backoff para concorrência.
  - Infra.IoC: composição de dependências, registro de Handlers/Validators/Behaviors e Event Bus em memória.
  - Api: endpoints Carter organizados por versão, middlewares de correlação e exceções, Swagger e métricas.
- Consistência e concorrência:
  - `IUnitOfWork.ExecuteInTransactionAsync` usa isolamento Serializable com até 3 tentativas e backoff exponencial para `SerializationFailure` e deadlocks.
  - Idempotência com índice único `(account_id, operation, reference_id)` em `transactions` e verificação de duplicidade antes de aplicar a operação.
- Modelo monetário:
  - `Money` encapsula `decimal` com duas casas e conversão para/desde centavos (`long`) evitando imprecisão.
- Eventos internos (event-driven):
  - `IEventPublisher` + `InMemoryEventBus` (fila Channel + `BackgroundService`) despacha eventos como `TransactionProcessedEvent` com retries e backoff.
- Observabilidade:
  - Serilog com correlação, enriquecimento de contexto e sink opcional para Postgres (`fast_pay_logs`).
  - `prometheus-net` expõe `/metrics`; compose inclui Prometheus + Grafana.

## Bibliotecas e Justificativas

- Carter: endpoints minimalistas e organização por módulo; integração fácil com OpenAPI.
- MediatR: separa transporte HTTP da aplicação, facilita testes e behaviors (logging/validação).
- FluentValidation: validação declarativa reutilizável e mensagens claras.
- EF Core + Npgsql: produtividade e suporte robusto a PostgreSQL; migrações e mapeamentos explícitos.
- Serilog (+ sinks): logging estruturado (console, Postgres), enriquecimento de escopo e correlação.
- prometheus-net: métricas HTTP padrão para Prometheus.
- xUnit + NSubstitute: testes de unidade e mocks simples.

## Requisitos

- .NET SDK 9.0
- PostgreSQL 14+ (recomendado latest)
- Docker e Docker Compose (opcional, para stack completa)
- dotnet-ef (CLI de migrações): `dotnet tool install -g dotnet-ef`

## Configuração

- Connection string:
  - Chave: `Settings:PostgresSettings:ConnectionString`
  - Variável de ambiente equivalente: `Settings__PostgresSettings__ConnectionString`
  - Exemplo: `Host=localhost;Port=5432;Database=fastpay_db;Username=fastpay;Password=fastpay`
- Ambiente:
  - `ASPNETCORE_ENVIRONMENT=Development` habilita Swagger em `/swagger`.
- Serilog (opcional Postgres):
  - Se a connection string estiver definida, os logs são gravados automaticamente na tabela `fast_pay_logs`.

## Build e Execução (Local)

1) Restaurar e compilar

```bash
dotnet restore
dotnet build
```

2) Configurar secrets da API (recomendado em dev)

```bash
cd FastPay.Api
dotnet user-secrets set "Settings:PostgresSettings:ConnectionString" "Host=localhost;Port=5432;Database=fastpay_db;Username=fastpay;Password=fastpay"
```

3) Aplicar migrações

- PowerShell (Windows):

```powershell
$env:Settings__PostgresSettings__ConnectionString="Host=localhost;Port=5432;Database=fastpay_db;Username=fastpay;Password=fastpay"; \
dotnet ef database update --project ..\FastPay.Infra.Data --startup-project .
```

- Bash (Linux/macOS):

```bash
Settings__PostgresSettings__ConnectionString="Host=localhost;Port=5432;Database=fastpay_db;Username=fastpay;Password=fastpay" \
dotnet ef database update --project ../FastPay.Infra.Data --startup-project .
```

4) Executar API

```bash
dotnet run --project FastPay.Api
# API: http://localhost:5030
# Swagger (Development): http://localhost:5030/swagger
# Health: /health/live e /health/ready
# Métricas: /metrics
```

## Execução via Docker Compose (stack completa)

Subir infraestrutura (Postgres, API, Prometheus e Grafana):

```bash
docker compose up -d --build
```

Serviços expostos:

- API: `http://localhost:5030`
- Prometheus: `http://localhost:9090`
- Grafana: `http://localhost:3000` (login: admin / senha: admin)

Aplicar migrações a partir do host (com banco do compose na 5432):

```bash
# PowerShell
$env:Settings__PostgresSettings__ConnectionString="Host=localhost;Port=5432;Database=fastpay_db;Username=fastpay;Password=fastpay"; \
dotnet ef database update --project FastPay.Infra.Data --startup-project FastPay.Api
```

Parar serviços:

```bash
docker compose down
```

## Testes

Executar todos os testes:

```bash
dotnet test
```

Projetos:

- `FastPay.Domain.Tests`: regras de domínio (`Account`, `Money`, etc.).
- `FastPay.Application.Tests`: handlers de transações/contas, idempotência, validações e eventos.

Cobertura conceitual:

- Débito usando limite de crédito, reservas/capturas parciais, transferências válidas/inválidas, idempotência por `referenceId`, falhas por moeda divergente e conta inexistente.

## Endpoints e Exemplos de Uso

Headers úteis:

- `X-Correlation-Id: <guid>` (opcional; a API gera se ausente)
- `Content-Type: application/json`

### Accounts (v1)

- Criar conta

```
POST /api/v1/accounts
{
  "clientId": "CLI-001",
  "initialBalance": 100.0,
  "creditLimit": 50.0,
  "currency": "BRL"
}
```

- Buscar por Id

```
GET /api/v1/accounts/{id:int}
```

- Buscar por ClientId

```
GET /api/v1/accounts/{clientId}
```

- Listar contas com paginação

```
GET /api/v1/accounts?clientId=CLI-001&page=1&pageSize=10
```

- Atualizar status

```
PUT /api/v1/accounts/{id:int}/status
{
  "status": "Blocked" // Active | Blocked | Inactive
}
```

- Listar transações da conta

```
GET /api/v1/accounts/{id:int}/transactions?page=1&pageSize=10
```

### Transactions (v1)

Campos:

- `operation`: `credit|debit|reserve|capture|reversal|transfer`
- `sourceAccountId`: int (origem)
- `destinationAccountId`: int (obrigatório em `transfer`)
- `amount`: long em centavos (ex.: 2500 = 25,00)
- `currency`: `BRL|USD|EUR`
- `referenceId`: string única por operação/conta
- `metadata`: objeto livre

Exemplos `curl`:

- Crédito

```bash
curl -X POST http://localhost:5030/api/v1/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "operation":"credit",
    "sourceAccountId":1,
    "destinationAccountId":0,
    "amount":2500,
    "currency":"BRL",
    "referenceId":"TXN-CR-001",
    "metadata":{"source":"pix"}
  }'
```

- Débito

```bash
curl -X POST http://localhost:5030/api/v1/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "operation":"debit",
    "sourceAccountId":1,
    "destinationAccountId":0,
    "amount":1000,
    "currency":"BRL",
    "referenceId":"TXN-DB-001"
  }'
```

- Reserva e Captura Parcial

```bash
# Reserva
curl -X POST http://localhost:5030/api/v1/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "operation":"reserve",
    "sourceAccountId":1,
    "destinationAccountId":0,
    "amount":4000,
    "currency":"BRL",
    "referenceId":"TXN-RS-001"
  }'

# Captura parcial
curl -X POST http://localhost:5030/api/v1/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "operation":"capture",
    "sourceAccountId":1,
    "destinationAccountId":0,
    "amount":1000,
    "currency":"BRL",
    "referenceId":"TXN-CP-001"
  }'
```

- Estorno de débito

```bash
curl -X POST http://localhost:5030/api/v1/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "operation":"reversal",
    "sourceAccountId":1,
    "destinationAccountId":0,
    "amount":500,
    "currency":"BRL",
    "referenceId":"TXN-RV-001"
  }'
```

- Transferência

```bash
curl -X POST http://localhost:5030/api/v1/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "operation":"transfer",
    "sourceAccountId":1,
    "destinationAccountId":2,
    "amount":500,
    "currency":"BRL",
    "referenceId":"TXN-TR-001"
  }'
```

Resposta (exemplo):

```json
{
  "success": true,
  "data": {
    "transactionId": "TXN-CR-001-PROCESSED",
    "status": "Success",
    "balance": 12500,
    "reservedBalance": 0,
    "availableBalance": 12500,
    "timestamp": "2025-11-04T05:58:00Z",
    "errorMessage": null
  },
  "errors": null
}
```

Idempotência:

- Repetir a mesma combinação `sourceAccountId` + `operation` + `referenceId` retorna o mesmo resultado (transação não é duplicada).

## Health, Swagger e Métricas

- Health: `GET /health/live` (liveness) e `GET /health/ready` (readiness ao DB).
- Swagger (Development): `GET /swagger`.
- Prometheus: `GET /metrics` na API. Compose inclui Prometheus e Grafana com dashboard `monitoring/grafana/dashboards/fastpay-overview.json`.

## Estrutura do Repositório (resumo)

- `FastPay.Api`: API, endpoints Carter, middlewares, Dockerfile.
- `FastPay.Application`: Commands/Queries/Handlers, DTOs, validações, behaviors e eventos.
- `FastPay.Domain`: entidades, enums, value objects, contratos de repositório.
- `FastPay.Infra.Data`: EF Core (DbContext, configurations, repositories, migrations).
- `FastPay.Infra.IoC`: registro de DI, event bus in-memory.
- `FastPay.Application.Tests` e `FastPay.Domain.Tests`: testes de aplicação e domínio.
- `docker-compose.yml` e `monitoring/*`: stack de observabilidade.

## Boas Práticas

- Sempre envie `referenceId` único por operação/conta para garantir idempotência.
- Use `currency` consistente com a moeda da conta; divergências são validadas e falham explicitamente.
- Em transferências, origem e destino devem ser diferentes e ter a mesma `currency`.
- Utilize o header `X-Correlation-Id` para rastreabilidade ponta a ponta.

