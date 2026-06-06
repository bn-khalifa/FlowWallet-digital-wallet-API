namespace FW.Domain
{
    public class User
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        public string Role { get; private set; } = "User";
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } 

        // Nav Props
        public Wallet Wallet { get; private set; } = default!;
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

        private User() { }

        // User related methods
        public static User Create(string fullName, string email, string passwordHash)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static User CreateAdmin(string fullName, string email, string passwordHash)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash,
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Deactivate() => IsActive = false;
        public void UpdatePassword(string newHash) => PasswordHash = newHash;
     }
}
