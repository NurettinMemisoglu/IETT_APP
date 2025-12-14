using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IETT_APP.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user, IList<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // 1. Key'i al (UTF8 daha güvenlidir)
            // Eğer appsettings boşsa patlamasın diye varsayılan bir key koyduk.
            var keyString = _config["Jwt:Key"] ?? "SuperSecretKey1234567890_SetInAppSettings";
            var key = Encoding.UTF8.GetBytes(keyString);

            // 2. Claims (Kimlik Bilgileri)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            // Rolleri ekle
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 3. Token Ayarları (KRİTİK DÜZELTME BURADA)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token ömrü (Örn: 1 saat)

                // Program.cs Validasyonuna Uyumlu Olması İçin Şunları Ekledik:
                Issuer = _config["Jwt:Issuer"] ?? "IETT_API",
                Audience = _config["Jwt:Audience"] ?? "IETT_Client",

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Daha güvenli (Kriptografik) Refresh Token Üretimi
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}