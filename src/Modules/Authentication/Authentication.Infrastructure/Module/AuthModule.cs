using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CoeurApi.Modules.Authentication.Application.Abstractions;
using CoeurApi.Modules.Authentication.Application.UseCases.GoogleLogin;
using CoeurApi.Modules.Authentication.Application.Settings;
using CoeurApi.Modules.Authentication.Infrastructure.Security;

namespace CoeurApi.Modules.Authentication.Infrastructure.Module;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<GoogleSettings>(configuration.GetSection("Google"));
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<GoogleLoginUseCase>();
        services.AddValidatorsFromAssemblyContaining<GoogleLoginValidator>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

        return services;
    }
}
