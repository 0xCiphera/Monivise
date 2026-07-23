using Monivise.App.DTOs;

namespace Monivise.App.API
{
    public class FixedObligationApiClient : ApiClient
    {
        public FixedObligationApiClient(HttpClient http) : base(http) { }
        public Task<List<FixedObligationItem>> GetChecklistAsync() =>
            GetAsync<List<FixedObligationItem>>("api/fixed-obligations");
        public Task MarkPaidAsync(Guid id) => PostAsync($"api/fixed-obligations/{id}/pay");
        public Task MarkUnpaidAsync(Guid id) => PostAsync($"api/fixed-obligations/{id}/unpay");
    }

}
