using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.Cycles;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Entities;
using Monivise.Domain.Exceptions;
using System.Security.Claims;

namespace Monivise.API.Controllers
{
    [ApiController]
    [Route("api/cycles")]
    [Authorize]
    public class BudgetCyclesController : ControllerBase
    {
        private readonly IBudgetCycleRepository _cycles;
        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public BudgetCyclesController(IBudgetCycleRepository cycles) => _cycles = cycles;

        [HttpPost("start")]
        [ProducesResponseType(typeof(CycleResponseDto), 201)]
        public async Task<IActionResult> StartCycle(CancellationToken ct)
        {
            var existing = await _cycles.GetActiveByUserIdAsync(UserId, ct);
            if (existing is not null) throw new DuplicateCycleException();

            var cycle = BudgetCycle.CreateCurrentMonth(UserId);
            await _cycles.AddAsync(cycle, ct);
            await _cycles.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetActive), new CycleResponseDto
            {
                Id = cycle.Id,
                StartDate = cycle.StartDate,
                EndDate = cycle.EndDate,
                Status = cycle.Status.ToString()
            });
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(CycleResponseDto), 200)]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var cycle = await _cycles.GetActiveByUserIdAsync(UserId, ct);
            if (cycle is null) return NotFound();

            return Ok(new CycleResponseDto
            {
                Id = cycle.Id,
                StartDate = cycle.StartDate,
                EndDate = cycle.EndDate,
                Status = cycle.Status.ToString()
            });
        }
    }
}
