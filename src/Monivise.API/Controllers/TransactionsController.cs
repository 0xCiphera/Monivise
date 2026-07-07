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

        public TransactionsController(IBudgetCycleRepository cycles, IBucketRepository buckets,
            ITransactionRepository transactions, IFinancialCalculationService calc, IAuditLogRepository audit)
        {
            _cycles = cycles;
            _buckets = buckets;
            _transactions = transactions;
            _calc = calc;
            _audit = audit;
        }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("income")]
        [ProducesResponseType(typeof(List<TransactionResponseDto>), 201)]
        public async Task<IActionResult> AddIncome([FromBody] AddIncomeDto dto, CancellationToken ct)
        {
            var cycle = await _cycles.GetActiveByUserIdAsync(UserId, ct)
                ?? throw new CycleNotFoundException(UserId);

            var buckets = (await _buckets.GetActiveByUserIdAsync(UserId, ct)).ToList();
            var incomeType = Enum.Parse<Monivise.Domain.Enums.IncomeType>(dto.IncomeType);
            var splits = dto.IncomeType == "Extra"
                ? _calc.AllocateIncome(dto.Amount, buckets.Where(b => b.Type == BucketType.Savings)).ToList()
                : _calc.AllocateIncome(dto.Amount, buckets).ToList();

            var txns = splits.Select(s => Transaction.CreateIncome(
                 UserId, s.BucketId, cycle.Id, s.Amount, dto.Source, incomeType)).ToList();

            await _transactions.AddRangeAsync(txns, ct);

            var logPayload = System.Text.Json.JsonSerializer.Serialize(new { dto.Amount, dto.Source });
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
                    Source = dto.Source,
                    IncomeType = dto.IncomeType,
                    Date = t.Date
                }).ToList();

            return CreatedAtAction(nameof(AddIncome), responseDtos);
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
                Date = t.Date
            }));
        }
    }
}
