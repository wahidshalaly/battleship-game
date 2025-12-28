var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BattleshipGame_WebAPI>("webapi");

builder.Build().Run();
