# OrderFlow Worker

Worker em .NET 10 que consome a fila `orderflow.order-created` do RabbitMQ e processa os pedidos:

**Pendente → Processando → (5 segundos) → Finalizado**, registrando a data de finalização e cada mudança no histórico de status (`order_status_history`), na mesma transação da atualização do pedido.

## Arquitetura

Clean Architecture enxuta, no mesmo padrão da API:

```
src/
├── OrderFlow.Worker.Domain          Order com as transições StartProcessing() e Finish()
├── OrderFlow.Worker.Application     OrderProcessor: regra de processamento e idempotência
├── OrderFlow.Worker.Infrastructure  EF Core + PostgreSQL (mesmas tabelas orders e order_status_history da API)
└── OrderFlow.Worker                 Host, consumer do RabbitMQ e política de retry
tests/
└── OrderFlow.Worker.UnitTests       Testes de domínio e do processador (com FakeTimeProvider)
```

As migrations pertencem à API; o worker apenas lê e atualiza pedidos.

## Garantias de processamento

| Situação | Comportamento |
|----------|---------------|
| Processamento concluído | Ack manual: a mensagem só sai da fila após o pedido ser finalizado |
| Worker cai no meio do processamento | A mensagem volta para a fila e o pedido é retomado de onde parou |
| Mensagem duplicada (sequencial) | Pedido já finalizado é ignorado |
| Mensagens duplicadas simultâneas | Controle de concorrência otimista (`xmin` do PostgreSQL): apenas uma processa, as demais são ignoradas |
| Falha transitória (ex.: banco instável) | Até 3 novas tentativas com backoff exponencial e jitter |
| Falha definitiva ou mensagem inválida | Mensagem enviada para a DLQ `orderflow.order-created.dlq` |
| RabbitMQ indisponível na inicialização | Nova tentativa de conexão a cada 5 segundos |

Processa até 5 mensagens em paralelo.

## Como rodar

Pré-requisitos: .NET 10 SDK, a infraestrutura no ar (`docker compose up -d` na raiz) e a API executada ao menos uma vez (para aplicar as migrations).

```bash
dotnet run --project src/OrderFlow.Worker
```

## Configuração

Em `appsettings.json` (valores de desenvolvimento em `appsettings.Development.json`):

| Chave | Padrão | Descrição |
|-------|--------|-----------|
| `OrderProcessing:ProcessingDelay` | `00:00:05` | Tempo em "Processando" antes de finalizar |
| `OrderProcessing:MaxRetryAttempts` | `3` | Novas tentativas em falhas transitórias |
| `OrderProcessing:RetryBaseDelay` | `00:00:02` | Espera base do backoff exponencial |
| `RabbitMq:MaxConcurrentMessages` | `5` | Mensagens processadas em paralelo |

Em outros ambientes, use variáveis de ambiente (ex.: `ConnectionStrings__OrderFlow`, `RabbitMq__Password`).

## Testes

```bash
dotnet test
```
