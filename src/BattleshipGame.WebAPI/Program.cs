using System.Reflection;
using BattleshipGame.Application.Common.Extensions;
using BattleshipGame.Infrastructure.Extensions;
using BattleshipGame.Infrastructure.Resilience;
using BattleshipGame.WebAPI.Filters;
using BattleshipGame.WebAPI.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<DomainContextEnricherFilter>();
    options.Filters.Add<ValidationLoggingFilter>();
});

//builder.Services.AddFluentValidationAutoValidation();

// Configure API behavior for validation errors
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

        var errors = context
            .ModelState.Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        logger.LogWarning(
            "Model binding/validation failed for {ActionName}. Errors: {@Errors}",
            context.ActionDescriptor.DisplayName,
            errors
        );

        return new BadRequestObjectResult(
            new ValidationProblemDetails(context.ModelState)
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path,
            }
        );
    };
});

// Configure routing to use lowercase URLs
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Battleship Game API", Version = "1.0" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Read Configuration
builder
    .Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true
    )
    .AddEnvironmentVariables();

// TODO: Review options pattern usage across the solution for consistency
builder
    .Services.AddOptions<AiOpponentResilienceOptions>()
    .Bind(builder.Configuration.GetSection(AiOpponentResilienceOptions.ConfigurationSectionName))
    .ValidateDataAnnotations();

// Register application and infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

// Add request/response logging middleware (should be first to capture all requests)
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Add exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapSwagger();
app.MapDefaultEndpoints();
app.MapControllers();

app.Run();

public partial class Program;
