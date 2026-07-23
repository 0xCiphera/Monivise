using Monivise.App.DTOs;

namespace Monivise.App.API;

public class TransactionApiClient : ApiClient
{
    public TransactionApiClient(HttpClient http) : base(http) { }
    public Task<List<TransactionDto>> RecordIncomeAsync(AddIncomeRequest req) =>
        PostAsync<List<TransactionDto>>("api/transactions/income", req);
    public Task<TransactionDto> RecordExpenseAsync(RecordExpenseRequest req) =>
        PostAsync<TransactionDto>("api/transactions/expense", req);
    public Task<TransactionDto> RecordInvestmentAsync(RecordInvestmentRequest req) =>
        PostAsync<TransactionDto>("api/transactions/investment", req);
    public Task<List<TransactionDto>> GetCurrentCycleAsync() =>
        GetAsync<List<TransactionDto>>("api/transactions/current-cycle");
}

