using BattleshipGame.WebAPI.Controllers;
using FluentValidation;

namespace BattleshipGame.WebAPI.Validation;

public sealed class PlaceShipRequestValidator : AbstractValidator<PlaceShipRequest>
{
    public PlaceShipRequestValidator()
    {
        RuleFor(x => x.Side).IsInEnum();
        RuleFor(x => x.ShipKind).IsInEnum();
        RuleFor(x => x.Orientation).IsInEnum();
        RuleFor(x => x.BowCode)
            .NotEmpty()
            .Matches("^[A-Z][1-9][0-9]?$")
            .WithMessage("Bow code must be like A1..Z26.");
    }
}
