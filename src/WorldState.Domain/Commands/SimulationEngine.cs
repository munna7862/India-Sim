using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class SimulationEngine
{
    private readonly GameSession _session;

    public GameSession Session => _session;

    public event Action<ICommand, CommandResult>? OnCommandExecuted;
    public event Action<DateOnly>? OnDayTicked;
    public event Action<DateOnly>? OnMonthRollover;

    public SimulationEngine(GameSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        WireClockEvents();
    }

    private void WireClockEvents()
    {
        _session.Clock.OnDayPassed += HandleDayPassed;
        _session.Clock.OnMonthPassed += HandleMonthPassed;
    }

    public void Tick(double deltaSeconds)
    {
        _session.Clock.Tick(deltaSeconds);
    }

    public CommandResult Dispatch(ICommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var result = command.Execute(_session);
        OnCommandExecuted?.Invoke(command, result);
        return result;
    }

    private void HandleDayPassed(DateOnly newDate)
    {
        // 1. Process Timed Operations
        var completedOps = new List<TimedOperation>();
        foreach (var op in _session.TimedOperations)
        {
            if (!op.IsCompleted && newDate >= op.CompletionDate)
            {
                CompleteTimedOperation(op);
                completedOps.Add(op);
            }
        }

        _session.TimedOperations.RemoveAll(op => op.IsCompleted);

        // 2. Clear expired cooldowns
        _session.Cooldowns.ClearExpired(newDate);

        OnDayTicked?.Invoke(newDate);
    }

    private void CompleteTimedOperation(TimedOperation op)
    {
        op.IsCompleted = true;
        if (!_session.Countries.TryGetValue(op.CountryId, out var country))
        {
            return;
        }

        switch (op.OperationType)
        {
            case "Construction":
                string facility = op.Parameters.GetValueOrDefault("FacilityType", "Farms");
                int qty = int.TryParse(op.Parameters.GetValueOrDefault("Quantity", "1"), out int q) ? q : 1;
                int count = country.Facilities.GetValueOrDefault(facility, 0);
                country.Facilities[facility] = count + qty;

                _session.AddEvent(new GameEvent(
                    Id: Guid.NewGuid().ToString("N"),
                    Date: _session.Clock.CurrentDate,
                    Title: "Facility Construction Finished",
                    Message: $"{country.Name} completed construction of {qty}x {facility} (Total: {country.Facilities[facility]}).",
                    Category: EventCategory.Economy,
                    Severity: EventSeverity.Info,
                    ActorCountryId: op.CountryId
                ));
                break;

            case "ArmyDrills":
                country.Stability = Math.Min(100.0, country.Stability + 2.0);
                _session.AddEvent(new GameEvent(
                    Id: Guid.NewGuid().ToString("N"),
                    Date: _session.Clock.CurrentDate,
                    Title: "Army Drills Concluded",
                    Message: $"{country.Name} successfully concluded comprehensive army drills, improving force readiness and national stability.",
                    Category: EventCategory.Military,
                    Severity: EventSeverity.Info,
                    ActorCountryId: op.CountryId
                ));
                break;
        }
    }

    private void HandleMonthPassed(DateOnly newDate)
    {
        // 1. Execute invariant monthly ledger across all countries
        foreach (var countryId in _session.Countries.Keys)
        {
            var ledgerCmd = new ExecuteMonthlyLedgerCommand(countryId);
            ledgerCmd.Execute(_session);
        }

        // 2. Evaluate tasks against authoritative world state
        EvaluateTasks();

        OnMonthRollover?.Invoke(newDate);
    }

    public void EvaluateTasks()
    {
        var playerCountry = _session.GetPlayerCountry();
        if (playerCountry == null) return;

        foreach (var (_, task) in _session.Tasks)
        {
            if (task.Status == GameTaskStatus.Completed) continue;

            bool stateChanged = false;
            foreach (var subTask in task.SubTasks)
            {
                if (subTask.Id.StartsWith("ST_"))
                {
                    string facilityKey = subTask.Id switch
                    {
                        "ST_POWER" => "PowerPlants",
                        "ST_FARMS" => "Farms",
                        "ST_MINES" => "Mines",
                        "ST_FACTORIES" => "Factories",
                        "ST_HOSPITALS" => "Hospitals",
                        "ST_SCHOOLS" => "Schools",
                        "ST_BARRACKS" => "Barracks",
                        _ => ""
                    };

                    if (!string.IsNullOrEmpty(facilityKey) && playerCountry.Facilities.TryGetValue(facilityKey, out int currentVal))
                    {
                        if (subTask.CurrentCount != currentVal)
                        {
                            subTask.CurrentCount = currentVal;
                            stateChanged = true;
                        }
                    }
                }
            }

            if (task.CheckCompletion() && stateChanged)
            {
                _session.AddEvent(new GameEvent(
                    Id: Guid.NewGuid().ToString("N"),
                    Date: _session.Clock.CurrentDate,
                    Title: "Presidential Task Completed",
                    Message: $"Goal reached: '{task.Title}'! Reward ready to claim in Presidential Objectives.",
                    Category: EventCategory.Tasks,
                    Severity: EventSeverity.Info,
                    ActorCountryId: playerCountry.Id
                ));
            }
        }
    }
}
