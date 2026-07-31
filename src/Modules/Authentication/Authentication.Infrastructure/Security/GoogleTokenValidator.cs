using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using CoeurApi.Modules.Authentication.Application.Abstractions;
using CoeurApi.Modules.Authentication.Application.Settings;

namespace CoeurApi.Modules.Authentication.Infrastructure.Security;

public class GoogleTokenValidator(IOptions<GoogleSettings> options) : IGoogleTokenValidator
{
    private readonly GoogleSettings _settings = options.Value;

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_settings.ClientId]
            });

            return new GoogleUserInfo(payload.Email, payload.Name, payload.EmailVerified);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
