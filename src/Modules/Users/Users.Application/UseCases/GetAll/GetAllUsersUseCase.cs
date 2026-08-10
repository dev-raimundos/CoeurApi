using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.SharedKernel.Common;

namespace CoeurApi.Modules.Users.Application.UseCases.GetAll;

public class GetAllUsersUseCase(IUsersRepository repository)
{
    public async Task<PagedResult<UserResponse>> ExecuteAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var (users, totalCount) = await repository
            .GetAllAsync(page, pageSize, cancellationToken);

        var result = new PagedResult<UserResponse>(
            users.Select(UserResponse.FromEntity).ToList(),
            page,
            pageSize,
            totalCount
        );

        return result;
    }
}
