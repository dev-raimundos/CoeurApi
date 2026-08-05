using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.Modules.Users.Application.UseCases.GetAll;
using CoeurApi.SharedKernel.Common;
using Microsoft.AspNetCore.Mvc;

namespace CoeurApi.Modules.Users.Presentation.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class ListUsersController(GetAllUsersUseCase useCase) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Lista Usuários")]
    [EndpointDescription("Retorna uma lista paginada de usuários.")]
    [ProducesResponseType<PagedResult<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<UserResponse>>> GetAll(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);

        var users = await useCase.ExecuteAsync(
                normalizedPage,
                normalizedPageSize,
                cancellationToken
        );

        return Ok(users);
    }
}
