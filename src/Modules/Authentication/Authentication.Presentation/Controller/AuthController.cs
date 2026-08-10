using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoeurApi.Modules.Authentication.Application.UseCases.Login;

namespace CoeurApi.Modules.Authentication.Presentation.Controller;

[ApiController]
[Route("api/v1/auth")]
[Tags("Autenticação")]
public class AuthController(LoginUseCase login) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]

    [EndpointSummary("Login com email e senha")]
    [EndpointDescription("Valida as credenciais e devolve o token JWT da API.")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await login.ExecuteAsync(request, cancellationToken);
        return Ok(response);
    }
}
