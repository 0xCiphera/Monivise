using Fluxor;
using Monivise.App.API;
using Monivise.App.State.Actions;

namespace Monivise.App.State.Effects;

public class DashboardEffects
{
    private readonly DashboardApiClient api;
    public DashboardEffects(DashboardApiClient api) => this.api = api;

    [EffectMethod]
    public async Task HandleLoadDashboard(LoadDashboardAction action, IDispatcher dispatcher)
    {
        try
        {
            var summary = await api.GetSummaryAsync();
            dispatcher.Dispatch(new LoadDashboardSuccessAction(summary));
        }
        catch (ApiException ex)
        {
            dispatcher.Dispatch(new LoadDashboardFailureAction(ex.Message));
            dispatcher.Dispatch(new ShowErrorAction(ex.Message));
        }
        catch (Exception)
        {
            const string message = "Network error. Please check your connection.";
            dispatcher.Dispatch(new LoadDashboardFailureAction(message));
            dispatcher.Dispatch(new ShowErrorAction(message));
        }
    }
}