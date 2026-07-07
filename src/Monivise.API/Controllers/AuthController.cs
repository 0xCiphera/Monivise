using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.Auth;
using Monivise.Application.Interfaces.Repositories;
using DomainUser = Monivise.Domain.Entities.User;
using Monivise.Infrastructure.Auth;

namespace Monivise.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly JwtTokenService _jwt;
     
        public AuthController(IUserRepository users, JwtTokenService jwt)
        {
            _users = users;
            _jwt = jwt;
        }

        
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), 201)]
        [ProducesResponseType(typeof(ProblemDetails), 409)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            if (await _users.ExistsByEmailAsync(dto.Email, ct))
                return Conflict(new ProblemDetails
                {
                    Title = "Email already registered",
                    Detail = "An account with this email address already exists.",
                    Status = 409
                });

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new ProblemDetails { Title = "Password is required", Status = 400 });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = DomainUser.Create(dto.Email, passwordHash, dto.DisplayName, dto.Currency);

            // Generate refresh token
            var refreshToken = _jwt.GenerateRefreshToken();
            user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

            await _users.AddAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            var (accessToken, expiresAt) = _jwt.GenerateAccessToken(user);

            
            SetRefreshTokenCookie(refreshToken);

            return CreatedAtAction(null, null, new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken, // Also send in body for initial storage
                ExpiresAt = expiresAt,
                UserId = user.Id,
                DisplayName = user.DisplayName,
                Currency = user.Currency
            });
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            var user = await _users.GetByEmailAsync(dto.Email, ct);
            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new ProblemDetails
                {
                    Title = "Invalid credentials",
                    Detail = "Email or password is incorrect.",
                    Status = 401
                });

            // Generate new refresh token
            var refreshToken = _jwt.GenerateRefreshToken();
            user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
            await _users.SaveChangesAsync(ct);

            var (accessToken, expiresAt) = _jwt.GenerateAccessToken(user);

            SetRefreshTokenCookie(refreshToken);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                DisplayName = user.DisplayName,
                Currency = user.Currency
            });
        }

        
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new ProblemDetails { Title = "No refresh token", Status = 401 });

            var user = await _users.GetByRefreshTokenAsync(refreshToken, ct);
            if (user is null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
                return Unauthorized(new ProblemDetails { Title = "Invalid refresh token", Status = 401 });

            // Rotate refresh token
            var newRefreshToken = _jwt.GenerateRefreshToken();
            user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
            await _users.SaveChangesAsync(ct);

            var (accessToken, expiresAt) = _jwt.GenerateAccessToken(user);

            SetRefreshTokenCookie(newRefreshToken);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                DisplayName = user.DisplayName,
                Currency = user.Currency
            });
        }

      
        [HttpPost("logout")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var userId = User.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _users.GetByIdAsync(Guid.Parse(userId), ct);
                if (user != null)
                {
                    user.ClearRefreshToken();
                    await _users.SaveChangesAsync(ct);
                }
            }

            Response.Cookies.Delete("refreshToken");
            return Ok(new { message = "Logged out successfully" });
        }

        private void SetRefreshTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
    }
}
