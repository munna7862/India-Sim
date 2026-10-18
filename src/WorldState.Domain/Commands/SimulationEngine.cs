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
        _session.PolicyRegistry ??= Economy.EconomicPolicyRegistry.CreateDefaultCatalog();
        _session.MilitaryRegistry ??= Military.MilitaryUnitRegistry.CreateDefaultCatalog();
        _session.AchievementRegistry ??= Achievements.AchievementRegistry.CreateDefaultCatalog();
        _session.WorldEventRegistry ??= Events.WorldEventRegistry.CreateDefaultCatalog();
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

    private Events.DomesticEventRegistry? _eventRegistry;

    public Events.DomesticEventRegistry EventRegistry
    {
        get => _eventRegistry ??= Events.DomesticEventRegistry.CreateDefaultCatalog();
        set => _eventRegistry = value;
    }

    private Economy.EconomicPolicyRegistry? _policyRegistry;

    public Economy.EconomicPolicyRegistry PolicyRegistry
    {
        get => _policyRegistry ??= _session.PolicyRegistry ?? Economy.EconomicPolicyRegistry.CreateDefaultCatalog();
        set
        {
            _policyRegistry = value;
            _session.PolicyRegistry = value;
        }
    }

    private Achievements.AchievementRegistry? _achievementRegistry;

    public Achievements.AchievementRegistry AchievementRegistry
    {
        get => _achievementRegistry ??= _session.AchievementRegistry ?? Achievements.AchievementRegistry.CreateDefaultCatalog();
        set
        {
            _achievementRegistry = value;
            _session.AchievementRegistry = value;
        }
    }

    private Events.WorldEventRegistry? _worldEventRegistry;

    public Events.WorldEventRegistry WorldEventRegistry
    {
        get => _worldEventRegistry ??= _session.WorldEventRegistry ?? Events.WorldEventRegistry.CreateDefaultCatalog();
        set
        {
            _worldEventRegistry = value;
            _session.WorldEventRegistry = value;
        }
    }

    private void HandleMonthPassed(DateOnly newDate)
    {
        // 1. Execute invariant monthly ledger across all countries
        foreach (var country in _session.Countries.Values)
        {
            var ledgerCmd = new ExecuteMonthlyLedgerCommand(country.Id);
            ledgerCmd.Execute(_session);

            // Update national indicators
            UpdateNationalIndicators(country);
        }

        // 2. Process domestic events
        ProcessDomesticEvents(newDate);

        // 3. Process world events
        ProcessWorldEvents(newDate);

        // 4. Evaluate tasks against authoritative world state
        EvaluateTasks();

        // 5. Evaluate achievements
        EvaluateAchievements();

        OnMonthRollover?.Invoke(newDate);
    }

    public void UpdateNationalIndicators(CountryState country)
    {
        // 1. Quality of living
        double healthEff = country.Ministries.TryGetValue(MinistryType.Health, out var hlth) ? hlth.CalculateEfficiency() : 0.50;
        int hospitals = country.Facilities.GetValueOrDefault("Hospitals", 10);
        double healthScore = Math.Clamp(healthEff * (hospitals / 30.0), 0.0, 1.0);

        double eduEff = country.Ministries.TryGetValue(MinistryType.Education, out var edu) ? edu.CalculateEfficiency() : 0.50;
        int schools = country.Facilities.GetValueOrDefault("Schools", 10);
        double eduScore = Math.Clamp(eduEff * (schools / 40.0), 0.0, 1.0);

        double infraEff = country.Ministries.TryGetValue(MinistryType.Infrastructure, out var infra) ? infra.CalculateEfficiency() : 0.50;
        int power = country.Facilities.GetValueOrDefault("PowerPlants", 10);
        double infraScore = Math.Clamp(infraEff * (power / 40.0), 0.0, 1.0);

        double econScore = Math.Clamp((1.0 - country.IncomeTaxRate) * (1.0 - (country.CorruptionIndex / 100.0)), 0.0, 1.0);

        double targetQol = (healthScore * 0.30 + eduScore * 0.25 + infraScore * 0.25 + econScore * 0.20) * 100.0;
        country.QualityOfLiving = Math.Clamp(country.QualityOfLiving + (targetQol - country.QualityOfLiving) * 0.10, 0.0, 100.0);

        // 2. Approval Rating
        double targetApproval = country.QualityOfLiving;
        double avgTax = (country.IncomeTaxRate + country.CorporateTaxRate + country.VatRate) / 3.0;
        if (avgTax > 0.25)
        {
            targetApproval -= (avgTax - 0.25) * 40.0; // Discontent from high tax
        }

        foreach (var m in country.Ministries.Values)
        {
            if (m.Funding == FundingLevel.Zero)
            {
                targetApproval -= 4.0;
            }
        }

        var ideologyDef = IdeologyDefinition.Get(country.Ideology);
        targetApproval += ideologyDef.ApprovalModifier;

        country.ApprovalRating = Math.Clamp(country.ApprovalRating + (targetApproval - country.ApprovalRating) * 0.10, 0.0, 100.0);

        // 3. National Stability
        double targetStability = 50.0 + (country.ApprovalRating - 50.0) * 0.5;
        if (country.CorruptionIndex > 30.0)
        {
            targetStability -= (country.CorruptionIndex - 30.0) * 0.3;
        }

        double intAffairsEff = country.Ministries.TryGetValue(MinistryType.InternalAffairs, out var intAff) ? intAff.CalculateEfficiency() : 0.50;
        targetStability += (intAffairsEff - 0.50) * 20.0;
        targetStability += ideologyDef.StabilityModifier;

        country.Stability = Math.Clamp(country.Stability + (targetStability - country.Stability) * 0.10, 0.0, 100.0);

        // 4. Corruption drift
        double justEff = country.Ministries.TryGetValue(MinistryType.Justice, out var just) ? just.CalculateEfficiency() : 0.50;
        double netCorruptionDelta = 0.15 - (justEff * 0.15 + intAffairsEff * 0.10);
        country.CorruptionIndex = Math.Clamp(country.CorruptionIndex + netCorruptionDelta, 0.0, 100.0);
    }

    public void ProcessDomesticEvents(DateOnly newDate)
    {
        // 1. Decrement event cooldowns
        var keys = _session.EventCooldowns.Keys.ToList();
        foreach (var k in keys)
        {
            if (_session.EventCooldowns[k] > 0)
            {
                _session.EventCooldowns[k]--;
                if (_session.EventCooldowns[k] <= 0)
                {
                    _session.EventCooldowns.Remove(k);
                }
            }
        }

        if (_session.PendingEvent != null) return;

        var playerCountry = _session.GetPlayerCountry();
        if (playerCountry == null) return;

        var catalog = EventRegistry;
        int monthIndex = (newDate.Year * 12) + newDate.Month;
        var rng = new Random(_session.Seed + monthIndex);

        foreach (var evtDef in catalog.Events)
        {
            int cd = _session.EventCooldowns.GetValueOrDefault(evtDef.Id, 0);
            if (evtDef.CanTrigger(playerCountry, cd))
            {
                if (rng.NextDouble() < evtDef.RandomWeight)
                {
                    _session.PendingEvent = new ActiveEventPrompt
                    {
                        EventId = evtDef.Id,
                        CountryId = playerCountry.Id,
                        Date = newDate,
                        Title = evtDef.Title,
                        Description = evtDef.Description,
                        Category = Enum.TryParse<EventCategory>(evtDef.Category, true, out var cat) ? cat : EventCategory.Emergency,
                        Choices = evtDef.Choices.ToList()
                    };

                    _session.AddEvent(new GameEvent(
                        Id: Guid.NewGuid().ToString("N"),
                        Date: newDate,
                        Title: $"EMERGENCY: {evtDef.Title}",
                        Message: evtDef.Description,
                        Category: EventCategory.Emergency,
                        Severity: EventSeverity.Critical,
                        ActorCountryId: playerCountry.Id
                    ));

                    break; // Max 1 new emergency per month
                }
            }
        }
    }

    public void EvaluateTasks()
    {
        var playerCountry = _session.GetPlayerCountry();
        if (playerCountry == null) return;

        foreach (var (_, task) in _session.Tasks)
        {
            if (task.Status == GameTaskStatus.Completed || task.Status == GameTaskStatus.Inactive) continue;

            bool stateChanged = false;
            foreach (var subTask in task.SubTasks)
            {
                int currentVal = subTask.CurrentCount;

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

                    if (!string.IsNullOrEmpty(facilityKey) && playerCountry.Facilities.TryGetValue(facilityKey, out int facCount))
                    {
                        currentVal = facCount;
                    }
                    else if (subTask.Id == "ST_APPROVAL_70")
                    {
                        currentVal = (int)Math.Floor(playerCountry.ApprovalRating);
                    }
                    else if (subTask.Id == "ST_STABILITY_75")
                    {
                        currentVal = (int)Math.Floor(playerCountry.Stability);
                    }
                    else if (subTask.Id == "ST_CORRUPTION_20")
                    {
                        currentVal = (int)Math.Max(0.0, Math.Floor(100.0 - playerCountry.CorruptionIndex));
                    }
                }

                if (subTask.CurrentCount != currentVal)
                {
                    subTask.CurrentCount = currentVal;
                    stateChanged = true;
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

                // Unlock any dependent tasks that were waiting for this task
                foreach (var otherTask in _session.Tasks.Values)
                {
                    if (otherTask.PrerequisiteTaskId == task.Id && otherTask.Status == GameTaskStatus.Inactive)
                    {
                        otherTask.Status = GameTaskStatus.Active;
                        _session.AddEvent(new GameEvent(
                            Id: Guid.NewGuid().ToString("N"),
                            Date: _session.Clock.CurrentDate,
                            Title: "New Objective Available",
                            Message: $"New task unlocked: '{otherTask.Title}'. Check Presidential Objectives for briefing.",
                            Category: EventCategory.Tasks,
                            Severity: EventSeverity.Info,
                            ActorCountryId: playerCountry.Id
                        ));
                    }
                }
            }
        }
    }

    public void ProcessWorldEvents(DateOnly newDate)
    {
        var playerCountry = _session.GetPlayerCountry();
        if (playerCountry == null) return;

        int monthIndex = (newDate.Year * 12) + newDate.Month;
        var rng = new Random(_session.Seed + (monthIndex * 37) + 11);

        foreach (var evtDef in WorldEventRegistry.Events)
        {
            int cd = _session.EventCooldowns.GetValueOrDefault(evtDef.Id, 0);
            if (cd <= 0 && rng.NextDouble() < evtDef.TriggerChance)
            {
                evtDef.ApplyEffects(_session, playerCountry);
                _session.EventCooldowns[evtDef.Id] = evtDef.CooldownMonths;

                var cat = Enum.TryParse<EventCategory>(evtDef.Category, true, out var c) ? c : EventCategory.World;
                var sev = Enum.TryParse<EventSeverity>(evtDef.Severity, true, out var s) ? s : EventSeverity.Info;
                var route = !string.IsNullOrEmpty(evtDef.ActionRoute) && Enum.TryParse<Navigation.ScreenRoute>(evtDef.ActionRoute, true, out var r) ? (Navigation.ScreenRoute?)r : null;

                _session.AddEvent(new GameEvent(
                    Id: Guid.NewGuid().ToString("N"),
                    Date: newDate,
                    Title: evtDef.Title,
                    Message: evtDef.Description,
                    Category: cat,
                    Severity: sev,
                    ActorCountryId: playerCountry.Id,
                    ActionRoute: route,
                    IsRead: false
                ));

                break; // Max 1 major world event per month
            }
        }
    }

    public void EvaluateAchievements()
    {
        var playerCountry = _session.GetPlayerCountry();
        if (playerCountry == null) return;

        foreach (var def in AchievementRegistry.Achievements)
        {
            if (!_session.Achievements.TryGetValue(def.Id, out var progress))
            {
                progress = new AchievementProgress
                {
                    AchievementId = def.Id,
                    IsUnlocked = false,
                    ProgressPercent = 0.0
                };
                _session.Achievements[def.Id] = progress;
            }

            var (isMet, percent) = def.Evaluate(playerCountry, _session);
            progress.ProgressPercent = percent;

            if (isMet && !progress.IsUnlocked)
            {
                progress.IsUnlocked = true;
                progress.UnlockedDate = _session.Clock.CurrentDate;

                _session.AddEvent(new GameEvent(
                    Id: Guid.NewGuid().ToString("N"),
                    Date: _session.Clock.CurrentDate,
                    Title: $"Achievement Unlocked: {def.Title}",
                    Message: $"Milestone achieved! '{def.Title}': {def.Description}. Claim reward in the Achievements menu.",
                    Category: EventCategory.Achievements,
                    Severity: EventSeverity.Info,
                    ActorCountryId: playerCountry.Id,
                    ActionRoute: Navigation.ScreenRoute.Achievements,
                    IsRead: false
                ));
            }
        }
    }
}
