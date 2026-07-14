using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.Buckets;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Application.Interfaces.Services;
using Monivise.Domain.Exceptions;

namespace Monivise.API.Controllers;

[ApiController]
[Route("api/buckets")]
[Authorize]
public class BucketsController(
    IBucketRepository buckets,
    ITransactionRepository transactions,
    IBudgetCycleRepository cycles,
    IFinancialCalculationService calc) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var bucketList = (await buckets.GetActiveByUserIdAsync(UserId, ct)).ToList();

        // Load current-cycle transactions so financial fields are populated
        var cycle = await cycles.GetActiveByUserIdAsync(UserId, ct);
        var txns = cycle is not null
            ? (await transactions.GetByUserAndCycleAsync(UserId, cycle.Id, ct)).ToList()
            : [];

        return Ok(bucketList.Select(b => new BucketResponseDto
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
            Allocated = calc.GetAllocated(b.Id, txns),
            Spent = calc.GetSpent(b.Id, txns),
            Balance = calc.GetBalance(b.Id, txns),
            UsedPercent = calc.GetAllocated(b.Id, txns) > 0
                ? Math.Round(calc.GetSpent(b.Id, txns) / calc.GetAllocated(b.Id, txns) * 100, 1)
                : 0m,
        }));
    }

    [HttpPost]
    public IActionResult Create() =>
        UnprocessableEntity(new
        {
            code = "BUCKETS_ARE_SEEDED",
            detail = "Bucket percentages are set by your chosen pathway. Re-run onboarding to change them."
        });

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBucketDto dto, CancellationToken ct)
    {
        var bucket = await buckets.GetByIdAsync(id, ct)
            ?? throw new BucketNotFoundException(id);

        if (bucket.UserId != UserId) return Forbid();

        bucket.Update(
            dto.Name ?? bucket.Name,
            dto.Icon ?? bucket.Icon,
            dto.Color ?? bucket.Color,
            bucket.Type, bucket.AllocationPercent, bucket.SavingsTarget);

        await buckets.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var bucket = await buckets.GetByIdAsync(id, ct)
            ?? throw new BucketNotFoundException(id);

        if (bucket.UserId != UserId) return Forbid();

        bucket.Deactivate();
        await buckets.SaveChangesAsync(ct);
        return NoContent();
    }
}
