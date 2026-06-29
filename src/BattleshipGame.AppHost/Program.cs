var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var db = postgres.AddDatabase("battleship");

var migrations = builder
    .AddProject<Projects.BattleshipGame_MigrationRunner>("migrations")
    .WithReference(db)
    .WaitFor(postgres);

// Battleship Web API
builder
    .AddProject<Projects.BattleshipGame_WebAPI>("webapi")
    .WithReference(db)
    .WaitForCompletion(migrations);

// Note: OpenAI-compatible API is managed externally.
// For local development, you should have Ollama is installed and running.
// For hosted environment, your should have a Cloud-based model provider.
// In both cases, an API URL and Key should be available in configuration.

builder.Build().Run();
