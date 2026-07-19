using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.Review;
using Monivise.Application.Interfaces.Services;

namespace Monivise.API.Controllers
{
    [Authorize]
    [Route("api/review")]
    public class ReviewController(ISurplusSweepService sweep) : ApiControllerBase
    {
        [HttpGet("weekly")]
        public async Task<IActionResult> Weekly(CancellationToken ct)
            => Ok(await sweep.BuildReviewAsync(UserId, ct));

        [HttpPost("sweep")]
        public async Task<IActionResult> Sweep([FromBody] ApplySweepDto dto, CancellationToken ct)
        {
            await sweep.ApplySweepAsync(UserId, dto, ct);
            return Ok(new { message = "Surplus applied" });
        }
    }
}