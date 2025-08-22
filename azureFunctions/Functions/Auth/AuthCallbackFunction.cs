using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Utils;
using TaskManagement.Factories;
using TaskManagement.Repositories.Implementations;
using TaskManagement.Models;

namespace TaskManagement
{
    public static class AuthCallbackFunction
    {
        [FunctionName("auth")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "auth/callback")] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("Auth Callback function processed a request.");

            try
            {
                // 1. Configuración
                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                // 2. Obtener el código de autorización del query string
                string authorizationCode = req.Query["code"];
                string state = req.Query["state"];
                string error = req.Query["error"];
                string errorDescription = req.Query["error_description"];

                // 3. Manejar errores de OAuth
                if (!string.IsNullOrEmpty(error))
                {
                    log.LogWarning($"OAuth error: {error} - {errorDescription}");
                    return ResponseBuilder.BadRequest($"Error de autenticación: {error} - {errorDescription}");
                }

                // 4. Validar que tenemos el código de autorización
                if (string.IsNullOrEmpty(authorizationCode))
                {
                    return ResponseBuilder.BadRequest("Código de autorización no proporcionado");
                }

                // 5. Intercambiar código por tokens
                var tokenResponse = await ExchangeCodeForTokensAsync(authorizationCode, config, log);
                if (tokenResponse == null)
                {
                    return ResponseBuilder.BadRequest("Error al obtener tokens de acceso");
                }

                // 6. Validar y extraer información del token
                var userClaims = ValidateAndExtractClaims(tokenResponse.IdToken, config, log);
                if (userClaims == null)
                {
                    return ResponseBuilder.Unauthorized("Token inválido");
                }

                // 7. Crear o actualizar usuario en la base de datos
                var connectionFactory = new SqlConnectionFactory(config);
                var userRepository = new UserRepository(connectionFactory);
                
                var user = await CreateOrUpdateUserAsync(userClaims, userRepository, log);

                // 8. Generar JWT token para la aplicación
                var appToken = GenerateApplicationToken(user, config);

                // 9. Preparar respuesta de éxito
                var authResponse = new AuthCallbackResponse
                {
                    Success = true,
                    Token = appToken,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role
                    },
                    ExpiresIn = 3600, // 1 hora
                    Message = "Autenticación exitosa"
                };

                log.LogInformation($"Usuario autenticado exitosamente: {user.Email}");
                return ResponseBuilder.Success(authResponse, "Autenticación completada exitosamente");

            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error inesperado en el callback de autenticación");
                return ResponseBuilder.InternalServerError("Error interno durante la autenticación");
            }
        }

        /// <summary>
        /// Intercambia el código de autorización por tokens de acceso
        /// </summary>
        private static async Task<TokenResponse> ExchangeCodeForTokensAsync(string code, IConfiguration config, ILogger log)
        {
            try
            {
                var clientId = config["AzureAd__ClientId"];
                var clientSecret = config["AzureAd__ClientSecret"];
                var tenantId = config["AzureAd__TenantId"];
                // var redirectUri = config["AzureAd:RedirectUri"] ?? "http://localhost:7071/api/auth/callback";
                var redirectUri = config["AzureAd__RedirectUri"] ?? "http://localhost:7071/api/auth/callback";

                var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

                using var httpClient = new HttpClient();
                
                var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("scope", "openid profile email")
                });

                var response = await httpClient.PostAsync(tokenEndpoint, tokenRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                }
                else
                {
                    log.LogError($"Error obteniendo tokens: {responseContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error intercambiando código por tokens");
                return null;
            }
        }

        /// <summary>
        /// Valida el token JWT y extrae los claims del usuario
        /// </summary>
        private static ClaimsPrincipal ValidateAndExtractClaims(string idToken, IConfiguration config, ILogger log)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(idToken);

                // En un entorno de producción, aquí validarías la firma del token
                // Por ahora, solo extraemos los claims
                return new ClaimsPrincipal(new ClaimsIdentity(jsonToken.Claims, "jwt"));
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error validando token JWT");
                return null;
            }
        }

        /// <summary>
        /// Crea o actualiza el usuario en la base de datos
        /// </summary>
        private static async Task<UserModel> CreateOrUpdateUserAsync(ClaimsPrincipal claims, UserRepository userRepository, ILogger log)
        {
            var userId = claims.FindFirst("oid")?.Value ?? claims.FindFirst("sub")?.Value;
            var email = claims.FindFirst("email")?.Value ?? claims.FindFirst("preferred_username")?.Value;
            var name = claims.FindFirst("name")?.Value ?? claims.FindFirst("given_name")?.Value + " " + claims.FindFirst("family_name")?.Value;

            // Verificar si el usuario ya existe
            var existingUser = await userRepository.GetByIdAsync(userId);
            
            if (existingUser != null)
            {
                // Actualizar último login
                await userRepository.UpdateLastLoginAsync(userId);
                log.LogInformation($"Usuario existente actualizado: {email}");
                return existingUser;
            }
            else
            {
                // Crear nuevo usuario
                var newUser = new UserModel
                {
                    Id = userId,
                    Name = name?.Trim(),
                    Email = email,
                    Role = "User", // Rol por defecto
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    LastLoginAt = DateTime.UtcNow
                };

                await userRepository.CreateAsync(newUser);
                log.LogInformation($"Nuevo usuario creado: {email}");
                return newUser;
            }
        }

        /// <summary>
        /// Genera un JWT token para usar en la aplicación
        /// </summary>
        private static string GenerateApplicationToken(UserModel user, IConfiguration config)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(config["Jwt:SecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    // DTOs para el intercambio de tokens
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }

    // DTOs para la respuesta del callback
    public class AuthCallbackResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public UserInfo User { get; set; }
        public int ExpiresIn { get; set; }
        public string Message { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
