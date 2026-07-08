using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.User;
using Monivise.Application.Interfaces.Repositories;

namespace Monivise.API.Controllers;

[Authorize]
[Route("api/user")]
public class UserController(IUserRepository users) : ApiControllerBase
{
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var user = await users.GetByIdAsync(UserId, ct);
        if (user is null) return NotFound();
        return Ok(Map(user));
    }

    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(UserId, ct);
        if (user is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.DisplayName))
            user.UpdateDisplayName(dto.DisplayName);

        if (!string.IsNullOrWhiteSpace(dto.Currency))
            user.UpdateCurrency(dto.Currency);

        await users.SaveChangesAsync(ct);
        return Ok(Map(user));
    }

    private static UserProfileDto Map(Domain.Entities.User u) => new()
    {
        Id = u.Id,
        DisplayName = u.DisplayName,
        Email = u.Email,
        Currency = u.Currency,
        OnboardingComplete = u.OnboardingComplete
    };
}
