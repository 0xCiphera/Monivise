namespace Monivise.App.DTOs;

// ── Auth ──────────────────────────────────────────────────────────────────────
public record LoginRequest(string Email, string Password);
public record RegisterRequest(string DisplayName, string Email, string Password);
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    string DisplayName);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);

// ── Buckets ───────────────────────────────────────────────────────────────────
// Read-only — buckets are pathway-seeded at onboarding/rollover, no create/update/delete surface.
public record BucketDto(
    Guid Id,
    string Name,
    string Icon,
    string Color,
    string Type,          // Fixed|Flexible|Investment|Wants
    decimal AllocationPercent,
    bool IsActive,
    int DisplayOrder,
    decimal Allocated,
    decimal Spent,
    decimal Balance);

// ── Transactions ──────────────────────────────────────────────────────────────
public record TransactionDto(
    Guid Id,
    string Kind,
    Guid? BucketId,
    string? BucketName,
    string? BucketIcon,
    string? BucketColor,
    decimal Amount,
    DateTimeOffset Date,
    string? Note);

public record IncomeAllocationSplit(string Destination, decimal Percent); // Buffer|Wants|Goal
public record AddIncomeRequest(string IncomeType, decimal? Amount, string? Source, List<IncomeAllocationSplit>? Splits);
public record RecordExpenseRequest(Guid BucketId, decimal Amount, string? Note);
public record RecordInvestmentRequest(Guid IntakeItemId, decimal Amount);

// ── Simulator ─────────────────────────────────────────────────────────────────
public record SimulateRequest(Guid BucketId, Guid? IntakeItemId, Guid? WantCategoryId, decimal Amount);
public record SimulateResponse(
    string BucketName,
    decimal Amount,
    decimal BucketBalanceBefore,
    decimal BucketBalanceAfter,
    decimal SafeToSpendBefore,
    decimal SafeToSpendAfter,
    decimal DailyLimitBefore,
    decimal DailyLimitAfter,
    decimal DepletionPercent,
    string RiskLevel,
    string RiskMessage,
    List<string> RegretSignals,
    bool WillOverdraftBucket,
    bool WillDrawFromBuffer,
    decimal BufferDrawAmount,
    decimal BufferBalanceAfter,
    bool WillDrawFromPool,
    decimal PoolDrawAmount,
    decimal PoolBalanceAfter,
    decimal CurrentBalance,
    decimal PostSpendBalance,
    decimal PaceScore,
    decimal AverageDailySpend,
    int DaysRemaining,
    bool CanAfford);

// ── Dashboard ─────────────────────────────────────────────────────────────────
public record CycleDto(Guid Id, DateTime StartDate, DateTime EndDate, string Status);
public record DashboardSummaryDto(
    decimal TotalIncome,
    decimal TotalSpent,
    decimal SafeToSpend,
    decimal DailyLimit,
    decimal SpendingPace,
    string PaceLabel,
    int DaysRemaining,
    int DaysElapsed,
    int TotalDays,
    List<BucketDto> Buckets,
    List<TransactionDto> Transactions,
    CycleDto? CurrentCycle);

// ── Onboarding ────────────────────────────────────────────────────────────────
public record ExpenseIntake(string Name, string Category, string Nature, decimal MonthlyAmount, bool ReserveOnly);
public record WantCategoryRequest(string Name, bool IsUnpriced, decimal MonthlyAmount);
public record SubmitIntakeRequest(decimal BaselineIncome, List<ExpenseIntake> Items, List<WantCategoryRequest> WantCategories);
public record PathwayBucket(string Name, string Type, decimal AllocationPercent);
public record PathwayPreview(
    string Pathway,
    bool IsAffordable,
    decimal MonthlySavings,
    decimal SaveRate,
    decimal DailyLimit,
    decimal WeeklyLimit,
    decimal AffordabilityGap,
    decimal BufferAmount,
    decimal UnpricedWantsPoolAmount,
    List<string> SuggestedCuts,
    List<PathwayBucket> Buckets);
public record CommitPathwayRequest(string Pathway);

// ── Fixed Obligations ─────────────────────────────────────────────────────────
// Paying one is a pure toggle — a true Fixed Obligation doesn't vary month to
// month. If the real amount changed, that's a re-onboarding update, not this.
public record FixedObligationItem(Guid Id, Guid IntakeItemId, string Name, decimal Reserved, bool IsPaid, DateTime? PaidAt);

// ── Goals ─────────────────────────────────────────────────────────────────────
public record GoalDto(
    Guid Id,
    string Name,
    string Icon,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal ProgressPercent,
    string Status);
public record CreateGoalRequest(string Name, string Icon, decimal TargetAmount);

// ── Review ────────────────────────────────────────────────────────────────────
public record ItemActual(Guid? IntakeItemId, Guid? WantCategoryId, string Name, decimal Reserved, decimal Actual);
public record GoalRef(Guid Id, string Name, decimal ProgressPercent);
public record WeeklyReview(
    bool IsMonthEnd,
    List<ItemActual> Flexible,
    List<ItemActual> Investment,
    List<ItemActual> WantsPriced,
    List<ItemActual> FixedObligations,
    decimal UnpricedPoolBalance,
    decimal BufferBalance,
    decimal TotalSurplus,
    GoalRef? ActiveGoal);
public record ApplySweepRequest(List<IncomeAllocationSplit>? Splits); // required only if TotalSurplus > 0

// ── Cycle rollover ────────────────────────────────────────────────────────────
public record RolloverRequest(List<IncomeAllocationSplit>? Splits); // required only if surplus exists
public record RolloverResponse(string Message, Guid PreviousCycleId, Guid NewCycleId, decimal SurplusSwept);

// ── API Response Wrapper ────────────────────────────────────────────────────────
public record ApiResponse<T>(
    bool IsSuccess,
    string Message,
    T? Data = default);

// ── User Profile ──────────────────────────────────────────────────────────────
public record UserProfileDto(Guid Id, string DisplayName, string Email, bool OnboardingComplete);
public record UpdateProfileRequest(string? DisplayName);
