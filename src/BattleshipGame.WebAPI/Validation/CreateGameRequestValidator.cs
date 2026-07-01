using BattleshipGame.WebAPI.Controllers;
using FluentValidation;

namespace BattleshipGame.WebAPI.Validation;

public sealed class CreateGameRequestValidator : AbstractValidator<CreateGameRequest>
{
    public CreateGameRequestValidator()
    {
        // The owner is derived from the authenticated token, not the request body.
        RuleFor(x => x.BoardSize).InclusiveBetween(10, 26).When(x => x.BoardSize.HasValue);
    }
}
