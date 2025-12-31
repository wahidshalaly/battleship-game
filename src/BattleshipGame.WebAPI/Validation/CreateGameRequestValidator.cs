using BattleshipGame.WebAPI.Controllers;
using FluentValidation;

namespace BattleshipGame.WebAPI.Validation;

public sealed class CreateGameRequestValidator : AbstractValidator<CreateGameRequest>
{
    public CreateGameRequestValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();

        RuleFor(x => x.BoardSize).InclusiveBetween(10, 26).When(x => x.BoardSize.HasValue);
    }
}
