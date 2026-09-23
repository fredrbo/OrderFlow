# OrderFlow Web

Interface web do OrderFlow em React + Vite + TypeScript.

## Funcionalidades

- Listagem de pedidos em tabela responsiva (colunas secundárias se adaptam em telas menores)
- Criação de pedidos via formulário com validação (mesmas regras da API)
- Visualização dos detalhes do pedido, incluindo data de finalização
- Feedback visual de mudança de status: a linha é destacada e um toast é exibido
- Atualização automática adaptativa: a cada 2s enquanto houver pedidos em andamento e a cada 15s quando todos estiverem finalizados, com indicador do horário da última atualização
- "Pergunte sobre os pedidos": perguntas em linguagem natural respondidas por IA com dados reais
- Avisos claros quando a API está indisponível, mantendo os últimos dados carregados

## Stack

| Item | Uso |
|------|-----|
| [Vite](https://vite.dev) + React 19 + TypeScript | Base da aplicação |
| [Tailwind CSS](https://tailwindcss.com) + [shadcn/ui](https://ui.shadcn.com) | Estilo e componentes |
| [TanStack Query](https://tanstack.com/query) | Chamadas à API, cache e atualização automática |
| [React Hook Form](https://react-hook-form.com) + [Zod](https://zod.dev) | Formulário e validação |

## Estrutura

```
src/
├── api/http-client.ts        Cliente HTTP (fetch) + tratamento de erros ProblemDetails
├── features/
│   ├── orders/               Pedidos: api, components, hooks, schemas e tipos
│   └── assistant/            Card de perguntas sobre os pedidos
├── components/ui/            Componentes shadcn/ui
└── lib/                      Utilitários (formatação de moeda/data, cn)
```

Organização por *feature*: cada funcionalidade concentra sua API, componentes, hooks e tipos.

## Como rodar

Pré-requisitos: Node 20+ e a [OrderFlow API](../OrderFlowApi) rodando em `http://localhost:5099`.

```bash
npm install
npm run dev
```

Acesse http://localhost:5173. A URL da API pode ser alterada criando um `.env.local` (veja [`.env.example`](.env.example)).

## Scripts

| Comando | Descrição |
|---------|-----------|
| `npm run dev` | Servidor de desenvolvimento |
| `npm run build` | Type-check + build de produção |
| `npm run lint` | Lint com oxlint |
| `npm run preview` | Serve o build de produção localmente |
