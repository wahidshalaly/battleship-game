using Microsoft.AspNetCore.Mvc.Filters;
using Serilog.Context;

namespace BattleshipGame.WebAPI.Filters;

/// <summary>
/// Enriches logs with GameId and PlayerId from route values for traceability.
/// </summary>
public sealed class DomainContextEnricherFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Extract GameId from route or action parameters
        if (context.RouteData.Values.TryGetValue("id", out var idValue) && idValue is string idStr)
        {
            if (Guid.TryParse(idStr, out var gameId))
            {
                LogContext.PushProperty("GameId", gameId);
            }
        }

        // Extract PlayerId from request body if available (CreateGameRequest, etc.)
        if (context.ActionArguments.TryGetValue("request", out var requestObj))
        {
            var playerIdProp = requestObj?.GetType().GetProperty("PlayerId");
            if (playerIdProp?.GetValue(requestObj) is Guid playerId)
            {
                LogContext.PushProperty("PlayerId", playerId);
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No cleanup needed; LogContext scope is disposed automatically
    }
}
