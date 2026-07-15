using Monivise.App.DTOs;

namespace Monivise.App.State.Actions;

public record LoginAction(string Email, string Password);
public record LoginSuccessAction(AuthResponse Response);
public record LoginFailureAction(string Message);
public record RegisterAction(string DisplayName, string Email, string Password);
public record RegisterSuccessAction(AuthResponse Response);
public record RegisterFailureAction(string Message);
public record LogoutAction;
public record LogoutSuccessAction;
public record ClearAuthErrorAction;
