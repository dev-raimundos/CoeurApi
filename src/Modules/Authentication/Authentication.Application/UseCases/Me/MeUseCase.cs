using CoeurApi.Application.Abstractions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.SharedKernel.Exceptions;

namespace CoeurApi.Modules.Authentication.Application.UseCases.Me;

public class MeUseCase(IUsersRepository repository, ICurrentUser currentUser)
{
    private const string ErrNotFound = "Usuário não encontrado.";

    public async Task<UserResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByIdAsync(currentUser.Id, cancellationToken)
            ?? throw HttpException.NotFound(ErrNotFound);

        return UserResponse.FromEntity(user);
    }
}
