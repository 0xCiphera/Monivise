using Monivise.App.DTOs;
using Monivise.App.DTOs.Buckets;

namespace Monivise.App.API;

public class BucketApiClient : ApiClient
{
    public BucketApiClient(HttpClient http) : base(http) { }
    public Task<List<BucketDto>> GetBucketsAsync() => GetAsync<List<BucketDto>>("api/buckets");
}
