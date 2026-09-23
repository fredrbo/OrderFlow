# OrderFlow API

API REST de pedidos em .NET 10. Persiste no PostgreSQL, publica um evento no RabbitMQ a cada pedido criado e oferece um assistente de IA que responde perguntas sobre os pedidos.

## Endpoints

| Método | Rota           | Descrição |
|--------|----------------|-----------|
| POST   | `/orders`      | Cria um novo pedido (status inicial `Pendente`) |
| GET    | `/orders`      | Lista todos os pedidos |
| GET    | `/orders/{id}` | Obtém detalhes de um pedido, com o histórico de status |
| POST   | `/orders/ask`  | Responde uma pergunta em linguagem natural sobre os pedidos |

Documentação interativa em http://localhost:5099/swagger e exemplos em [`OrderFlow.Api.http`](src/OrderFlow.Api/OrderFlow.Api.http). Erros seguem o padrão ProblemDetails (RFC 9457).

## Arquitetura

Clean Architecture enxuta, sem frameworks extras (sem MediatR/CQRS):

```
src/
├── OrderFlow.Domain          Entidade Order, OrderStatus e regras de negócio (sem dependências)
├── OrderFlow.Application     Casos de uso (OrderService, OrderAssistant), DTOs e interfaces
├── OrderFlow.Infrastructure  EF Core + PostgreSQL, outbox + publisher do RabbitMQ, cliente da LLM (Ollama)
└── OrderFlow.Api             Controllers, tratamento de erros, Swagger
tests/
└── OrderFlow.UnitTests       Testes de domínio, do caso de uso e das ferramentas do assistente
```

As dependências apontam para dentro: `Api → Application ← Infrastructure` e `Application → Domain`. A Application depende apenas de abstrações (`IOrderRepository`, `IOrderEventOutbox`, `IUnitOfWork`, `IOrderStatistics`, `IChatClient`), o que permite testá-la sem banco, broker ou LLM.

## Mensageria

| Item        | Valor |
|-------------|-------|
| Exchange    | `orderflow.orders` (topic, durável) |
| Routing key | `order.created` |
| Fila        | `orderflow.order-created` (durável) |
| DLQ         | `orderflow.order-created.dlq` (recebe mensagens rejeitadas pelo worker) |

### Outbox Pattern

Gravar o pedido e depois publicar no broker são duas operações independentes: se o RabbitMQ estiver fora do ar entre uma e outra, o pedido fica salvo sem evento e nunca é processado. Para evitar isso:

1. O `POST /orders` grava o pedido **e** o evento na tabela `outbox_messages` em um único `SaveChanges` (mesma transação). A requisição não depende do RabbitMQ.
2. O `OutboxProcessor` (BackgroundService na API) lê as mensagens pendentes com `FOR UPDATE SKIP LOCKED` (seguro com várias instâncias da API), publica com *publisher confirms* e marca como processadas.
3. Assim que um evento é gravado, o processor é acordado e publica na hora; o polling (`Outbox:PollingInterval`, padrão 5s) serve para retentar falhas.
4. Falhas de publicação ficam registradas em `attempts` e `last_error` e são retentadas no próximo ciclo.

A entrega é *at-least-once*: uma mensagem pode ser publicada mais de uma vez (ex.: queda entre publicar e confirmar no banco), por isso o worker é idempotente.

### Mensagem

A mensagem é persistente e seu `message_id` é o id da mensagem na outbox:

```json
{
  "id": "0199...",
  "cliente": "Maria Silva",
  "produto": "Notebook",
  "valor": 4599.90,
  "status": "Pendente",
  "data_criacao": "2026-09-22T12:00:00+00:00"
}
```

## Assistente de IA

`POST /orders/ask` com `{ "pergunta": "Quantos pedidos estão pendentes?" }`.

Usa uma LLM local e gratuita ([Ollama](https://ollama.com) + `qwen2.5:7b`) via [Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai), com **tool calling**: o modelo interpreta a pergunta e escolhe uma das ferramentas abaixo; a API executa a consulta e o modelo redige a resposta.

| Ferramenta | Exemplo de pergunta |
|------------|---------------------|
| `contar_pedidos` | Quantos pedidos temos hoje? Quantos estão pendentes? |
| `somar_valor_pedidos` | Qual o valor total de pedidos finalizados este mês? Quanto a Maria gastou? |
| `tempo_medio_processamento` | Qual o tempo médio para aprovar os pedidos? |
| `agrupar_pedidos` | Qual o valor total por cliente? Qual o produto com maior faturamento? |

- As ferramentas são **somente leitura** e a LLM não escreve SQL: não há como alterar dados pela pergunta.
- Filtros por status, período, cliente e produto (busca parcial, sem diferenciar maiúsculas).
- Datas interpretadas no fuso de Brasília ("hoje", "este mês").
- Se o Ollama estiver indisponível, a API retorna `503` e o restante do sistema segue funcionando.
- Trocar de provedor de LLM exige alterar apenas o registro do `IChatClient` na Infrastructure.

## Como rodar

Pré-requisitos: .NET 10 SDK, Docker e (para o assistente) Ollama com o modelo baixado (`ollama pull qwen2.5:7b`).

```bash
# na raiz do repositório
docker compose up -d

# na pasta OrderFlowApi
dotnet run --project src/OrderFlow.Api
```

Em desenvolvimento as migrations são aplicadas automaticamente e o modelo de linguagem é pré-carregado na inicialização.

## Configuração

Os valores de desenvolvimento estão em `appsettings.Development.json`. Em outros ambientes, use variáveis de ambiente:

| Variável | Descrição |
|----------|-----------|
| `ConnectionStrings__OrderFlow` | Connection string do PostgreSQL |
| `RabbitMq__HostName`, `RabbitMq__UserName`, `RabbitMq__Password` | Conexão com o RabbitMQ (obrigatórias) |
| `Cors__AllowedOrigins__0` | Origem liberada para o frontend |
| `Outbox__PollingInterval`, `Outbox__BatchSize` | Intervalo de verificação da outbox e mensagens por lote (padrão `00:00:05` e `20`) |
| `Llm__Endpoint`, `Llm__Model` | Endereço do Ollama e modelo usado (padrão `http://127.0.0.1:11434` e `qwen2.5:7b`) |

Sem as credenciais do RabbitMQ a API não inicia, evitando subir com configuração incompleta.

## Resiliência

- Retry automático do EF Core para falhas transitórias do banco (ex.: reinício do PostgreSQL).
- Conexão única com o RabbitMQ, recriada sob demanda.
- Outbox: pedidos continuam sendo criados com o RabbitMQ fora do ar, e os eventos são publicados quando ele volta.

## Testes

```bash
dotnet test
```

## Migrations

```bash
dotnet tool restore
dotnet ef migrations add <Nome> --project src/OrderFlow.Infrastructure --startup-project src/OrderFlow.Api --output-dir Persistence/Migrations
```
