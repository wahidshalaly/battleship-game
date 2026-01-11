using BattleshipGame.WebAPI.Controllers;
using FluentValidation;

namespace BattleshipGame.WebAPI.Validation;

public sealed class AttackRequestValidator : AbstractValidator<AttackRequest>
{
    public AttackRequestValidator()
    {
        RuleFor(x => x.Cell)
            .NotEmpty()
            .Matches("^[A-Z][1-9][0-9]?$")
            .WithMessage("Cell must be like A1..Z26.");
    }
}
