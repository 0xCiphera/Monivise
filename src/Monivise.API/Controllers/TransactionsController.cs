using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/transactions")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly IBudgetCycleRepository _cycles;
        private readonly IBucketRepository _buckets;
        private readonly ITransactionRepository _transactions;
        private readonly IFinancialCalculationService _calc;
        private readonly IAuditLogRepository _audit;
        private readonly IIntakeProfileRepository _intakes;
        private readonly IGoalRepository _goals;

        public TransactionsController(IBudgetCycleRepository cycles, IBucketRepository buckets,
            ITransactionRepository transactions, IFinancialCalculationService calc, IAuditLogRepository audit, IIntakeProfileRepository intakes, IGoalRepository goals)
        {
            _cycles = cycles;
            _buckets = buckets;
            _transactions = transactions;
            _calc = calc;
            _audit = audit;
            _intakes = intakes;
            _goals = goals;
        }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("income")]
        [ProducesResponseType(typeof(List<TransactionResponseDto>), 201)]
        public async Task<IActionResult> AddIncome([FromBody] AddIncomeDto dto, CancellationToken ct)
        {
            var cycle = await _cycles.GetActiveByUserIdAsync(UserId, ct)
                ?? throw new CycleNotFoundException(UserId);

            if (dto.IncomeType == "Primary")
                return await HandlePrimaryIncome(cycle, ct);

            return await HandleExtraIncome(dto, cycle, ct);
        }

        private async Task<IActionResult> HandlePrimaryIncome(BudgetCycle cycle, CancellationToken ct)
        {
            var profile = await _intakes.GetByUserIdAsync(UserId, ct)
                ?? throw new ArgumentException("No intake profile — complete onboarding first.");

            decimal amount = profile.BaselineMonthlyIncome;
            if (amount <= 0) return UnprocessableEntity(new { code = "NO_BASELINE_INCOME" });

            var buckets = (await _buckets.GetActiveByUserIdAsync(UserId, ct)).ToList();
            var splits = _calc.AllocateIncome(amount, buckets).ToList();

            var txns = splits.Select(s => Transaction.CreateIncome(
                UserId, s.BucketId, cycle.Id, s.Amount, "Salary", IncomeType.Primary)).ToList();

            await _transactions.AddRangeAsync(txns, ct);
            var logPayload = System.Text.Json.JsonSerializer.Serialize(new { Amount = amount, Source = "Salary" });
            await _audit.AddAsync(AuditLog.Create(UserId, "ADD_INCOME", "Transaction", null, logPayload), ct);
            await _transactions.SaveChangesAsync(ct);
            await _audit.SaveChangesAsync(ct);

            var responseDtos = splits.Join(txns, s => s.BucketId, t => t.BucketId,
                (s, t) => new TransactionResponseDto
                {
                    Id = t.Id,
                    BucketId = t.BucketId,
                    BucketName = s.BucketName,
                    BucketIcon = buckets.First(b => b.Id == t.BucketId).Icon,
                    BucketColor = buckets.First(b => b.Id == t.BucketId).Color,
                    Kind = "Income",
                    Amount = t.Amount,
                    Source = "Salary",
                    IncomeType = "Primary",
                    Date = t.Date
                }).ToList();

            return CreatedAtAction(nameof(AddIncome), responseDtos);
        }

        private async Task<IActionResult> HandleExtraIncome(AddIncomeDto dto, BudgetCycle cycle, CancellationToken ct)
        {
            if (dto.Amount is not > 0)
                return BadRequest(new { code = "AMOUNT_REQUIRED" });
            if (dto.Splits is null || dto.Splits.Count == 0)
                return BadRequest(new { code = "SPLITS_REQUIRED" });

            var validDestinations = new[] { "Buffer", "Wants", "Goal" };
            if (dto.Splits.Any(s => !validDestinations.Contains(s.Destination)))
                return BadRequest(new { code = "INVALID_DESTINATION", detail = "Destination must be Buffer, Wants, or Goal." });

            decimal totalPct = dto.Splits.Sum(s => s.Percent);
            if (Math.Abs(totalPct - 100m) > 0.01m)
                return BadRequest(new { code = "SPLITS_MUST_SUM_TO_100", totalPct });

            Goal? activeGoal = null;
            if (dto.Splits.Any(s => s.Destination == "Goal"))
            {
                activeGoal = await _goals.GetActiveAsync(UserId, ct);
                if (activeGoal is null)
                    return UnprocessableEntity(new { code = "NO_ACTIVE_GOAL", detail = "You selected Goal but have no active goal to contribute to." });
            }

            decimal amount = dto.Amount.Value;
            decimal remainder = amount;
            var applied = new List<(string Destination, decimal Amount)>();

            for (int i = 0; i < dto.Splits.Count; i++)
            {
                bool isLast = i == dto.Splits.Count - 1;
                decimal share = isLast ? remainder : Math.Round(amount * dto.Splits[i].Percent / 100m, 2);
                remainder -= share;
                if (share <= 0) continue;

                switch (dto.Splits[i].Destination)
                {
                    case "Buffer":
                        cycle.AddToBuffer(share);
                        break;
                    case "Wants":
                        cycle.AddToUnpricedPool(share);
                        break;
                    case "Goal":
                        activeGoal!.Contribute(share);
                        break;
                }
                applied.Add((dto.Splits[i].Destination, share));
            }

            await _cycles.SaveChangesAsync(ct);
            if (activeGoal is not null) await _goals.SaveChangesAsync(ct);

            var logPayload = System.Text.Json.JsonSerializer.Serialize(new
            {
                Amount = amount,
                Source = dto.Source ?? "Extra",
                Splits = applied.Select(a => new { a.Destination, a.Amount })
            });
            await _audit.AddAsync(AuditLog.Create(UserId, "ADD_INCOME_EXTRA", "BudgetCycle", cycle.Id, logPayload), ct);
            await _audit.SaveChangesAsync(ct);

            // Buffer/Wants-pool/Goal aren't Buckets, so there's no Bucket-linked Transaction
            // to create here — same pattern SurplusSweepService already uses for goal
            // contributions. This means these entries won't show in Dashboard "Recent
            // activity" (which reads Bucket-linked Transactions only) — known, acceptable
            // gap; Buffer/pool top-ups surface via their own dedicated balances instead.
            return Ok(new
            {
                message = "Extra income applied",
                amount,
                applied = applied.Select(a => new { destination = a.Destination, amount = a.Amount })
            });
        }

        [HttpPost("expense")]
        [ProducesResponseType(typeof(TransactionResponseDto), 201)]
        public async Task<IActionResult> AddExpense([FromBody] AddExpenseDto dto, CancellationToken ct)
        {
            var cycle = await _cycles.GetActiveByUserIdAsync(UserId, ct)
                ?? throw new CycleNotFoundException(UserId);

            var bucket = await _buckets.GetByIdAsync(dto.BucketId, ct)
                ?? throw new BucketNotFoundException(dto.BucketId);

            if (bucket.UserId != UserId) return Forbid();

            var txn = Transaction.CreateExpense(UserId, dto.BucketId, cycle.Id, dto.Amount, dto.Note);

            await _transactions.AddAsync(txn, ct);
            await _transactions.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(AddExpense), new TransactionResponseDto
            {
                Id = txn.Id,
                BucketId = txn.BucketId,
                BucketName = bucket.Name,
                BucketIcon = bucket.Icon,
                BucketColor = bucket.Color,
                Kind = "Expense",
                Amount = txn.Amount,
                Note = txn.Note,
                Date = txn.Date
            });
        }

        [HttpGet("current-cycle")]
        [ProducesResponseType(typeof(List<TransactionResponseDto>), 200)]
        public async Task<IActionResult> GetCurrentCycle(CancellationToken ct)
        {
            var cycle = await _cycles.GetActiveByUserIdAsync(UserId, ct);
            if (cycle is null) return Ok(new List<TransactionResponseDto>());

            var txns = await _transactions.GetByUserAndCycleAsync(UserId, cycle.Id, ct);

            return Ok(txns.Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                BucketId = t.BucketId,
                BucketName = t.Bucket?.Name ?? string.Empty,
                BucketIcon = t.Bucket?.Icon ?? string.Empty,
                BucketColor = t.Bucket?.Color ?? string.Empty,
                Kind = t.Kind.ToString(),
                Amount = t.Amount,
                Note = t.Note,
                Source = t.Source,
                IncomeType = t.IncomeType.ToString(),
                Date = t.Date
            }));
        }
    }
}
