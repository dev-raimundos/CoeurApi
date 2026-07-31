using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoeurApi.Modules.Authentication.Application.UseCases.GoogleLogin;

namespace CoeurApi.Modules.Authentication.Presentation.Controller;

[ApiController]
[Route("api/v1/auth")]
[Tags("Autenticação via Google")]
public class AuthController(GoogleLoginUseCase googleLogin) : ControllerBase
{
    [HttpPost("google")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]

    [EndpointSummary("Login com Google")]
    [EndpointDescription("Valida o id_token do Google, cria a conta caso ainda não exista e devolve o token JWT da API.")]
    public async Task<ActionResult<AuthResponse>> Google([FromBody] GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var response = await googleLogin.ExecuteAsync(request, cancellationToken);
        return Ok(response);
    }
}
