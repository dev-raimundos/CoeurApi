using CoeurApi.Modules.Users.Application.UseCases;

namespace CoeurApi.Modules.Authentication.Application.UseCases.GoogleLogin;

public record AuthResponse(
    UserResponse User,
    string Token
);
