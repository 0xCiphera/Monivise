using Fluxor;
using Monivise.App.State;
using Monivise.App.State.Actions;

namespace Monivise.App.State.Reducers;

public static class AppReducer
{
    [ReducerMethod] public static AppState OnLoadDashboard(AppState s, LoadDashboardAction _) => s with { IsLoading = true };
    [ReducerMethod] public static AppState OnLoadDashboardSuccess(AppState s, LoadDashboardSuccessAction a) =>
        s with { IsLoading = false, Dashboard = a.Summary, Buckets = a.Summary.Buckets, Transactions = a.Summary.Transactions };
    [ReducerMethod] public static AppState OnLoadDashboardFailure(AppState s, LoadDashboardFailureAction _) => s with { IsLoading = false };
    [ReducerMethod] public static AppState OnShowSuccess(AppState s, ShowSuccessAction a) => s with { Messages = Append(s.Messages, a.Message, "success") };
    [ReducerMethod] public static AppState OnShowError(AppState s, ShowErrorAction a) => s with { Messages = Append(s.Messages, a.Message, "error") };
    [ReducerMethod] public static AppState OnShowInfo(AppState s, ShowInfoAction a) => s with { Messages = Append(s.Messages, a.Message, "info") };
    [ReducerMethod] public static AppState OnDismiss(AppState s, DismissMessageAction a) => s with { Messages = s.Messages.Where(m => m.Id != a.Id).ToList() };
    [ReducerMethod] public static AppState OnSetBucket(AppState s, SetSimulatorBucketAction a) => s with { SimulatorBucketId = a.BucketId, SimulatorPreview = null, SimulatorError = null };
    [ReducerMethod] public static AppState OnSetAmount(AppState s, SetSimulatorAmountAction a) => s with { SimulatorAmount = a.Amount };
    [ReducerMethod] public static AppState OnPreview(AppState s, PreviewSimulationAction _) => s with { SimulatorIsLoading = true, SimulatorError = null };
    [ReducerMethod] public static AppState OnPreviewSuccess(AppState s, PreviewSimulationSuccessAction a) => s with { SimulatorIsLoading = false, SimulatorPreview = a.Preview };
    [ReducerMethod] public static AppState OnPreviewFailure(AppState s, PreviewSimulationFailureAction a) => s with { SimulatorIsLoading = false, SimulatorError = a.Message };
    [ReducerMethod] public static AppState OnClearSimulator(AppState s, ClearSimulatorAction _) => s with { SimulatorBucketId = null, SimulatorAmount = 0, SimulatorPreview = null, SimulatorError = null };

    private static List<AppMessage> Append(List<AppMessage> msgs, string text, string variant) =>
        msgs.Append(new AppMessage(Guid.NewGuid(), text, variant)).ToList();
}
