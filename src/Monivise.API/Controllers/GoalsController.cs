using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.Goals;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Domain.Entities;

namespace Monivise.API.Controllers
{
    [Authorize]
    [Route("api/goals")]
    public class GoalsController(IGoalRepository goals) : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            var all = await goals.GetByUserIdAsync(UserId, ct);
            return Ok(all.Select(Map));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGoalDto dto, CancellationToken ct)
        {
            // one active goal: pause any existing active
            var active = await goals.GetActiveAsync(UserId, ct);
            active?.Pause();

            var goal = Goal.Create(UserId, dto.Name, dto.TargetAmount, dto.Icon);
            await goals.AddAsync(goal, ct);
            await goals.SaveChangesAsync(ct);
            return Ok(Map(goal));
        }

        [HttpPost("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
        {
            var target = await goals.GetByIdAsync(id, ct);
            if (target is null || target.UserId != UserId) return NotFound();

            var active = await goals.GetActiveAsync(UserId, ct);
            if (active is not null && active.Id != id) active.Pause();
            target.Resume();
            await goals.SaveChangesAsync(ct);
            return Ok(Map(target));
        }

        private static GoalResponseDto Map(Goal g) => new()
        {
            Id = g.Id,
            Name = g.Name,
            Icon = g.Icon,
            TargetAmount = g.TargetAmount,
            CurrentAmount = g.CurrentAmount,
            ProgressPercent = g.ProgressPercent,
            Status = g.Status.ToString()
        };
    }
}
