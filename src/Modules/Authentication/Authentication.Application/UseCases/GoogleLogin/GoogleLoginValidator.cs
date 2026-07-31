using FluentValidation;

namespace CoeurApi.Modules.Authentication.Application.UseCases.GoogleLogin;

public class GoogleLoginValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("IdToken é obrigatório.");
    }
}
