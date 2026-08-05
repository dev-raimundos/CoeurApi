using CoeurApi.Application.Abstractions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases.Delete;
using CoeurApi.Modules.Users.Domain;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;
using Moq;

namespace CoeurApi.Tests.Modules.Users;

public class DeleteUserUseCaseTests
{
    private readonly Mock<IUsersRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private DeleteUserUseCase CreateUseCase() => new(_repository.Object, _unitOfWork.Object, _currentUser.Object);

    [Fact]
    public async Task ExecuteAsync_UsuarioTentandoExcluirOutroPerfil_DeveLancarForbidden()
    {
        _currentUser.Setup(c => c.Id).Returns(Guid.NewGuid());
        _currentUser.Setup(c => c.IsAdmin).Returns(false);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(Guid.NewGuid()));

        Assert.Equal(403, ex.StatusCode);
        _repository.Verify(r => r.Delete(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UsuarioNaoEncontrado_DeveLancarNotFound()
    {
        var id = Guid.NewGuid();
        _currentUser.Setup(c => c.Id).Returns(id);
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var useCase = CreateUseCase();

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(id));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ComUsuarioValido_DeveExcluirESalvar()
    {
        var user = User.Create("Fulano", "fulano@teste.com", "hash");
        _currentUser.Setup(c => c.Id).Returns(user.Id);
        _repository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        await useCase.ExecuteAsync(user.Id);

        _repository.Verify(r => r.Delete(user), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Admin_DevePermitirExcluirQualquerUsuario()
    {
        var user = User.Create("Fulano", "fulano@teste.com", "hash");
        _currentUser.Setup(c => c.Id).Returns(Guid.NewGuid());
        _currentUser.Setup(c => c.IsAdmin).Returns(true);
        _repository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        await useCase.ExecuteAsync(user.Id);

        _repository.Verify(r => r.Delete(user), Times.Once);
    }
}
