using BattleshipGame.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<BattleshipGameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("battleship"))
);

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BattleshipGameDbContext>();

    logger.LogInformation("Applying database migrations...");
    await db.Database.MigrateAsync();
    logger.LogInformation("Migrations applied successfully.");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Migration failed.");
    return 1;
}

return 0;
