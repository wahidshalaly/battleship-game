using BattleshipGame.WebAPI.Controllers;
using FluentValidation;

namespace BattleshipGame.WebAPI.Validation;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(32)
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Username must be 3-32 chars, letters, digits, underscore only.");

        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
