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
        private readonly IWantCategoryRepository _wantCategories;

        public SimulatorController(IBudgetCycleRepository cycles, IBucketRepository buckets,
            ITransactionRepository transactions, IFinancialCalculationService calc, IWantCategoryRepository wantCategories)
        {
            _cycles = cycles;
            _buckets = buckets;
            _transactions = transactions;
            _calc = calc;
            _wantCategories = wantCategories;
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

            WantCategory? wantCategory = dto.WantCategoryId is not null
                    ? await _wantCategories.GetByIdAsync(dto.WantCategoryId.Value, ct)
                    : null;
            var result = _calc.SimulateDecision(dto.BucketId, dto.IntakeItemId, wantCategory, dto.Amount, buckets, txns, cycle);

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
                CanAfford = result.CanAfford,
                WillDrawFromBuffer = result.WillDrawFromBuffer,
                BufferDrawAmount = result.BufferDrawAmount,
                BufferBalanceAfter = result.BufferBalanceAfter,
                WillDrawFromPool = result.WillDrawFromPool,
                PoolBalanceAfter = result.PoolBalanceAfter,
                PoolDrawAmount = result.PoolDrawAmount
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

            WantCategory? wantCategory = dto.WantCategoryId is not null
                    ? await _wantCategories.GetByIdAsync(dto.WantCategoryId.Value, ct)
                    : null;

            var buckets = (await _buckets.GetActiveByUserIdAsync(UserId, ct)).ToList();
            var txns = (await _transactions.GetByUserAndCycleAsync(UserId, cycle.Id, ct)).ToList();
            var preCheck = _calc.SimulateDecision(dto.BucketId, dto.IntakeItemId, wantCategory, dto.Amount, buckets, txns, cycle);

            if (!preCheck.CanAfford)
                return UnprocessableEntity(new { code = "CANNOT_AFFORD", detail = "Even the buffer can't cover this." });

            if (preCheck.WillDrawFromPool)
                cycle.DrawFromUnpricedPool(preCheck.PoolDrawAmount);

            if (preCheck.WillDrawFromBuffer)
                cycle.DrawFromBuffer(preCheck.BufferDrawAmount);

            var txn = Transaction.CreateExpense(UserId, dto.BucketId, cycle.Id, dto.Amount,
                $"Simulated spend: {dto.Amount:C}", dto.IntakeItemId, dto.WantCategoryId);

            await _transactions.AddAsync(txn, ct);
            await _transactions.SaveChangesAsync(ct);
            await _cycles.SaveChangesAsync(ct); // persists the buffer draw, if any

            return CreatedAtAction(nameof(Preview), new TransactionResponseDto { /* unchanged */ });
        }
    }
}
