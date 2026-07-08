using Fluxor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Monivise.App;
using Monivise.App.API;
using Monivise.App.Authentication;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseAddress = builder.HostEnvironment.BaseAddress; // adjust if the API is on a different origin

builder.Services.AddSingleton<TokenStore>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddAuthorizationCore();

// Raw client for refresh calls only — no auth handler attached, avoids recursive 401 handling.
builder.Services.AddHttpClient("MoniviseRawApi", client => client.BaseAddress = new Uri(apiBaseAddress));

builder.Services.AddScoped<RefreshTokenHandler>(sp =>
{
    var tokenStore = sp.GetRequiredService<TokenStore>();
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var nav = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new RefreshTokenHandler(tokenStore, factory, () => nav.NavigateTo("/login"));
});

builder.Services.AddHttpClient("MoniviseApi", client => client.BaseAddress = new Uri(apiBaseAddress))
    .AddHttpMessageHandler<RefreshTokenHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("MoniviseApi"));

builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<DashboardApiClient>();
builder.Services.AddScoped<BucketApiClient>();
builder.Services.AddScoped<SimulatorApiClient>();
builder.Services.AddScoped<IncomeApiClient>();
builder.Services.AddScoped<TransactionApiClient>();
builder.Services.AddScoped<OnboardingApiClient>();
builder.Services.AddScoped<GoalApiClient>();
builder.Services.AddScoped<ReviewApiClient>();

builder.Services.AddFluxor(options => options.ScanAssemblies(typeof(Program).Assembly));

await builder.Build().RunAsync();