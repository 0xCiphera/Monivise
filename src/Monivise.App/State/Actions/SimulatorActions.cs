using Monivise.App.DTOs;

namespace Monivise.App.State.Actions;

public record SetSimulatorBucketAction(Guid BucketId);
public record SetSimulatorAmountAction(decimal Amount);
public record PreviewSimulationAction;
public record PreviewSimulationSuccessAction(SimulateResponse Preview);
public record PreviewSimulationFailureAction(string Message);
public record CommitSimulationAction;
public record CommitSimulationSuccessAction(TransactionDto Transaction);
public record CommitSimulationFailureAction(string Message);
public record ClearSimulatorAction;
