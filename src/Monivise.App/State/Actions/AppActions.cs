using Monivise.App.DTOs;

namespace Monivise.App.State.Actions;

public record LoadDashboardAction;
public record RequestIncomeSheetAction;
public record AcknowledgeIncomeSheetRequestAction;
public record LoadDashboardSuccessAction(DashboardSummaryDto Summary);
public record LoadDashboardFailureAction(string Message);
public record ShowSuccessAction(string Message);
public record ShowErrorAction(string Message);
public record ShowInfoAction(string Message);
public record DismissMessageAction(Guid Id);

