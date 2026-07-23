using Fluxor;
using Monivise.App.API;
using Monivise.App.DTOs;
using Monivise.App.State.Actions;

namespace Monivise.App.State.Effects;

public class SimulatorEffects
{
    private readonly SimulatorApiClient api;
    private readonly IState<AppState> state;

    public SimulatorEffects(SimulatorApiClient api, IState<AppState> state)
    {
        this.api = api;
        this.state = state;
    }

    [EffectMethod]
    public async Task HandlePreview(PreviewSimulationAction action, IDispatcher dispatcher)
    {
        var s = state.Value;
        if (s.SimulatorBucketId is null || s.SimulatorAmount <= 0) return;

        try
        {
            var preview = await api.PreviewAsync(new SimulateRequest(s.SimulatorBucketId.Value, null, null, s.SimulatorAmount));
            dispatcher.Dispatch(new PreviewSimulationSuccessAction(preview));
        }
        catch (ApiException ex)
        {
            dispatcher.Dispatch(new PreviewSimulationFailureAction(ex.Message));
        }
        catch (Exception)
        {
            dispatcher.Dispatch(new PreviewSimulationFailureAction("Network error. Please check your connection."));
        }
    }

    [EffectMethod]
    public async Task HandleCommit(CommitSimulationAction action, IDispatcher dispatcher)
    {
        var s = state.Value;
        if (s.SimulatorBucketId is null || s.SimulatorAmount <= 0) return;

        try
        {
            var tx = await api.CommitAsync(new SimulateRequest(s.SimulatorBucketId.Value, null, null, s.SimulatorAmount));
            dispatcher.Dispatch(new CommitSimulationSuccessAction(tx));
            dispatcher.Dispatch(new ShowSuccessAction("Spend recorded."));
            dispatcher.Dispatch(new ClearSimulatorAction());
            dispatcher.Dispatch(new LoadDashboardAction());
        }
        catch (ApiException ex)
        {
            dispatcher.Dispatch(new CommitSimulationFailureAction(ex.Message));
            dispatcher.Dispatch(new ShowErrorAction(ex.Message));
        }
        catch (Exception)
        {
            const string message = "Network error. Please check your connection.";
            dispatcher.Dispatch(new CommitSimulationFailureAction(message));
            dispatcher.Dispatch(new ShowErrorAction(message));
        }
    }
}