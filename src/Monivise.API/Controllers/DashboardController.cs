using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monivise.Application.DTOs.Buckets;
using Monivise.Application.DTOs.Cycles;
using Monivise.Application.DTOs.Dashboard;
using Monivise.Application.DTOs.Transactions;
using Monivise.Application.Interfaces.Repositories;
using Monivise.Application.Interfaces.Services;
using Monivise.Domain.Entities;
using Monivise.Domain.Enums;
using System.Security.Claims;

namespace Monivise.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IBudgetCycleRepository _cycles;
        private readonly IBucketRepository _buckets;
        private readonly ITransactionRepository _transactions;
        private readonly IFinancialCalculationService _calc;

        public DashboardController(IBudgetCycleRepository cycles, IBucketRepository buckets,
            ITransactionRepository transactions, IFinancialCalculationService calc)
        {
            _cycles = cycles;
            _buckets = buckets;
            _transactions = transactions;
            _calc = calc;
        }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        [ProducesResponseType(typeof(DashboardDto), 200)]
        public async Task<IActionResult> GetDashboard(CancellationToken ct)
        {
            var cycle = await _cycles.GetActiveByUserIdAsync(UserId, ct);
            if (cycle is null) return Ok(new DashboardDto()); // Pre-onboarding state

            var buckets = (await _buckets.GetActiveByUserIdAsync(UserId, ct)).ToList();
            var txns = (await _transactions.GetByUserAndCycleAsync(UserId, cycle.Id, ct)).ToList();

            var safeToSpend = _calc.GetSafeToSpend(buckets, txns);
            var pace = _calc.GetSpendingPace(buckets, txns, cycle);
            var dailyLimit = _calc.GetDailyLimit(safeToSpend, pace, cycle);

            string paceLabel = pace switch
            {
                <= 0.8m => "On Track",
                <= 1.0m => "Good",
                <= 1.2m => "Caution",
                _ => "Risky"
            };

            var bucketDtos = buckets.Select(b => new BucketResponseDto
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
                Allocated = _calc.GetAllocated(b.Id, txns),
                Spent = _calc.GetSpent(b.Id, txns),
                Balance = _calc.GetBalance(b.Id, txns),
                UsedPercent = _calc.GetAllocated(b.Id, txns) > 0
                    ? Math.Round(_calc.GetSpent(b.Id, txns) / _calc.GetAllocated(b.Id, txns) * 100, 1)
                    : 0m,
            }).ToList();

            var transactionDtos = txns.OrderByDescending(t => t.Date).Take(10).Select(t => new TransactionResponseDto
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
            }).ToList();

            return Ok(new DashboardDto
            {
                SafeToSpend = safeToSpend,
                DailyLimit = dailyLimit,
                SpendingPace = pace,
                PaceLabel = paceLabel,
                DaysRemaining = cycle.RemainingDays,
                DaysElapsed = cycle.ElapsedDays,
                TotalDays = cycle.TotalDays,
                TotalIncome = txns.Where(t => t.Kind == TransactionKind.Income).Sum(t => t.Amount),
                TotalSpent = txns.Where(t => t.Kind == TransactionKind.Expense).Sum(t => t.Amount),
                Buckets = bucketDtos,
                Transactions = transactionDtos,
                CurrentCycle = new CycleResponseDto
                {
                    Id = cycle.Id,
                    StartDate = cycle.StartDate,
                    EndDate = cycle.EndDate,
                    Status = cycle.Status.ToString()
                }
            });
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardDto), 200)]
        public async Task<IActionResult> GetSummary(CancellationToken ct)
        {
            // Alias for frontend compatibility
            return await GetDashboard(ct);
        }
    }
}
