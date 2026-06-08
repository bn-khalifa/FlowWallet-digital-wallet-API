using FW.Domain;

namespace FW.Application;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    string Role { get; }
}