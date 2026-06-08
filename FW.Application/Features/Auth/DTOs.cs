namespace FW.Application;

public record RegisterRequest(string FullName, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserDto User);

public record UserDto(Guid Id, string FullName, string Email, string Role);