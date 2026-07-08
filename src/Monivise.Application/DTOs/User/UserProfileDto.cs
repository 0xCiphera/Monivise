namespace Monivise.Application.DTOs.User;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public bool OnboardingComplete { get; set; }
}

public class UpdateProfileDto
{
    public string? DisplayName { get; set; }
    public string? Currency { get; set; }
}
