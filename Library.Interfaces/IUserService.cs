using Library.Common.Entities;
using Library.Common.DTOs;

namespace Library.Interfaces;

public interface IUserService
{
    AuthenticateResponseDto? Authenticate(string username, string password);
    AuthenticateResponseDto? RefreshTokens(string refreshToken);
    AuthenticateResponseDto? RenewTokens(string userEmail);
    bool RevokeToken(string refreshToken);
    User? GetByEmail(string email);

    IEnumerable<User> GetAll();
    User? GetById(Guid id);
    IEnumerable<RefreshTokenEntry> GetRefreshTokensByUserId(Guid userId);
}
