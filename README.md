# OrderFlow

Sistema de gestão de pedidos com processamento assíncrono via mensageria e um assistente de IA que responde perguntas sobre os pedidos.

| Pasta | Descrição | Stack |
|-------|-----------|-------|
| [OrderFlowApi](OrderFlowApi/) | API REST de pedidos + assistente de IA | .NET 10, EF Core, PostgreSQL, RabbitMQ, Ollama |
| [OrderFlowWorkers](OrderFlowWorkers/) | Worker que processa os pedidos | .NET 10, RabbitMQ, EF Core |
| [OrderFlowWeb](OrderFlowWeb/) | Interface web | React, Vite, TypeScript, Tailwind, shadcn/ui |

## Fluxo

```
Web ──HTTP──▶ API ──▶ PostgreSQL (pedido + outbox, mesma transação)
                                   │
                        OutboxProcessor ──order.created──▶ RabbitMQ ──▶ Worker ──▶ PostgreSQL
                                                                    (Pendente → Processando → 5s → Finalizado)
```

1. O usuário cria um pedido na Web; a API grava o pedido com status **Pendente** e o evento `order.created` na outbox, na mesma transação. Um processo em segundo plano publica o evento no RabbitMQ, então nenhum pedido fica sem evento, mesmo com o broker fora do ar.
2. O Worker consome a mensagem, muda para **Processando** e, após 5 segundos, para **Finalizado**. Cada mudança de status fica registrada no histórico do pedido.
3. A Web atualiza a lista automaticamente e avisa quando um status muda.
4. No card "Pergunte sobre os pedidos", uma LLM local responde perguntas usando dados reais.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)
- [Docker](https://www.docker.com/products/docker-desktop/)
- [Ollama](https://ollama.com/download), apenas para o assistente de IA

## Como rodar

```bash
# 1. Infraestrutura (PostgreSQL + RabbitMQ)
docker compose up -d

# 2. Modelo de linguagem (uma única vez, ~4,7 GB)
ollama pull qwen2.5:7b

# 3. API (aplica as migrations automaticamente em desenvolvimento)
cd OrderFlowApi && dotnet run --project src/OrderFlow.Api

# 4. Worker (em outro terminal)
cd OrderFlowWorkers && dotnet run --project src/OrderFlow.Worker

# 5. Web (em outro terminal)
cd OrderFlowWeb && npm install && npm run dev
```

| Serviço | Endereço |
|---------|----------|
| Web | http://localhost:5173 |
| API (Swagger) | http://localhost:5099/swagger |
| RabbitMQ (painel) | http://localhost:15672 (usuário/senha `orderflow`) |
| PostgreSQL | `localhost:5433` (usuário/senha `orderflow`) |

O PostgreSQL usa a porta **5433** no host para não conflitar com instalações locais.

Sem o Ollama, todo o sistema funciona normalmente; apenas o assistente responde "indisponível".

## Testes

```bash
cd OrderFlowApi && dotnet test
cd OrderFlowWorkers && dotnet test
```
