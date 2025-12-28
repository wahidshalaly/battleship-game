using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BattleshipGame.WebAPI.Filters;

/// <summary>
/// Enriches logs with GameId and PlayerId from route values for traceability.
/// Works with OpenTelemetry Activity tags for distributed tracing.
/// </summary>
public sealed class DomainContextEnricherFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var activity = Activity.Current;

        // Extract GameId from route or action parameters
        if (context.RouteData.Values.TryGetValue("id", out var idValue) && idValue is string idStr)
        {
            if (Guid.TryParse(idStr, out var gameId))
            {
                activity?.SetTag("game.id", gameId.ToString());
                context.HttpContext.Items["GameId"] = gameId;
            }
        }

        // Extract PlayerId from request body if available (CreateGameRequest, etc.)
        if (context.ActionArguments.TryGetValue("request", out var requestObj))
        {
            var playerIdProp = requestObj?.GetType().GetProperty("PlayerId");
            if (playerIdProp?.GetValue(requestObj) is Guid playerId)
            {
                activity?.SetTag("player.id", playerId.ToString());
                context.HttpContext.Items["PlayerId"] = playerId;
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No cleanup needed; Activity tags are automatically propagated
    }
}
