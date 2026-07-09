using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Monivise.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public bool OnboardingComplete { get; private set; } = false;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Refresh token support
        public string? RefreshToken { get; private set; }
        public DateTime? RefreshTokenExpiresAt { get; private set; }

        // Navigation
        public ICollection<BudgetCycle> BudgetCycles { get; private set; } = [];
        public ICollection<Bucket> Buckets { get; private set; } = [];

        protected User() { }

        public static User Create(string email, string passwordHash, string displayName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
            ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

            return new User
            {
                Email = email.ToLowerInvariant().Trim(),
                PasswordHash = passwordHash,
                DisplayName = displayName.Trim()
            };
        }

        public void CompleteOnboarding() => OnboardingComplete = true;
        public void UpdateDisplayName(string name) { DisplayName = name.Trim(); Touch(); }

        public void SetRefreshToken(string token, DateTime expiresAt)
        {
            RefreshToken = token;
            RefreshTokenExpiresAt = expiresAt;
            Touch();
        }

        public void ClearRefreshToken()
        {
            RefreshToken = null;
            RefreshTokenExpiresAt = null;
            Touch();
        }

        private void Touch() => UpdatedAt = DateTime.UtcNow;
    }
}
