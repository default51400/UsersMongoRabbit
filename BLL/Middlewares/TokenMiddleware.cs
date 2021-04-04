using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BLL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Middlewares
{
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration configuration;

        public TokenMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this._next = next;
            this.configuration = configuration;
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
                        var identity = GetIdentity(userSecrets.Username, userSecrets.Password);
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

        private bool IsValid(string headerToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = AuthOptions.GetSymmetricSecurityKey();
            try
            {
                tokenHandler.ValidateToken(headerToken, new TokenValidationParameters()
                {
                    IssuerSigningKey = key,
                    ValidAudience = AuthOptions.AUDIENCE,
                    ValidIssuer = AuthOptions.ISSUER,
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

        private static string GenerateJwt(ClaimsIdentity identity)
        {
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        private ClaimsIdentity GetIdentity(string username, string password)
        {
            UserSecrets user = new UserSecrets()
            {
                Username = configuration.GetSection("UserSecrets:Username").Value,
                Password = configuration.GetSection("UserSecrets:Password").Value
            };

            if (user.Username == username && user.Password == password
                && !String.IsNullOrWhiteSpace(user.Username)
                && !String.IsNullOrWhiteSpace(user.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username),
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            return null;
        }
    }
}
