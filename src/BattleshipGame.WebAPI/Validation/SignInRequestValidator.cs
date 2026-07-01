using BattleshipGame.WebAPI.Controllers;
using FluentValidation;

namespace BattleshipGame.WebAPI.Validation;

public class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
