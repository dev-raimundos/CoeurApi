using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases.GetAll;
using CoeurApi.Modules.Users.Domain;
using Moq;

namespace CoeurApi.Tests.Modules.Users;

public class GetAllUsersUseCaseTests
{
    private readonly Mock<IUsersRepository> _repository = new();

    private GetAllUsersUseCase CreateUseCase() => new(_repository.Object);

    [Fact]
    public async Task ExecuteAsync_DeveRetornarPaginaMapeadaComTotalCount()
    {
        var users = new List<User>
        {
            User.Create("Fulano", "fulano@teste.com", "hash"),
            User.Create("Ciclano", "ciclano@teste.com", "hash")
        };
        _repository.Setup(r => r.GetAllAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, 2));

        var useCase = CreateUseCase();
        var result = await useCase.ExecuteAsync(1, 10);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(users[0].Email, result.Items[0].Email);
    }

    [Fact]
    public async Task ExecuteAsync_SemUsuarios_DeveRetornarListaVazia()
    {
        _repository.Setup(r => r.GetAllAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        var useCase = CreateUseCase();
        var result = await useCase.ExecuteAsync(1, 10);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
