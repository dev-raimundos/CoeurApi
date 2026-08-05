using CoeurApi.Application.Abstractions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Application.UseCases.Update;
using CoeurApi.Modules.Users.Domain;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;
using Moq;

namespace CoeurApi.Tests.Modules.Users;

public class UpdateUserUseCaseTests
{
    private readonly Mock<IUsersRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private UpdateUserUseCase CreateUseCase() => new(_repository.Object, _unitOfWork.Object, _currentUser.Object);

    [Fact]
    public async Task ExecuteAsync_UsuarioTentandoEditarOutroPerfil_DeveLancarForbidden()
    {
        _currentUser.Setup(c => c.Id).Returns(Guid.NewGuid());
        _currentUser.Setup(c => c.IsAdmin).Returns(false);

        var useCase = CreateUseCase();
        var request = new UpdateUserRequest("Novo Nome");

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(Guid.NewGuid(), request));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_UsuarioNaoEncontrado_DeveLancarNotFound()
    {
        var id = Guid.NewGuid();
        _currentUser.Setup(c => c.Id).Returns(id);
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var useCase = CreateUseCase();
        var request = new UpdateUserRequest("Novo Nome");

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(id, request));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ComDadosValidos_DeveAtualizarPerfilESalvar()
    {
        var user = User.Create("Fulano", "fulano@teste.com", "hash");
        _currentUser.Setup(c => c.Id).Returns(user.Id);
        _repository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        var result = await useCase.ExecuteAsync(user.Id, new UpdateUserRequest("Novo Nome"));

        Assert.Equal("Novo Nome", result.Name);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Admin_DevePermitirEditarQualquerUsuario()
    {
        var user = User.Create("Fulano", "fulano@teste.com", "hash");
        _currentUser.Setup(c => c.Id).Returns(Guid.NewGuid());
        _currentUser.Setup(c => c.IsAdmin).Returns(true);
        _repository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        var result = await useCase.ExecuteAsync(user.Id, new UpdateUserRequest("Novo Nome"));

        Assert.Equal("Novo Nome", result.Name);
    }
}
