using Fluxor;
using Monivise.App.State.Actions;

namespace Monivise.App.State.Reducers;

public static class AuthReducer
{
    [ReducerMethod] public static AuthState OnLogin(AuthState s, LoginAction _) => s with { IsLoading = true, ErrorMessage = null };
    [ReducerMethod] public static AuthState OnLoginSuccess(AuthState s, LoginSuccessAction a) =>
        s with { IsLoading = false, IsAuthenticated = true, DisplayName = a.Response.DisplayName, ErrorMessage = null };
    [ReducerMethod] public static AuthState OnLoginFailure(AuthState s, LoginFailureAction a) =>
        s with { IsLoading = false, IsAuthenticated = false, ErrorMessage = a.Message };
    [ReducerMethod] public static AuthState OnRegister(AuthState s, RegisterAction _) => s with { IsLoading = true, ErrorMessage = null };
    [ReducerMethod] public static AuthState OnRegisterSuccess(AuthState s, RegisterSuccessAction a) =>
        s with { IsLoading = false, IsAuthenticated = true, DisplayName = a.Response.DisplayName, ErrorMessage = null };
    [ReducerMethod] public static AuthState OnRegisterFailure(AuthState s, RegisterFailureAction a) =>
        s with { IsLoading = false, ErrorMessage = a.Message };
    [ReducerMethod] public static AuthState OnLogoutSuccess(AuthState s, LogoutSuccessAction _) => AuthState.CreateInitial();
    [ReducerMethod] public static AuthState OnClearError(AuthState s, ClearAuthErrorAction _) => s with { ErrorMessage = null };
}
