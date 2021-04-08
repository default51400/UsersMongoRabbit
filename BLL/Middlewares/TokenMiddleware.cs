using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using BLL.Infrastructure.Extensions;
using BLL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Middlewares
{
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<AuthOptions> authOptions;
        private readonly IOptions<UserSecrets> userSecretsFromConfig;
       

        public TokenMiddleware(RequestDelegate next, IOptions<AuthOptions> authOptions, IOptions<UserSecrets> userSecretsFromConfig)
        {
            this._next = next;
            this.authOptions = authOptions;
            this.userSecretsFromConfig = userSecretsFromConfig;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var headerToken = context.Request.Headers["Token"].ToString();

                if (String.IsNullOrWhiteSpace(headerToken))
                {
                    string body = "";

                    using (StreamReader stream = new StreamReader(context.Request.Body))
                    {
                        body = await stream.ReadToEndAsync();
                    }

                    if (body.ToLower().Contains("username") && body.ToLower().Contains("password"))
                    {
                        var userSecrets = JsonSerializer.Deserialize<UserSecrets>(body);
                        var userConfig = GetUserSecretsFromConfig();
                        var secretsIsValid = SecretsIsValid(userSecrets, userConfig);
                        if (!secretsIsValid)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Invalid username or password.");
                        }

                        var identity = GetIdentity(userSecrets, userConfig);
                        if (identity == null)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Invalid username or password.");
                        }

                        var response = new UserSecretsResponse()
                        {
                            Token = GenerateJwt(identity),
                            Username = identity.Name
                        };

                        context.Response.StatusCode = StatusCodes.Status200OK;
                        await context.Response.WriteAsync($"Success auth: {JsonSerializer.Serialize(response)}");
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Please set token from response to headers " +
                            "(KEY: \"Token\", VALUE: \"TokenValue\") " +
                            "\nTo generate a new one, please specify in the body: { \"username\": \"value\", \"password\":\"value\" }");
                    }
                }
                else if (IsValid(headerToken))
                {
                    await _next.Invoke(context);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden; ;
                    await context.Response.WriteAsync("Token is invalid.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Middleware error: {ex}");
            }
        }

        private bool SecretsIsValid(UserSecrets userSecrets, UserSecrets userConfig)
        {
            if (!String.IsNullOrWhiteSpace(userSecrets.Username)
                 && !String.IsNullOrWhiteSpace(userSecrets.Password)
                 && !String.IsNullOrWhiteSpace(userConfig.Username)
                 && !String.IsNullOrWhiteSpace(userConfig.Password))
                return true;
            else return false;
        }

        private bool IsValid(string headerToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = authOptions.Value.GetSymmetricSecurityKey();
            //var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AuthOptions.ke));//AuthOptions.GetSymmetricSecurityKey();

            try
            {
                tokenHandler.ValidateToken(headerToken, new TokenValidationParameters()
                {
                    IssuerSigningKey = key,
                    ValidAudience = authOptions.Value.AUDIENCE,
                    ValidIssuer = authOptions.Value.ISSUER,
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                if (jwtToken.ValidTo > DateTime.UtcNow)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateJwt(ClaimsIdentity identity)
        {
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: authOptions.Value.ISSUER,
                    audience: authOptions.Value.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(authOptions.Value.LIFETIME)),
                    signingCredentials: new SigningCredentials(authOptions.Value.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        private ClaimsIdentity GetIdentity(UserSecrets userSecrets, UserSecrets userConfig)
        {
            if (userSecrets.Username == userConfig.Username && userSecrets.Password == userConfig.Password)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, userSecrets.Username),
                };

                return new ClaimsIdentity(claims, "Token",
                    ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType); ;
            }

            return null;
        }

        private UserSecrets GetUserSecretsFromConfig()
        {
            return new UserSecrets()
            {
                Username = userSecretsFromConfig.Value.Username,
                Password = userSecretsFromConfig.Value.Password
            };
        }
    }
}
