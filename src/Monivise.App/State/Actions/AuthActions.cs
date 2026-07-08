using Monivise.App.DTOs;
using Monivise.App.DTOs.Auth;

namespace Monivise.App.State.Actions;

public record LoginAction(string Email, string Password);
public record LoginSuccessAction(AuthResponse Response);
public record LoginFailureAction(string Message);
public record RegisterAction(string DisplayName, string Email, string Password, string Currency);
public record RegisterSuccessAction(AuthResponse Response);
public record RegisterFailureAction(string Message);
public record LogoutAction;
public record LogoutSuccessAction;
public record ClearAuthErrorAction;
