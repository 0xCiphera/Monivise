using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Exceptions;

namespace Monivise.API.Controllers
{
    [ApiController]
    [Route("api/fixed-obligations")]
    [Authorize]
    public class FixedObligationsController(
        IBudgetCycleRepository cycles,
        IFixedObligationStatusRepository statuses) : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetChecklist(CancellationToken ct)
        {
            var cycle = await cycles.GetActiveByUserIdAsync(UserId, ct)
                ?? throw new CycleNotFoundException(UserId);
            var items = await statuses.GetByCycleIdAsync(cycle.Id, ct);
            return Ok(items.Select(s => new
            {
                s.Id,
                s.IntakeItemId,
                Name = s.Item.Name,
                Reserved = s.Item.MonthlyAmount,
                s.IsPaid,
                s.PaidAmount,
                s.PaidAt
            }));
        }

        [HttpPost("{id:guid}/pay")]
        public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkPaidDto dto, CancellationToken ct)
        {
            var status = await statuses.GetByIdAsync(id, ct)
                ?? throw new ArgumentException("Not found");
            if (status.Cycle.UserId != UserId) return Forbid();

            status.MarkPaid(dto.ActualAmount);
            await statuses.SaveChangesAsync(ct);
            return Ok(new { status.Id, status.IsPaid, status.PaidAmount });
        }

        [HttpPost("{id:guid}/unpay")]
        public async Task<IActionResult> MarkUnpaid(Guid id, CancellationToken ct)
        {
            var status = await statuses.GetByIdAsync(id, ct)
                ?? throw new ArgumentException("Not found");
            if (status.Cycle.UserId != UserId) return Forbid();

            status.MarkUnpaid();
            await statuses.SaveChangesAsync(ct);
            return Ok(new { status.Id, status.IsPaid });
        }
    }

    public class MarkPaidDto
    {
        public decimal ActualAmount { get; set; }
    }
}