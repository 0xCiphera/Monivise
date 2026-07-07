using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.Income;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Application.Interfaces.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Exceptions;
using System.Security.Claims;

namespace Monivise.API.Controllers
{
    [ApiController]
    [Route("api/income")]
    [Authorize]
    public class IncomeController : ControllerBase
    {
        private readonly IBudgetCycleRepository _cycles;
        private readonly IBucketRepository _buckets;
        private readonly ITransactionRepository _transactions;
        private readonly IFinancialCalculationService _calc;

        public IncomeController(IBudgetCycleRepository cycles, IBucketRepository buckets,
            ITransactionRepository transactions, IFinancialCalculationService calc)
        {
            _cycles = cycles;
            _buckets = buckets;
            _transactions = transactions;
            _calc = calc;
        }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Allocate income across all active buckets proportionally.</summary>
        [HttpPost("allocate")]
        [ProducesResponseType(typeof(List<AllocationResultDto>), 201)]
        public async Task<IActionResult> Allocate([FromBody] AllocateIncomeRequestDto dto, CancellationToken ct)
        {
            var cycle = await _cycles.GetActiveByUserIdAsync(UserId, ct)
                ?? throw new CycleNotFoundException(UserId);

            var buckets = (await _buckets.GetActiveByUserIdAsync(UserId, ct)).ToList();
            var splits = _calc.AllocateIncome(dto.Amount, buckets).ToList();

            var txns = splits.Select(s => Transaction.CreateIncome(
                UserId, s.BucketId, cycle.Id, s.Amount, dto.Source)).ToList();

            await _transactions.AddRangeAsync(txns, ct);
            await _transactions.SaveChangesAsync(ct);

            var result = splits.Select(s => new AllocationResultDto
            {
                BucketId = s.BucketId,
                BucketName = s.BucketName,
                Amount = s.Amount
            }).ToList();

            return CreatedAtAction(nameof(Allocate), result);
        }
    }
}
