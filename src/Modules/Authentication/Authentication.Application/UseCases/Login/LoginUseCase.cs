using CoeurApi.Modules.Authentication.Application.Abstractions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Authentication.Application.UseCases.Login;

public class LoginUseCase(IUsersRepository repository, ITokenService tokenService, IUnitOfWork unitOfWork)
{
    private const string ErrInvalidCredentials = "Email ou senha inválidos.";
    private const string ErrAccountLocked = "Conta temporariamente bloqueada por excesso de tentativas. Tente novamente mais tarde.";
    private const string ErrAccountInactive = "Conta desativada.";

    public async Task<AuthResponse> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || user.PasswordHash is null)
        {
            throw HttpException.Unauthorized(ErrInvalidCredentials);
        }

        if (user.IsLocked)
        {
            throw HttpException.Forbidden(ErrAccountLocked);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw HttpException.Unauthorized(ErrInvalidCredentials);
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
