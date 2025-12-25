using BattleshipGame.Domain.DomainModel.GameAggregate;
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

public sealed class AttackRequestValidator : AbstractValidator<AttackRequest>
{
    public AttackRequestValidator()
    {
        RuleFor(x => x.Side).IsInEnum();
        RuleFor(x => x.Cell)
            .NotEmpty()
            .Matches("^[A-Z][1-9][0-9]?$")
            .WithMessage("Cell must be like A1..Z26.");
    }
}
