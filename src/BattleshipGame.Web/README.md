# BattleshipGame.Web

React + TypeScript + Vite frontend for the Battleship game API.

## Stack

- **Vite** + **React 19** + **TypeScript**
- **Tailwind CSS v4** (via `@tailwindcss/vite`)
- **TanStack Query** for server-state caching
- **React Router** for navigation
- **openapi-typescript** + **openapi-fetch** for a typed API client generated from
  `../../docs/openapi.yaml`
- **oxlint** for linting, **Prettier** for formatting

## Running locally

The frontend runs standalone against the API. Start the backend first (the API and its
Keycloak/Postgres dependencies) via Aspire, then run the dev server:

```bash
# From the repo root — starts Postgres, Keycloak, migrations, and the Web API
dotnet run --project src/BattleshipGame.AppHost

# In this folder — starts the Vite dev server on http://localhost:5173
npm install   # first time only
npm run dev
```

The dev server calls the API at `VITE_API_BASE_URL` (see `.env.development`, defaults to
`http://localhost:5298`). The API allows the `http://localhost:5173` origin via CORS
(`Cors:AllowedOrigins` in the API's `appsettings.Development.json`).

## Scripts

| Script                       | Description                                               |
| ---------------------------- | --------------------------------------------------------- |
| `npm run dev`                | Start the Vite dev server (port 5173)                     |
| `npm run build`              | Type-check and build for production                       |
| `npm run lint`               | Run oxlint                                                |
| `npm run format`             | Format with Prettier                                      |
| `npm run format:check`       | Check formatting without writing                          |
| `npm run generate:api-types` | Regenerate `src/api/schema.d.ts` from `docs/openapi.yaml` |

Regenerate the API types whenever `docs/openapi.yaml` changes (same cadence as the
backend's `dotnet swagger tofile` step).
