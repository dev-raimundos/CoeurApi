namespace CoeurApi.Modules.Authentication.Application.Abstractions;

public record GoogleUserInfo(string Email, string Name, bool EmailVerified);

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
