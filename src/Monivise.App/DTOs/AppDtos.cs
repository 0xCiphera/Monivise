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
public record BucketDto(
    Guid Id, 
    string Name, 
    string Icon, 
    string Color, 
    string Type,
    decimal AllocationPercent, 
    bool IsActive, 
    int DisplayOrder,
    decimal Allocated, 
    decimal Spent, 
    decimal Balance);

public record CreateBucketRequest(string Name, string Icon, string Color, string Type, decimal AllocationPercent);

public record UpdateBucketRequest(string? Name, string? Icon, string? Color);

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
public record RecordIncomeRequest(decimal Amount, string Source, string IncomeType);
public record RecordExpenseRequest(Guid BucketId, decimal Amount, string? Note);

// ── Simulator ─────────────────────────────────────────────────────────────────
public record SimulateRequest(Guid BucketId, decimal Amount);
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
    decimal CurrentBalance, 
    decimal PostSpendBalance,
    decimal PaceScore, 
    decimal AverageDailySpend,
    int DaysRemaining, 
    bool CanAfford);

// ── Income ────────────────────────────────────────────────────────────────────
public record AllocationResult(Guid BucketId, string BucketName, decimal Amount);

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
public record OnboardingIntakeRequest(decimal BaselineIncome, List<ExpenseIntake> Items);
public record PathwayBucket(string Name, string Type, decimal AllocationPercent);
public record PathwayPreview(
    string Pathway, 
    bool IsAffordable, 
    decimal MonthlySavings, 
    decimal SaveRate,
    decimal DailyLimit, 
    decimal WeeklyLimit, 
    decimal AffordabilityGap,
    List<string> SuggestedCuts, 
    List<PathwayBucket> Buckets);
public record CommitPathwayRequest(string Pathway);

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
public record FixedActual(Guid IntakeItemId, string Name, decimal Reserved, decimal ActualSpent);
public record GoalRef(Guid Id, string Name, decimal ProgressPercent);
public record WeeklyReviewDto(
    List<FixedActual> FixedPrompts, 
    decimal DailyUnderspend, 
    decimal TotalSurplus, 
    GoalRef? ActiveGoal);
public record SweepRequest(Guid GoalId, decimal Amount, List<FixedActual> FixedActuals);

// ── API Response Wrapper───────────────────────────────────────────────────────────────
public record ApiResponse<T>(
    bool IsSuccess,
    string Message,
    T? Data = default);
// ── User Profile ──────────────────────────────────────────────────────────────
public record UserProfileDto(Guid Id, string DisplayName, string Email, bool OnboardingComplete);
public record UpdateProfileRequest(string? DisplayName);
