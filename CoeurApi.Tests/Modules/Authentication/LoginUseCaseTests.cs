using CoeurApi.Modules.Authentication.Application.Settings;
using CoeurApi.Modules.Authentication.Application.UseCases.Login;
using CoeurApi.Modules.Authentication.Infrastructure.Security;
using CoeurApi.SharedKernel.Abstractions;
using CoeurApi.SharedKernel.Exceptions;
using Microsoft.Extensions.Options;
using Moq;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.Modules.Users.Domain;

namespace CoeurApi.Tests.Modules.Authentication;

public class LoginUseCaseTests
{
    private readonly Mock<IUsersRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly TokenService _tokenService = new(Options.Create(new JwtSettings
    {
        Secret = "chave-de-teste-com-pelo-menos-32-caracteres",
        Issuer = "coeur-api-tests",
        Audience = "coeur-api-tests",
        ExpirationHours = 1
    }));

    private LoginUseCase CreateUseCase()
        => new(_repository.Object, _tokenService, _unitOfWork.Object);

    private static User CreateActiveUser(string email = "fulano@teste.com", string password = "senha-correta")
        => User.Create("Fulano", email, BCrypt.Net.BCrypt.HashPassword(password));

    [Fact]
    public async Task ExecuteAsync_ComEmailInexistente_DeveLancarUnauthorized()
    {
        _repository.Setup(r => r.GetByEmailAsync("inexistente@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var useCase = CreateUseCase();
        var request = new LoginRequest("inexistente@teste.com", "qualquer-senha");

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(request));

        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ComSenhaIncorreta_DeveLancarUnauthorizedERegistrarFalha()
    {
        var user = CreateActiveUser();
        _repository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        var request = new LoginRequest(user.Email, "senha-errada");

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(request));

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal(1, user.FailedLoginAttempts);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ComContaDesativada_DeveLancarForbidden()
    {
        var user = CreateActiveUser();
        typeof(User).GetProperty(nameof(User.IsActive))!.SetValue(user, false);
        _repository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(new LoginRequest(user.Email, "senha-correta")));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ComContaBloqueada_DeveLancarForbidden()
    {
        var user = CreateActiveUser();
        typeof(User).GetProperty(nameof(User.LockedUntil))!.SetValue(user, DateTime.UtcNow.AddMinutes(10));
        _repository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(new LoginRequest(user.Email, "senha-correta")));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ComCredenciaisValidas_DeveRetornarToken()
    {
        var user = CreateActiveUser();
        _repository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        var response = await useCase.ExecuteAsync(new LoginRequest(user.Email, "senha-correta"));

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(user.Email, response.User.Email);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
