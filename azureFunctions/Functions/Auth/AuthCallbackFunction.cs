using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Net;
using System.Linq;
using TaskManagement.Utils;
using TaskManagement.Repositories.Interfaces;
using TaskManagement.Models;

namespace TaskManagement
{
    public class AuthCallbackFunction
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthCallbackFunction(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        [Function("auth")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "auth/callback")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("AuthCallbackFunction");
            logger.LogInformation("Auth Callback function processed a request.");

            try
            {
                // 1. Usar configuración inyectada
                var config = _configuration;

                // 2. Obtener el código de autorización del query string
                var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                string authorizationCode = query["code"];
                string state = query["state"];
                string error = query["error"];
                string errorDescription = query["error_description"];

                // 3. Manejar errores de OAuth
                if (!string.IsNullOrEmpty(error))
                {
                    logger.LogWarning($"OAuth error: {error} - {errorDescription}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = $"Error de autenticación: {error} - {errorDescription}" }));
                    return errorResponse;
                }

                // 4. Validar que tenemos el código de autorización
                if (string.IsNullOrEmpty(authorizationCode))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Código de autorización no proporcionado" }));
                    return badRequestResponse;
                }

                // 5. Validar configuración de Azure AD (desde la sección Values de local.settings.json)
                var clientId = config["Values:AzureAd__ClientId"];
                var clientSecret = config["Values:AzureAd__ClientSecret"];
                var tenantId = config["Values:AzureAd__TenantId"];

                // Debug logging para diagnosticar configuración
                logger.LogInformation("Debug Config - ClientId: {ClientId}, ClientSecret: {ClientSecret}, TenantId: {TenantId}", 
                    clientId ?? "NULL", 
                    string.IsNullOrEmpty(clientSecret) ? "NULL" : "***CONFIGURED***", 
                    tenantId ?? "NULL");

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(tenantId))
                {
                    logger.LogWarning("Azure AD configuration missing - ClientId: {ClientId}, TenantId: {TenantId}", 
                        string.IsNullOrEmpty(clientId) ? "MISSING" : "CONFIGURED", 
                        string.IsNullOrEmpty(tenantId) ? "MISSING" : "CONFIGURED");
                    var configErrorResponse = req.CreateResponse(HttpStatusCode.ServiceUnavailable);
                    await configErrorResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Configuración de Azure AD no completada. Contacte al administrador." }));
                    return configErrorResponse;
                }

                // 6. Intercambiar código por tokens
                var tokenResponse = await ExchangeCodeForTokensAsync(authorizationCode, config, logger);
                if (tokenResponse == null)
                {
                    var tokenErrorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await tokenErrorResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Error al obtener tokens de acceso" }));
                    return tokenErrorResponse;
                }

                // 6. Validar que tenemos el IdToken
                if (string.IsNullOrEmpty(tokenResponse.IdToken))
                {
                    logger.LogError("IdToken is null or empty in token response");
                    var tokenMissingResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await tokenMissingResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "ID Token no recibido de Azure AD. Verifique los scopes configurados." }));
                    return tokenMissingResponse;
                }

                // 7. Validar y extraer información del token
                var userClaims = ValidateAndExtractClaims(tokenResponse.IdToken, config, logger);
                if (userClaims == null)
                {
                    var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorizedResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Token inválido" }));
                    return unauthorizedResponse;
                }

                // 8. Crear o actualizar usuario en la base de datos
                var user = await CreateOrUpdateUserAsync(userClaims, _userRepository, logger);

                // 9. Generar JWT token para la aplicación
                var appToken = GenerateApplicationToken(user, config);

                // 10. Preparar respuesta de éxito
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

                logger.LogInformation($"Usuario autenticado exitosamente: {user.Email}");
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new 
                { 
                    success = true, 
                    data = authResponse, 
                    message = "Autenticación completada exitosamente" 
                }));
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado en el callback de autenticación");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new { error = "Error interno durante la autenticación" }));
                return errorResponse;
            }
        }

        /// <summary>
        /// Intercambia el código de autorización por tokens de acceso
        /// </summary>
        private static async Task<TokenResponse> ExchangeCodeForTokensAsync(string code, IConfiguration config, ILogger log)
        {
            try
            {
                var clientId = config["Values:AzureAd__ClientId"];
                var clientSecret = config["Values:AzureAd__ClientSecret"];
                var tenantId = config["Values:AzureAd__TenantId"];
                var redirectUri = config["Values:AzureAd__RedirectUri"] ?? "http://localhost:7071/api/auth/callback";

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
                    log.LogInformation($"Token exchange successful. Response: {responseContent}");
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    log.LogInformation($"Parsed TokenResponse - AccessToken: {(string.IsNullOrEmpty(tokenResponse?.AccessToken) ? "NULL" : "PRESENT")}, IdToken: {(string.IsNullOrEmpty(tokenResponse?.IdToken) ? "NULL" : "PRESENT")}");
                    return tokenResponse;
                }
                else
                {
                    log.LogError($"Error obteniendo tokens - Status: {response.StatusCode} - Response: {responseContent}");
                    log.LogError($"Request Details - ClientId: {clientId}, TenantId: {tenantId}, RedirectUri: {redirectUri}");
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
                // Cargar assembly manualmente si hay problemas
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jsonToken = tokenHandler.ReadJwtToken(idToken);

                    // En un entorno de producción, aquí validarías la firma del token
                    // Por ahora, solo extraemos los claims
                    return new ClaimsPrincipal(new ClaimsIdentity(jsonToken.Claims, "jwt"));
                }
                catch (FileNotFoundException ex) when (ex.Message.Contains("System.IdentityModel.Tokens.Jwt"))
                {
                    log.LogError("JWT Assembly not found, trying to load manually");
                    
                    // Fallback: Load assembly manually
                    var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.IdentityModel.Tokens.Jwt.dll");
                    if (File.Exists(assemblyPath))
                    {
                        var assembly = Assembly.LoadFrom(assemblyPath);
                        var handlerType = assembly.GetType("System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler");
                        var handler = Activator.CreateInstance(handlerType);
                        var method = handlerType.GetMethod("ReadJwtToken", new[] { typeof(string) });
                        var token = method.Invoke(handler, new object[] { idToken });
                        
                        var claimsProperty = token.GetType().GetProperty("Claims");
                        var claims = (IEnumerable<Claim>)claimsProperty.GetValue(token);
                        
                        return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
                    }
                    throw;
                }
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
        private static async Task<UserModel> CreateOrUpdateUserAsync(ClaimsPrincipal claims, IUserRepository userRepository, ILogger log)
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
                    Department = "General", // Departamento por defecto cuando no está disponible
                    ProfilePictureUrl = "", // Valor por defecto para evitar NULL constraint
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
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
        
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        
        [JsonPropertyName("token_type")]
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
