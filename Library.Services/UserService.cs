using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Library.Common;
using Library.Common.DTOs;
using Library.Common.Entities;
using Library.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Library.Services
{
    public class UserService : IUserService
    {
        private List<User> _users = new List<User>();
        private Dictionary<string, RefreshTokenEntry> _refreshTokens = new Dictionary<string, RefreshTokenEntry>();

        private readonly AuthorizationSettings _authorizationSettings;

        private readonly byte[] _salt;

        public UserService(IOptions<AuthorizationSettings> appSettings)
        {
            _authorizationSettings = appSettings.Value;

            _salt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(_salt);
            }

            addTestUsers();
        }
        public AuthenticateResponseDto? Authenticate(string username, string password)
        {
            var user = _users.SingleOrDefault(x => x.Username == username && x.PasswordHash == HashPassword(password));

            if (user == null) 
                return null;

            return new AuthenticateResponseDto
            {
                IdToken = GenerateIdToken(user),
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken(user)
            };
        }

        public AuthenticateResponseDto? RefreshTokens(string refreshToken)
        {
            if (!_refreshTokens.TryGetValue(refreshToken, out var tokenEntry))
                return null;

            if (tokenEntry.Expiry < DateTime.UtcNow)
            {
                _refreshTokens.Remove(refreshToken);
                return null;
            }

            var user = _users.SingleOrDefault(x => x.Id == tokenEntry.UserId);
            if (user == null)
                return null;

            _refreshTokens.Remove(refreshToken);

            return new AuthenticateResponseDto
            {
                IdToken = GenerateIdToken(user),
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken(user)
            };
        }

        public AuthenticateResponseDto? RenewTokens(string userEmail)
        {
            var user = _users.SingleOrDefault(x => x.Email == userEmail);

            if (user == null)
                return null;

            return new AuthenticateResponseDto
            {
                IdToken = GenerateIdToken(user),
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken(user)
            };
        }

        public bool RevokeToken(string refreshToken)
        {            
            var tokenExists = _refreshTokens.ContainsKey(refreshToken);

            if (!tokenExists)
                return false;

            _refreshTokens.Remove(refreshToken);

            return true;
        }

        public User? GetByEmail(string email)
        {
            return _users.SingleOrDefault(x => x.Email == email);
        }

        public IEnumerable<User> GetAll()
        {
            return _users;
        }

        public User? GetById(Guid id)
        {
            return _users.SingleOrDefault(x => x.Id == id);
        }

        public IEnumerable<RefreshTokenEntry> GetRefreshTokensByUserId(Guid userId)
        {
            return _refreshTokens.Values
                    .Where(t => t.UserId == userId)
                    .ToList();
        }

        private string GenerateIdToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authorizationSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
                new Claim("tokenType", "id_token")
            };

            var token = new JwtSecurityToken(
                issuer: _authorizationSettings.Issuer,
                audience: _authorizationSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_authorizationSettings.IdTokenLifetimeInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateAccessToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authorizationSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("role", user.UserRole),
                new Claim("tokenType", "access_token")
            };

            var token = new JwtSecurityToken(
                issuer: _authorizationSettings.Issuer,
                audience: _authorizationSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_authorizationSettings.AccessTokenLifetimeInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken(User user)
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var entry = new RefreshTokenEntry
            {
                Token = refreshToken,
                UserId = user.Id,
                Expiry = DateTime.UtcNow.AddMinutes(_authorizationSettings.RefreshTokenLifetimeInMinutes)
            };

            _refreshTokens[refreshToken] = entry;

            CleanupExpiredRefreshTokens();

            return refreshToken;
        }

        private void CleanupExpiredRefreshTokens() 
        { 
            var expiredTokens = _refreshTokens
                .Where(x => x.Value.Expiry < DateTime.UtcNow)
                .Select(x => x.Key)
                .ToList();

            foreach (var expiredToken in expiredTokens)
            {
                _refreshTokens.Remove(expiredToken);
            }
        }

        private void addTestUsers()
        {
            _users.Add(new User { Id = Guid.NewGuid(), FirstName = "User1", LastName = "User2", Username = "test-user", PasswordHash = HashPassword("user"), UserRole = "User", Email = "test-user" });
            _users.Add(new User { Id = Guid.NewGuid(), FirstName = "User1", LastName = "User2", Username = "test-admin", PasswordHash = HashPassword("admin"), UserRole = "Admin", Email = "test-admin" });
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: _salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}
