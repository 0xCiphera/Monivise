using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.Dashboard;
using Monivise.Application.DTOs.Transactions;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Application.Interfaces.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using Monivise.Domain.Exceptions;
using System.Security.Claims;

namespace Monivise.API.Controllers
{
    [ApiController]
    [Route("api/simulator")]
    [Authorize]
    public class SimulatorController : ControllerBase
    {
        private readonly IBudgetCycleRepository _cycles;
        private readonly IBucketRepository _buckets;
        private readonly ITransactionRepository _transactions;
        private readonly IFinancialCalculationService _calc;

        public SimulatorController(IBudgetCycleRepository cycles, IBucketRepository buckets,
            ITransactionRepository transactions, IFinancialCalculationService calc)
        {
            _cycles = cycles;
            _buckets = buckets;
            _transactions = transactions;
            _calc = calc;
        }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Preview spend consequence before committing.</summary>
        [HttpPost("preview")]
        [ProducesResponseType(typeof(DecisionSimulationResponseDto), 200)]
        public async Task<IActionResult> Preview([FromBody] DecisionSimulationRequestDto dto, CancellationToken ct)
        {
            var cycle = await _cycles.GetActiveByUserIdAsync(UserId, ct)
                ?? throw new CycleNotFoundException(UserId);

            var buckets = (await _buckets.GetActiveByUserIdAsync(UserId, ct)).ToList();
            var txns = (await _transactions.GetByUserAndCycleAsync(UserId, cycle.Id, ct)).ToList();

            var result = _calc.SimulateDecision(dto.BucketId, dto.Amount, buckets, txns, cycle);

            return Ok(new DecisionSimulationResponseDto
            {
                BucketName = result.BucketName,
                Amount = result.Amount,
                BucketBalanceBefore = result.BucketBalanceBefore,
                BucketBalanceAfter = result.BucketBalanceAfter,
                SafeToSpendBefore = result.SafeToSpendBefore,
                SafeToSpendAfter = result.SafeToSpendAfter,
                DailyLimitBefore = result.DailyLimitBefore,
                DailyLimitAfter = result.DailyLimitAfter,
                DepletionPercent = result.DepletionPercent,
                RiskLevel = result.Risk.ToString(),
                RiskMessage = result.Risk switch
                {
                    RiskLevel.Safe => "This spend looks financially healthy.",
                    RiskLevel.Caution => "You will feel this spend by month-end.",
                    RiskLevel.Risky => "This significantly tightens your budget.",
                    RiskLevel.Critical => "This will blow your flexible budget.",
                    _ => string.Empty
                },
                RegretSignals = result.RegretSignals,
                WillOverdraftBucket = result.WillOverdraftBucket,
                // Frontend-sync fields
                CurrentBalance = result.CurrentBalance,
                PostSpendBalance = result.PostSpendBalance,
                PaceScore = result.PaceScore,
                AverageDailySpend = result.AverageDailySpend,
                DaysRemaining = result.DaysRemaining,
                CanAfford = result.CanAfford
            });
        }

        /// <summary>Commit the expense after user confirms.</summary>
        [HttpPost("commit")]
        [ProducesResponseType(typeof(TransactionResponseDto), 201)]
        public async Task<IActionResult> Commit([FromBody] DecisionSimulationRequestDto dto, CancellationToken ct)
        {
            var cycle = await _cycles.GetActiveByUserIdAsync(UserId, ct)
                ?? throw new CycleNotFoundException(UserId);

            var bucket = await _buckets.GetByIdAsync(dto.BucketId, ct)
                ?? throw new BucketNotFoundException(dto.BucketId);

            if (bucket.UserId != UserId) return Forbid();

            var txn = Transaction.CreateExpense(UserId, dto.BucketId, cycle.Id, dto.Amount,
                $"Simulated spend: {dto.Amount:C}");

            await _transactions.AddAsync(txn, ct);
            await _transactions.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(Preview), new TransactionResponseDto
            {
                Id = txn.Id,
                BucketId = txn.BucketId,
                BucketName = bucket.Name,
                BucketIcon = bucket.Icon,
                BucketColor = bucket.Color,
                Kind = "Expense",
                Amount = txn.Amount,
                Note = txn.Note,
                Source = "Simulator",
                IncomeType = string.Empty,
                Date = txn.Date
            });
        }
    }
}
