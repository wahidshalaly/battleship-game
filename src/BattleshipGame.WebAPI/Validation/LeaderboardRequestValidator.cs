using BattleshipGame.WebAPI.Controllers;
using FluentValidation;

namespace BattleshipGame.WebAPI.Validation;

public sealed class LeaderboardRequestValidator : AbstractValidator<LeaderboardRequest>
{
    public const int MaximumTop = 100;

    public LeaderboardRequestValidator()
    {
        RuleFor(x => x.Top).InclusiveBetween(1, MaximumTop);
    }
}
