using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.Buckets;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using Monivise.Domain.Exceptions;
using System.Security.Claims;

namespace Monivise.API.Controllers
{
    [ApiController]
    [Route("api/buckets")]
    [Authorize]
    public class BucketsController : ControllerBase
    {
        private readonly IBucketRepository _buckets;
        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public BucketsController(IBucketRepository buckets) => _buckets = buckets;

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var buckets = await _buckets.GetActiveByUserIdAsync(UserId, ct);
            return Ok(buckets.Select(b => new BucketResponseDto
            {
                Id = b.Id,
                Name = b.Name,
                Icon = b.Icon,
                Color = b.Color,
                Type = b.Type.ToString(),
                AllocationPercent = b.AllocationPercent,
                SavingsTarget = b.SavingsTarget,
                DisplayOrder = b.DisplayOrder,
                IsActive = b.IsActive,
                BucketIcon = b.Icon
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBucketDto dto, CancellationToken ct)
        {
            return UnprocessableEntity(new
            {
                code = "BUCKETS_ARE_SEEDED",
                detail = "Bucket percentages are set by your chosen pathway. Re-run onboarding to change them."
            });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBucketDto dto, CancellationToken ct)
        {
            var bucket = await _buckets.GetByIdAsync(id, ct)
                ?? throw new BucketNotFoundException(id);

            if (bucket.UserId != UserId) return Forbid();

            if (!Enum.TryParse<BucketType>(dto.Type, true, out var bucketType))
                return BadRequest(new ProblemDetails { Title = "Invalid bucket type" });

            bucket.Update(dto.Name!, dto.Icon!, dto.Color!, bucketType,
              bucket.AllocationPercent, dto.SavingsTarget);

            await _buckets.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var bucket = await _buckets.GetByIdAsync(id, ct)
                ?? throw new BucketNotFoundException(id);

            if (bucket.UserId != UserId) return Forbid();

            bucket.Deactivate();
            await _buckets.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
