using BattleshipGame.WebAPI.Controllers;
using FluentValidation;

namespace BattleshipGame.WebAPI.Validation;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
