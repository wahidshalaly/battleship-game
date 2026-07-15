var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var db = postgres.AddDatabase("battleship");

// Keycloak identity provider — runs in dev mode with the battleship realm imported on startup.
// Admin UI: http://localhost:8080  (admin / admin)
var keycloak = builder
    .AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.1")
    .WithArgs("start-dev", "--import-realm")
    .WithBindMount("./Realms", "/opt/keycloak/data/import", isReadOnly: true)
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    // Gate readiness on the realm's OIDC discovery document: it returns 200 only once Keycloak
    // is serving HTTP *and* the battleship realm has finished importing, so WaitFor(keycloak)
    // below doesn't let the API start against a half-initialized Keycloak.
    .WithHttpHealthCheck("/realms/battleship/.well-known/openid-configuration");

var migrations = builder
    .AddProject<Projects.BattleshipGame_MigrationRunner>("migrations")
    .WithReference(db)
    .WaitFor(postgres);

// Battleship Web API
builder
    .AddProject<Projects.BattleshipGame_WebAPI>("webapi")
    .WithReference(db)
    .WithEnvironment("Authentication__Authority", "http://localhost:8080/realms/battleship")
    .WithEnvironment("Authentication__Audience", "account")
    .WithEnvironment("Authentication__RequireHttpsMetadata", "false")
    .WithEnvironment("Keycloak__BaseUrl", "http://localhost:8080")
    .WithEnvironment("Keycloak__Realm", "battleship")
    .WithEnvironment("Keycloak__ClientId", "battleship-api")
    .WithEnvironment("Keycloak__ClientSecret", "battleship-secret")
    .WithEnvironment("Keycloak__AdminUsername", "admin")
    .WithEnvironment("Keycloak__AdminPassword", "admin")
    .WaitFor(keycloak)
    .WaitForCompletion(migrations);

// The React + Vite frontend (BattleshipGame.Web) is run standalone with `npm run dev`
// on port 5173, not as an Aspire resource. Aspire assigns dynamic ports and proxies
// them, which conflicts with the API's static CORS allow-list; keeping the frontend
// outside Aspire avoids that friction. Its VITE_API_BASE_URL points at the API's fixed
// dev port (5298) via .env.development, and the API allows the 5173 origin via CORS.

// Note: OpenAI-compatible API is managed externally.
// For local development, you should have Ollama is installed and running.
// For hosted environment, your should have a Cloud-based model provider.
// In both cases, an API URL and Key should be available in configuration.

builder.Build().Run();
