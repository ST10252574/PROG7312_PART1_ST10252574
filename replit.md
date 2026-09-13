# Smart-X IoT Gateway

Client-facing telemetry ingestion and validation dashboard for a simulated Smart-X IoT mesh, with a .NET 10 Visual Studio API package.

## Run & Operate

- `pnpm --filter @workspace/api-server run dev` — run the API server (port 5000)
- `pnpm --filter @workspace/smartx-gateway run dev` — run the React dashboard
- `pnpm run typecheck` — full typecheck across all packages
- `pnpm run build` — typecheck + build all packages
- `pnpm --filter @workspace/api-spec run codegen` — regenerate API hooks and Zod schemas from the OpenAPI spec
- `pnpm --filter @workspace/db run push` — push DB schema changes (dev only)
- Required env: `DATABASE_URL` — Postgres connection string

## Stack

- pnpm workspaces, Node.js 24, TypeScript 5.9
- API: Express 5
- DB: PostgreSQL + Drizzle ORM
- Validation: Zod (`zod/v4`), `drizzle-zod`
- API codegen: Orval (from OpenAPI spec)
- Build: esbuild (CJS bundle)

## Where things live

- `lib/api-spec/openapi.yaml` — source-of-truth API contract
- `artifacts/api-server/src/routes/smartx.ts` — local preview API implementation
- `artifacts/smartx-gateway` — React dashboard
- `visual-studio/SmartX.Gateway.sln` — .NET 10 Minimal API for Visual Studio
- `docs/part1-research.md` — Part 1 research paper

## Architecture decisions

- The OpenAPI contract is shared by the React client and the local Express preview API.
- The ZIP includes a separate .NET 10 Minimal API because the assessment requires .NET as the backend backbone.
- In-memory seeded data is intentional for the assessment simulation; persistence belongs in Part 2.
- The engagement surface implements proactive operator alerts, based on the research strategy selected for Task 1.

## Product

Operators can register sensors, inspect recent telemetry, validate a reading before ingestion, review activity, and configure the active engagement trigger and delivery channel.

## User preferences

_Populate as you build — explicit user instructions worth remembering across sessions._

## Gotchas

- Run `pnpm --filter @workspace/api-spec run codegen` after changing the OpenAPI spec.
- The Visual Studio solution requires the .NET 10 SDK; the Replit preview uses the Express mirror because the container does not include the .NET CLI.

## Pointers

- See the `pnpm-workspace` skill for workspace structure, TypeScript setup, and package details
