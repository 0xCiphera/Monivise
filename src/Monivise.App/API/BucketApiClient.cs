using Monivise.App.DTOs;

namespace Monivise.App.API;

public class BucketApiClient : ApiClient
{
    public BucketApiClient(HttpClient http) : base(http) { }
    public Task<List<BucketDto>> GetBucketsAsync() => GetAsync<List<BucketDto>>("api/buckets");
    public Task<BucketDto> CreateAsync(CreateBucketRequest req) => PostAsync<BucketDto>("api/buckets", req);
    public Task<BucketDto> UpdateAsync(Guid id, UpdateBucketRequest req) => PutAsync<BucketDto>($"api/buckets/{id}", req);
    public Task DeleteAsync(Guid id) => base.DeleteAsync($"api/buckets/{id}");
}
