using CoeurApi.Modules.Authentication.Application.Abstractions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.Modules.Users.Domain;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Authentication.Application.UseCases.GoogleLogin;

public class GoogleLoginUseCase(
    IGoogleTokenValidator googleTokenValidator,
    IUsersRepository repository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
{
    private const string ErrInvalidToken = "Token do Google inválido.";
    private const string ErrAccountInactive = "Conta desativada.";

    public async Task<AuthResponse> ExecuteAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default)
    {
        var googleUser = await googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken)
            ?? throw HttpException.Unauthorized(ErrInvalidToken);

        var user = await repository.GetByEmailAsync(googleUser.Email, cancellationToken);

        if (user is null)
        {
            user = User.CreateFromGoogle(googleUser.Name, googleUser.Email);
            await repository.AddAsync(user, cancellationToken);
        }

        if (!user.IsActive)
        {
            throw HttpException.Forbidden(ErrAccountInactive);
        }

        user.RecordLogin();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = tokenService.Generate(user);
        return new AuthResponse(UserResponse.FromEntity(user), token);
    }
}
