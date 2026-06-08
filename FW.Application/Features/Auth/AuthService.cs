using FW.Domain;

namespace FW.Application;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<ApiResponse<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> LogoutAsync(string refreshToken, CancellationToken ct = default);
}

// Implementation

public class AuthService(
    IUserRepository userRepo,
    IRefreshTokenRepository tokenRepo,
    IWalletRepository walletRepo,
    IUnitOfWork uow,
    IJwtService jwt,
    IPasswordService passwords) : IAuthService
{
    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await userRepo.ExistsByEmailAsync(request.Email, ct))
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        var user = User.Create(request.FullName, request.Email, passwords.Hash(request.Password));
        var wallet = Wallet.Create(user.Id);

        await userRepo.AddAsync(user, ct);
        await walletRepo.AddAsync(wallet, ct);
        await uow.SaveChangesAsync(ct);

        return ApiResponse<AuthResponse>.Ok(BuildAuthResponse(user), "Registration successful.");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userRepo.GetByEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive) throw new UnauthorizedException("Account is deactivated.");
        if (!passwords.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        return ApiResponse<AuthResponse>.Ok(BuildAuthResponse(user), "Login successful.");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var stored = await tokenRepo.GetByTokenAsync(request.RefreshToken, ct)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (!stored.IsValid) throw new UnauthorizedException("Refresh token expired or revoked.");

        stored.Revoke();
        var user = stored.User;

        var newRefresh = RefreshToken.Create(user.Id, jwt.GenerateRefreshToken());
        await tokenRepo.AddAsync(newRefresh, ct);
        await uow.SaveChangesAsync(ct);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse(
            jwt.GenerateAccessToken(user),
            newRefresh.Token,
            newRefresh.ExpiresAt,
            ToDto(user)), "Token refreshed.");
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await tokenRepo.GetByTokenAsync(refreshToken, ct);
        if (stored is not null)
        {
            stored.Revoke();
            await uow.SaveChangesAsync(ct);
        }
        return ApiResponse<bool>.Ok(true, "Logged out successfully.");
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var refreshToken = RefreshToken.Create(user.Id, jwt.GenerateRefreshToken());
        user.RefreshTokens.Add(refreshToken);
        return new AuthResponse(
            jwt.GenerateAccessToken(user),
            refreshToken.Token,
            refreshToken.ExpiresAt,
            ToDto(user));
    }

    private static UserDto ToDto(User u) => new(u.Id, u.FullName, u.Email, u.Role);
}
