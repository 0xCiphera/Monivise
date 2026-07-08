using Fluxor;
using Microsoft.AspNetCore.Components;
using Monivise.App.API;
using Monivise.App.Authentication;
using Monivise.App.State.Actions;

namespace Monivise.App.State.Effects;

public class AuthEffects
{
    private readonly AuthApiClient api;
    private readonly TokenStore tokenStore;
    private readonly NavigationManager nav;

    public AuthEffects(AuthApiClient api, TokenStore tokenStore, NavigationManager nav)
    {
        this.api = api;
        this.tokenStore = tokenStore;
        this.nav = nav;
    }

    [EffectMethod]
    public async Task HandleLogin(LoginAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await api.LoginAsync(new(action.Email, action.Password));
            tokenStore.Set(response.AccessToken, response.ExpiresAt);
            dispatcher.Dispatch(new LoginSuccessAction(response));
            nav.NavigateTo("/dashboard");
        }
        catch (ApiException ex)
        {
            var message = ex.StatusCode switch
            {
                401 => "Invalid email or password.",
                >= 500 => "Something went wrong. Please try again.",
                0 => "Network error. Please check your connection.",
                _ => ex.Message
            };
            dispatcher.Dispatch(new LoginFailureAction(message));
        }
        catch (Exception)
        {
            dispatcher.Dispatch(new LoginFailureAction("Network error. Please check your connection."));
        }
    }

    [EffectMethod]
    public async Task HandleRegister(RegisterAction action, IDispatcher dispatcher)
    {
        try
        {
            var response = await api.RegisterAsync(new(action.DisplayName, action.Email, action.Password, action.Currency));
            tokenStore.Set(response.AccessToken, response.ExpiresAt);
            dispatcher.Dispatch(new RegisterSuccessAction(response));
            nav.NavigateTo("/onboarding");
        }
        catch (ApiException ex)
        {
            var message = ex.StatusCode switch
            {
                409 => "An account with this email already exists.",
                422 when ex.FieldErrors is not null => string.Join(" ", ex.FieldErrors.Values.SelectMany(v => v)),
                >= 500 => "Something went wrong. Please try again.",
                0 => "Network error. Please check your connection.",
                _ => ex.Message
            };
            dispatcher.Dispatch(new RegisterFailureAction(message));
        }
        catch (Exception)
        {
            dispatcher.Dispatch(new RegisterFailureAction("Network error. Please check your connection."));
        }
    }

    [EffectMethod]
    public async Task HandleLogout(LogoutAction action, IDispatcher dispatcher)
    {
        try
        {
            await api.LogoutAsync();
        }
        catch (ApiException)
        {
            // Logging out locally still proceeds even if the server call fails —
            // the user's intent is to leave, and we must not trap them in the app.
        }
        finally
        {
            tokenStore.Clear();
            dispatcher.Dispatch(new LogoutSuccessAction());
            nav.NavigateTo("/login");
        }
    }
}