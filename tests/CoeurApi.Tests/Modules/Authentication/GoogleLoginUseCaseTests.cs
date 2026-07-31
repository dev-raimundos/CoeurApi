using CoeurApi.Modules.Authentication.Application.Abstractions;
using CoeurApi.Modules.Authentication.Infrastructure.Security;
using CoeurApi.Modules.Authentication.Application.Settings;
using CoeurApi.Modules.Authentication.Application.UseCases.GoogleLogin;
using CoeurApi.SharedKernel.Exceptions;
using CoeurApi.Modules.Users.Application.Abstractions;
using CoeurApi.SharedKernel.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using CoeurApi.Modules.Users.Domain;

namespace CoeurApi.Tests.Modules.Authentication;

public class GoogleLoginUseCaseTests
{
    private readonly Mock<IUsersRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IGoogleTokenValidator> _googleTokenValidator = new();

    private readonly TokenService _tokenService = new(Options.Create(new JwtSettings
    {
        Secret = "chave-de-teste-com-pelo-menos-32-caracteres",
        Issuer = "coeur-api-tests",
        Audience = "coeur-api-tests",
        ExpirationHours = 1
    }));

    private GoogleLoginUseCase CreateUseCase()
        => new(_googleTokenValidator.Object, _repository.Object, _tokenService, _unitOfWork.Object);

    private static User CreateActiveUser(string email = "fulano@teste.com")
        => User.CreateFromGoogle("Fulano", email);

    [Fact]
    public async Task ExecuteAsync_ComIdTokenInvalido_DeveLancarUnauthorized()
    {
        _googleTokenValidator.Setup(v => v.ValidateAsync("token-invalido", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GoogleUserInfo?)null);

        var useCase = CreateUseCase();
        var request = new GoogleLoginRequest("token-invalido");

        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(request));

        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ComEmailJaExistente_DeveLogarSemCriarNovaConta()
    {
        var user = CreateActiveUser();
        _googleTokenValidator.Setup(v => v.ValidateAsync("token-valido", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo(user.Email, user.Name, true));
        _repository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        var response = await useCase.ExecuteAsync(new GoogleLoginRequest("token-valido"));

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(user.Email, response.User.Email);
        _repository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ComEmailInexistente_DeveCriarContaERetornarToken()
    {
        _googleTokenValidator.Setup(v => v.ValidateAsync("token-valido", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo("novo@teste.com", "Novo Usuário", true));
        _repository.Setup(r => r.GetByEmailAsync("novo@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var useCase = CreateUseCase();
        var response = await useCase.ExecuteAsync(new GoogleLoginRequest("token-valido"));

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal("novo@teste.com", response.User.Email);
        _repository.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "novo@teste.com"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ComContaDesativada_DeveLancarForbidden()
    {
        var user = CreateActiveUser();
        typeof(User).GetProperty(nameof(User.IsActive))!.SetValue(user, false);

        _googleTokenValidator.Setup(v => v.ValidateAsync("token-valido", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo(user.Email, user.Name, true));
        _repository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        var ex = await Assert.ThrowsAsync<HttpException>(() => useCase.ExecuteAsync(new GoogleLoginRequest("token-valido")));

        Assert.Equal(403, ex.StatusCode);
    }
}
