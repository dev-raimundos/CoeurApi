using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.Modules.Users.Application.UseCases.GetById;
using Microsoft.AspNetCore.Mvc;

namespace CoeurApi.Modules.Users.Presentation.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class GetUserByIdController(GetUserByIdUseCase useCase) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [EndpointSummary("Encontra um Usuário")]
    [EndpointDescription("Retorna um usuário identificado pelo ID")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await useCase.ExecuteAsync(id, cancellationToken);
        return Ok(user);
    }
}
