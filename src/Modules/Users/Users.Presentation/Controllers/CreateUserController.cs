using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.Modules.Users.Application.UseCases.Create;
using Microsoft.AspNetCore.Mvc;

namespace CoeurApi.Modules.Users.Presentation.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class CreateUserController(CreateUserUseCase service) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Cria um Usuário")]
    [EndpointDescription("Criação de um usuário dado email, nome e senha.")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var users = await service.ExecuteAsync(request, cancellationToken);
        return Created($"api/v1/users/{users.Id}", users);
    }
}
