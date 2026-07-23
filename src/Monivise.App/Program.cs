using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Monivise.App;
using Monivise.App.API;
using Monivise.App.Authentication;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseAddress = "https://localhost:7082/";

builder.Services.AddSingleton<TokenStore>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddAuthorizationCore();

builder.Services.AddTransient<BrowserCredentialsHandler>();

builder.Services.AddHttpClient("MoniviseRawApi", client => client.BaseAddress = new Uri(apiBaseAddress))
    .AddHttpMessageHandler<BrowserCredentialsHandler>();

builder.Services.AddScoped<RefreshTokenHandler>(sp =>
{
    var tokenStore = sp.GetRequiredService<TokenStore>();
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var nav = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new RefreshTokenHandler(tokenStore, factory, () => nav.NavigateTo("/login"));
});

builder.Services.AddHttpClient("MoniviseApi", client => client.BaseAddress = new Uri(apiBaseAddress))
    .AddHttpMessageHandler<BrowserCredentialsHandler>()
    .AddHttpMessageHandler<RefreshTokenHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("MoniviseApi"));

builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<DashboardApiClient>();
builder.Services.AddScoped<SimulatorApiClient>();
builder.Services.AddScoped<TransactionApiClient>();
builder.Services.AddScoped<OnboardingApiClient>();
builder.Services.AddScoped<GoalApiClient>();
builder.Services.AddScoped<ReviewApiClient>();
builder.Services.AddScoped<UserApiClient>();
builder.Services.AddScoped<BudgetCycleApiClient>();
builder.Services.AddScoped<FixedObligationApiClient>();

builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.UseReduxDevTools();
});

// Register Effects manually
builder.Services.AddScoped<Monivise.App.State.Effects.AuthEffects>();
builder.Services.AddScoped<Monivise.App.State.Effects.DashboardEffects>();
builder.Services.AddScoped<Monivise.App.State.Effects.SimulatorEffects>();

await builder.Build().RunAsync();