using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.Modules.Users.Application.UseCases.Create;
using CoeurApi.Modules.Users.Application.UseCases.Delete;
using CoeurApi.Modules.Users.Application.UseCases.GetAll;
using CoeurApi.Modules.Users.Application.UseCases.GetById;
using CoeurApi.Modules.Users.Application.UseCases.Update;
using CoeurApi.SharedKernel.Common;
using Microsoft.AspNetCore.Routing;

namespace CoeurApi.Modules.Users.Presentation;

[ApiController]
[Route("api/v1/users")]
public class UsersController(
    CreateUserUseCase createUser,
    GetAllUsersUseCase getAllUsers,
    GetUserByIdUseCase getUserById,
    UpdateUserUseCase updateUser,
    DeleteUserUseCase deleteUser) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Lista Usuários")]
    [EndpointDescription("Retorna uma lista paginada de usuários.")]
    [ProducesResponseType<PagedResult<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<UserResponse>>> GetAll(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var users = await getAllUsers.ExecuteAsync(
            normalizedPage,
            normalizedPageSize,
            cancellationToken
        );
        return Ok(users);
    }

    [HttpPost]
    [EndpointSummary("Cria um Usuário")]
    [EndpointDescription("Criação de um usuário dado email, nome e senha.")]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await createUser.ExecuteAsync(request, cancellationToken);
        return Created($"api/v1/users/{user.Id}", user);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Encontra um Usuário")]
    [EndpointDescription("Retorna um usuário identificado pelo ID")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await getUserById.ExecuteAsync(id, cancellationToken);
        return Ok(user);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Edita um Usuário")]
    [EndpointDescription("Atualiza um usuário identificado pelo ID")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await updateUser.ExecuteAsync(id, request, cancellationToken);
        return Ok(user);
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Deleta um Usuário")]
    [EndpointDescription("Exclui um usuário identificado pelo ID")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteUser.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
