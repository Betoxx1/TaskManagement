using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace TaskManagement.Utils
{
    public class JwtValidator
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtValidator(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:SecretKey"] ?? throw new ArgumentException("JWT SecretKey not configured");
            _issuer = _configuration["Jwt:Issuer"] ?? throw new ArgumentException("JWT Issuer not configured");
            _audience = _configuration["Jwt:Audience"] ?? throw new ArgumentException("JWT Audience not configured");
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetUserIdFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal == null)
                    return null;

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) 
                    ?? principal.FindFirst("sub") 
                    ?? principal.FindFirst("userid");

                return userIdClaim?.Value;
            }
            catch
            {
                return null;
            }
        }

        public string GetEmailFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal == null)
                    return null;

                var emailClaim = principal.FindFirst(ClaimTypes.Email) 
                    ?? principal.FindFirst("email");

                return emailClaim?.Value;
            }
            catch
            {
                return null;
            }
        }

        public bool IsTokenValid(string token)
        {
            return ValidateToken(token) != null;
        }

        public DateTime? GetTokenExpiration(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                return jsonToken.ValidTo;
            }
            catch
            {
                return null;
            }
        }
    }
} 