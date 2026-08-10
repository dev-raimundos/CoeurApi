using CoeurApi.Modules.Users.Domain;

namespace CoeurApi.Modules.Users.Application.Abstractions;

public interface IUsersRepository
{
    public Task<(List<User> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    public Task AddAsync(User user, CancellationToken cancellationToken = default);
    public void Delete(User user);
}