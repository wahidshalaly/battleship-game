var builder = DistributedApplication.CreateBuilder(args);

// Battleship Web API
builder.AddProject<Projects.BattleshipGame_WebAPI>("webapi");

// Note: OpenAI-compatible API is managed externally.
// For local development, you should have Ollama is installed and running.
// For hosted environment, your should have a Cloud-based model provider.
// In both cases, an API URL and Key should be available in configuration.

builder.Build().Run();
