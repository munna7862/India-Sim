using System;
using System.Linq;
using Godot;
using WorldState.Domain.Commands;
using WorldState.Domain.Models;
using WorldState.Domain.Navigation;
using WorldState.Domain.Services;
using WorldState.Domain.ViewModels;

namespace WorldState.Desktop.Scenes;

public partial class GameShell : Control
{
    public static GameSession? ActiveSession { get; set; }
    private GameSession _session = ActiveSession ?? GameSession.CreateNewGame("IND");
    private SimulationEngine _engine = null!;
    private readonly SaveManager _saveManager = new();
    private readonly NavigationService _nav = new();

    private Label? _dateLabel;
    private Label? _treasuryLabel;
    private Label? _popLabel;
    private Label? _approvalLabel;
    private Label? _livingLabel;
    private Label? _speedLabel;
    private Button? _pauseBtn;
    private Button? _speed1Btn;
    private Button? _speed2Btn;
    private Button? _speed3Btn;

    // Phase 04 Navigation & Panels
    private Button? _btnOverview;
    private Button? _btnCabinet;
    private Button? _btnTasks;
    private Button? _btnEconomy;

    private Panel? _cabinetPanel;
    private Panel? _tasksPanel;
    private Panel? _emergencyDialog;
    private Panel? _economyPanel;

    private Label? _ideologyLabel;
    private Button? _changeIdeologyBtn;
    private VBoxContainer? _ministryList;
    private Label? _tasksSummaryLabel;
    private VBoxContainer? _tasksList;

    private Label? _eventTitle;
    private Label? _eventDescription;
    private VBoxContainer? _choicesContainer;

    // Phase 05 Economy Controls
    private Label? _gdpLabel;
    private Label? _creditRatingLabel;
    private Label? _debtLabel;
    private Button? _borrowBtn;
    private Button? _repayBtn;
    private Label? _ledgerSummaryLabel;
    private VBoxContainer? _taxesContainer;
    private VBoxContainer? _facilitiesContainer;
    private VBoxContainer? _policiesContainer;

    // Phase 06 Diplomacy & Military Controls
    private Button? _btnDiplomacy;
    private Button? _btnMilitary;
    private Panel? _diplomacyPanel;
    private Panel? _militaryPanel;
    private Label? _diplomacySummaryLabel;
    private VBoxContainer? _nationsList;
    private Label? _forceStrengthLabel;
    private Label? _drillsStatusLabel;
    private Button? _drillsBtn;
    private Button? _counterTerrorBtn;
    private Button? _protectionLevelBtn;
    private VBoxContainer? _unitsList;

    public override void _Ready()

    {
        if (ActiveSession != null)
        {
            _session = ActiveSession;
        }

        _engine = new SimulationEngine(_session);
        _engine.OnMonthRollover += date =>
        {
            _saveManager.AutoSave(_session);
        };

        _dateLabel = GetNodeOrNull<Label>("TopBar/HBox/DateLabel");
        _treasuryLabel = GetNodeOrNull<Label>("TopBar/HBox/TreasuryLabel");
        _popLabel = GetNodeOrNull<Label>("TopBar/HBox/PopLabel");
        _approvalLabel = GetNodeOrNull<Label>("TopBar/HBox/ApprovalLabel");
        _livingLabel = GetNodeOrNull<Label>("TopBar/HBox/LivingLabel");
        _speedLabel = GetNodeOrNull<Label>("TopBar/HBox/SpeedLabel");

        _pauseBtn = GetNodeOrNull<Button>("TopBar/HBox/SpeedControls/PauseButton");
        _speed1Btn = GetNodeOrNull<Button>("TopBar/HBox/SpeedControls/Speed1Button");
        _speed2Btn = GetNodeOrNull<Button>("TopBar/HBox/SpeedControls/Speed2Button");
        _speed3Btn = GetNodeOrNull<Button>("TopBar/HBox/SpeedControls/Speed3Button");

        if (_pauseBtn != null) _pauseBtn.Pressed += () => SetSpeed(ClockSpeed.Paused);
        if (_speed1Btn != null) _speed1Btn.Pressed += () => SetSpeed(ClockSpeed.Normal);
        if (_speed2Btn != null) _speed2Btn.Pressed += () => SetSpeed(ClockSpeed.Fast);
        if (_speed3Btn != null) _speed3Btn.Pressed += () => SetSpeed(ClockSpeed.Ultra);

        // Left Navigation buttons
        _btnOverview = GetNodeOrNull<Button>("LeftNavRail/VBox/BtnOverview");
        _btnCabinet = GetNodeOrNull<Button>("LeftNavRail/VBox/BtnCabinet");
        _btnTasks = GetNodeOrNull<Button>("LeftNavRail/VBox/BtnTasks");
        _btnEconomy = GetNodeOrNull<Button>("LeftNavRail/VBox/BtnEconomy");
        _btnDiplomacy = GetNodeOrNull<Button>("LeftNavRail/VBox/BtnDiplomacy");
        _btnMilitary = GetNodeOrNull<Button>("LeftNavRail/VBox/BtnMilitary");

        if (_btnOverview != null) _btnOverview.Pressed += () => ShowPanel(null);
        if (_btnCabinet != null) _btnCabinet.Pressed += () => ShowPanel(_cabinetPanel);
        if (_btnTasks != null) _btnTasks.Pressed += () => ShowPanel(_tasksPanel);
        if (_btnEconomy != null) _btnEconomy.Pressed += () => ShowPanel(_economyPanel);
        if (_btnDiplomacy != null) _btnDiplomacy.Pressed += () => ShowPanel(_diplomacyPanel);
        if (_btnMilitary != null) _btnMilitary.Pressed += () => ShowPanel(_militaryPanel);

        // Panels
        _cabinetPanel = GetNodeOrNull<Panel>("CabinetPanel");
        _tasksPanel = GetNodeOrNull<Panel>("TasksPanel");
        _emergencyDialog = GetNodeOrNull<Panel>("EmergencyDialog");
        _economyPanel = GetNodeOrNull<Panel>("EconomyPanel");
        _diplomacyPanel = GetNodeOrNull<Panel>("DiplomacyPanel");
        _militaryPanel = GetNodeOrNull<Panel>("MilitaryPanel");

        var closeCabinet = GetNodeOrNull<Button>("CabinetPanel/VBox/Header/CloseButton");
        if (closeCabinet != null) closeCabinet.Pressed += () => ShowPanel(null);

        var closeTasks = GetNodeOrNull<Button>("TasksPanel/VBox/Header/CloseButton");
        if (closeTasks != null) closeTasks.Pressed += () => ShowPanel(null);

        var closeEconomy = GetNodeOrNull<Button>("EconomyPanel/VBox/Header/CloseButton");
        if (closeEconomy != null) closeEconomy.Pressed += () => ShowPanel(null);

        var closeDiplomacy = GetNodeOrNull<Button>("DiplomacyPanel/VBox/Header/CloseButton");
        if (closeDiplomacy != null) closeDiplomacy.Pressed += () => ShowPanel(null);

        var closeMilitary = GetNodeOrNull<Button>("MilitaryPanel/VBox/Header/CloseButton");
        if (closeMilitary != null) closeMilitary.Pressed += () => ShowPanel(null);

        _diplomacySummaryLabel = GetNodeOrNull<Label>("DiplomacyPanel/VBox/DiplomacySummaryLabel");
        _nationsList = GetNodeOrNull<VBoxContainer>("DiplomacyPanel/VBox/Scroll/NationsList");

        _forceStrengthLabel = GetNodeOrNull<Label>("MilitaryPanel/VBox/MilitarySummaryHBox/ForceStrengthLabel");
        _drillsStatusLabel = GetNodeOrNull<Label>("MilitaryPanel/VBox/MilitarySummaryHBox/DrillsStatusLabel");
        _drillsBtn = GetNodeOrNull<Button>("MilitaryPanel/VBox/MilitarySummaryHBox/DrillsBtn");
        if (_drillsBtn != null)
        {
            _drillsBtn.Pressed += () =>
            {
                var country = _session.GetPlayerCountry();
                if (country != null)
                {
                    _engine.Dispatch(new ConductArmyDrillsCommand(country.Id));
                    RefreshMilitaryView();
                    RefreshHud();
                }
            };
        }

        _counterTerrorBtn = GetNodeOrNull<Button>("MilitaryPanel/VBox/SecurityActionsHBox/CounterTerrorBtn");
        if (_counterTerrorBtn != null)
        {
            _counterTerrorBtn.Pressed += () =>
            {
                var country = _session.GetPlayerCountry();
                if (country != null)
                {
                    _engine.Dispatch(new LaunchCounterTerrorismCommand(country.Id));
                    RefreshMilitaryView();
                    RefreshHud();
                }
            };
        }

        _protectionLevelBtn = GetNodeOrNull<Button>("MilitaryPanel/VBox/SecurityActionsHBox/ProtectionLevelBtn");
        if (_protectionLevelBtn != null)
        {
            _protectionLevelBtn.Pressed += () =>
            {
                var country = _session.GetPlayerCountry();
                if (country != null)
                {
                    var next = country.PresidentialProtectionLevel switch
                    {
                        FundingLevel.Low => FundingLevel.Medium,
                        FundingLevel.Medium => FundingLevel.High,
                        FundingLevel.High => FundingLevel.Maximum,
                        FundingLevel.Maximum => FundingLevel.Low,
                        _ => FundingLevel.Medium
                    };
                    _engine.Dispatch(new FundPresidentialProtectionCommand(country.Id, next));
                    RefreshMilitaryView();
                }
            };
        }

        _unitsList = GetNodeOrNull<VBoxContainer>("MilitaryPanel/VBox/Scroll/UnitsList");

        _ideologyLabel = GetNodeOrNull<Label>("CabinetPanel/VBox/IdeologyHBox/IdeologyLabel");
        _changeIdeologyBtn = GetNodeOrNull<Button>("CabinetPanel/VBox/IdeologyHBox/ChangeIdeologyBtn");
        if (_changeIdeologyBtn != null)
        {
            _changeIdeologyBtn.Pressed += CycleIdeology;
        }

        _ministryList = GetNodeOrNull<VBoxContainer>("CabinetPanel/VBox/Scroll/MinistryList");
        _tasksSummaryLabel = GetNodeOrNull<Label>("TasksPanel/VBox/SummaryLabel");
        _tasksList = GetNodeOrNull<VBoxContainer>("TasksPanel/VBox/Scroll/TasksList");

        _eventTitle = GetNodeOrNull<Label>("EmergencyDialog/VBox/EventTitle");
        _eventDescription = GetNodeOrNull<Label>("EmergencyDialog/VBox/EventDescription");
        _choicesContainer = GetNodeOrNull<VBoxContainer>("EmergencyDialog/VBox/ChoicesContainer");

        // Economy Panel controls
        _gdpLabel = GetNodeOrNull<Label>("EconomyPanel/VBox/MacroSummaryHBox/GdpLabel");
        _creditRatingLabel = GetNodeOrNull<Label>("EconomyPanel/VBox/MacroSummaryHBox/CreditRatingLabel");
        _debtLabel = GetNodeOrNull<Label>("EconomyPanel/VBox/MacroSummaryHBox/DebtLabel");
        _borrowBtn = GetNodeOrNull<Button>("EconomyPanel/VBox/MacroSummaryHBox/BorrowBtn");
        _repayBtn = GetNodeOrNull<Button>("EconomyPanel/VBox/MacroSummaryHBox/RepayBtn");

        if (_borrowBtn != null)
        {
            _borrowBtn.Pressed += () =>
            {
                var country = _session.GetPlayerCountry();
                if (country != null)
                {
                    _engine.Dispatch(new TakeLoanCommand(country.Id, 5_000_000_000m));
                    RefreshEconomyView();
                    RefreshHud();
                }
            };
        }

        if (_repayBtn != null)
        {
            _repayBtn.Pressed += () =>
            {
                var country = _session.GetPlayerCountry();
                if (country != null)
                {
                    _engine.Dispatch(new RepayLoanCommand(country.Id, 5_000_000_000m));
                    RefreshEconomyView();
                    RefreshHud();
                }
            };
        }

        _ledgerSummaryLabel = GetNodeOrNull<Label>("EconomyPanel/VBox/Scroll/EconomyContent/LedgerSummaryLabel");
        _taxesContainer = GetNodeOrNull<VBoxContainer>("EconomyPanel/VBox/Scroll/EconomyContent/TaxesContainer");
        _facilitiesContainer = GetNodeOrNull<VBoxContainer>("EconomyPanel/VBox/Scroll/EconomyContent/FacilitiesContainer");
        _policiesContainer = GetNodeOrNull<VBoxContainer>("EconomyPanel/VBox/Scroll/EconomyContent/PoliciesContainer");

        _nav.ResetTo(ScreenRoute.WorldMap);
        RefreshHud();
    }

    public override void _Process(double delta)
    {
        _engine.Tick(delta);
        RefreshHud();
        CheckEmergencyPrompt();
    }

    private void ShowPanel(Panel? targetPanel)
    {
        if (_cabinetPanel != null) _cabinetPanel.Visible = (_cabinetPanel == targetPanel);
        if (_tasksPanel != null) _tasksPanel.Visible = (_tasksPanel == targetPanel);
        if (_economyPanel != null) _economyPanel.Visible = (_economyPanel == targetPanel);
        if (_diplomacyPanel != null) _diplomacyPanel.Visible = (_diplomacyPanel == targetPanel);
        if (_militaryPanel != null) _militaryPanel.Visible = (_militaryPanel == targetPanel);

        if (targetPanel == _cabinetPanel)
        {
            _nav.NavigateTo(ScreenRoute.GovernmentCabinet);
            RefreshCabinetView();
        }
        else if (targetPanel == _tasksPanel)
        {
            _nav.NavigateTo(ScreenRoute.TasksAndObjectives);
            RefreshTasksView();
        }
        else if (targetPanel == _economyPanel)
        {
            _nav.NavigateTo(ScreenRoute.EconomyAndBudget);
            RefreshEconomyView();
        }
        else if (targetPanel == _diplomacyPanel)
        {
            _nav.NavigateTo(ScreenRoute.DiplomacyAndTreaties);
            RefreshDiplomacyView();
        }
        else if (targetPanel == _militaryPanel)
        {
            _nav.NavigateTo(ScreenRoute.MilitaryAndDefense);
            RefreshMilitaryView();
        }
        else
        {
            _nav.NavigateTo(ScreenRoute.WorldMap);
        }
    }


    private void RefreshCabinetView()
    {
        var country = _session.GetPlayerCountry();
        if (country == null) return;

        var govVm = GovernmentViewModel.FromCountry(country);
        if (_ideologyLabel != null)
        {
            _ideologyLabel.Text = $"State Ideology: {govVm.IdeologyDisplayName} ({govVm.IdeologyDescription})";
        }

        if (_changeIdeologyBtn != null)
        {
            _changeIdeologyBtn.Disabled = !govVm.CanChangeIdeology;
            _changeIdeologyBtn.Text = govVm.CanChangeIdeology
                ? "Adopt Democratic Reforms ($1.0B)"
                : $"Reforms on Cooldown ({govVm.IdeologyCooldownMonths} mo)";
        }

        if (_ministryList == null) return;

        // Clear existing rows
        foreach (Node child in _ministryList.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var m in govVm.Ministries)
        {
            var row = new HBoxContainer();
            row.CustomMinimumSize = new Vector2(0, 36);

            var nameLabel = new Label
            {
                Text = $"{m.DisplayName}: {m.MinisterName} ({m.CompetencePercent})",
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            row.AddChild(nameLabel);

            var fundingLabel = new Label
            {
                Text = $"Funding: {m.FundingLevelName} ({m.OperationalCostFormatted}) [Eff: {m.EfficiencyPercent}]",
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            row.AddChild(fundingLabel);

            var cycleFundingBtn = new Button
            {
                Text = "+ Funding",
                CustomMinimumSize = new Vector2(90, 0)
            };
            var ministryType = m.Type;
            var currentFunding = m.Funding;
            cycleFundingBtn.Pressed += () =>
            {
                var nextFunding = currentFunding switch
                {
                    FundingLevel.Zero => FundingLevel.Low,
                    FundingLevel.Low => FundingLevel.Medium,
                    FundingLevel.Medium => FundingLevel.High,
                    FundingLevel.High => FundingLevel.Maximum,
                    _ => FundingLevel.Zero
                };
                _engine.Dispatch(new AdjustMinistryFundingCommand(country.Id, ministryType, nextFunding));
                RefreshCabinetView();
            };
            row.AddChild(cycleFundingBtn);

            if (m.IsVacant)
            {
                var appointBtn = new Button
                {
                    Text = "Appoint ($20M)",
                    CustomMinimumSize = new Vector2(110, 0)
                };
                appointBtn.Pressed += () =>
                {
                    var candidates = MinisterRosterService.GetCandidates(ministryType);
                    if (candidates.Count > 0)
                    {
                        var cand = candidates[0];
                        var minister = new Minister(cand.Id, cand.Name, cand.Competence, cand.MonthlySalary);
                        _engine.Dispatch(new AppointMinisterCommand(country.Id, ministryType, minister, cand.HiringFee));
                        RefreshCabinetView();
                    }
                };
                row.AddChild(appointBtn);
            }
            else
            {
                var dismissBtn = new Button
                {
                    Text = "Dismiss",
                    CustomMinimumSize = new Vector2(80, 0)
                };
                dismissBtn.Pressed += () =>
                {
                    _engine.Dispatch(new DismissMinisterCommand(country.Id, ministryType));
                    RefreshCabinetView();
                };
                row.AddChild(dismissBtn);
            }

            _ministryList.AddChild(row);
        }
    }

    private void CycleIdeology()
    {
        var country = _session.GetPlayerCountry();
        if (country == null) return;

        var nextIdeology = country.Ideology switch
        {
            IdeologyType.ConstitutionalDemocracy => IdeologyType.DemocraticSocialism,
            IdeologyType.DemocraticSocialism => IdeologyType.AuthoritarianRepublic,
            IdeologyType.AuthoritarianRepublic => IdeologyType.NationalistFederation,
            IdeologyType.NationalistFederation => IdeologyType.Technocracy,
            _ => IdeologyType.ConstitutionalDemocracy
        };

        _engine.Dispatch(new ChangeIdeologyCommand(country.Id, nextIdeology));
        RefreshCabinetView();
    }

    private void RefreshTasksView()
    {
        var tasksVm = TasksViewModel.FromSession(_session);
        if (_tasksSummaryLabel != null)
        {
            _tasksSummaryLabel.Text = $"Objectives: {tasksVm.OverallProgressText}";
        }

        if (_tasksList == null) return;

        foreach (Node child in _tasksList.GetChildren())
        {
            child.QueueFree();
        }

        var allTasks = tasksVm.PrincipalTasks.Concat(tasksVm.SecondaryTasks);
        foreach (var task in allTasks)
        {
            var taskBox = new VBoxContainer();

            var header = new HBoxContainer();
            var titleLabel = new Label
            {
                Text = $"{(task.IsPrincipal ? "[PRINCIPAL] " : "[SECONDARY] ")}{task.Title} ({task.StatusText})",
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            header.AddChild(titleLabel);

            if (task.CanClaimReward)
            {
                var claimBtn = new Button
                {
                    Text = $"Claim Reward: {task.RewardSummary}",
                    CustomMinimumSize = new Vector2(250, 0)
                };
                var taskId = task.Id;
                claimBtn.Pressed += () =>
                {
                    _engine.Dispatch(new ClaimTaskRewardCommand(_session.PlayerCountryId, taskId));
                    RefreshTasksView();
                    RefreshHud();
                };
                header.AddChild(claimBtn);
            }
            taskBox.AddChild(header);

            var descLabel = new Label
            {
                Text = $"{task.Description} • Progress: {task.ProgressText} • Reward: {task.RewardSummary}"
            };
            descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            taskBox.AddChild(descLabel);

            foreach (var st in task.SubTasks)
            {
                var stLabel = new Label
                {
                    Text = $"   - {st.Description}: {st.ProgressText} {(st.IsCompleted ? "✓" : "")}"
                };
                taskBox.AddChild(stLabel);
            }

            _tasksList.AddChild(taskBox);
            _tasksList.AddChild(new HSeparator());
        }
    }

    private void RefreshEconomyView()
    {
        var country = _session.GetPlayerCountry();
        if (country == null) return;

        var vm = EconomyViewModel.FromCountry(country, _engine.PolicyRegistry);

        if (_gdpLabel != null)
        {
            _gdpLabel.Text = $"Monthly GDP: ${vm.MonthlyGdp / 1_000_000_000m:F1}B (Annual: ${vm.AnnualGdp / 1_000_000_000m:F1}B)";
        }
        if (_creditRatingLabel != null)
        {
            _creditRatingLabel.Text = $"Credit Rating: {vm.CreditRating} ({vm.MonthlyInterestRate * 100:F2}%/mo)";
        }
        if (_debtLabel != null)
        {
            _debtLabel.Text = $"Debt: ${vm.Debt / 1_000_000_000m:F1}B (Ceiling: ${vm.DebtCeiling / 1_000_000_000m:F1}B | Ratio: {vm.DebtToGdpRatio * 100:F1}%)";
        }
        if (_ledgerSummaryLabel != null)
        {
            _ledgerSummaryLabel.Text = $"Projected Revenue: ${vm.ProjectedGrossRevenue / 1_000_000_000m:F2}B | Expenses: ${vm.ProjectedGrossExpenses / 1_000_000_000m:F2}B | Net: {(vm.ProjectedNetBalance >= 0 ? "+" : "")}${vm.ProjectedNetBalance / 1_000_000_000m:F2}B/mo (Efficiency: {vm.ProjectedTaxEfficiency * 100:F1}%)";
        }

        // 1. Taxes Container
        if (_taxesContainer != null)
        {
            foreach (Node child in _taxesContainer.GetChildren())
            {
                child.QueueFree();
            }

            var taxes = new (string Name, TaxType Type, double Rate, decimal ProjectedRev)[]
            {
                ("Income Tax", TaxType.IncomeTax, vm.IncomeTaxRate, vm.ProjectedIncomeTaxRevenue),
                ("Corporate Tax", TaxType.CorporateTax, vm.CorporateTaxRate, vm.ProjectedCorporateTaxRevenue),
                ("VAT / Sales Tax", TaxType.Vat, vm.VatRate, vm.ProjectedVatRevenue)
            };

            foreach (var tax in taxes)
            {
                var row = new HBoxContainer { CustomMinimumSize = new Vector2(0, 32) };
                var nameLabel = new Label
                {
                    Text = $"{tax.Name}: {tax.Rate * 100:F1}% (Projected: ${tax.ProjectedRev / 1_000_000_000m:F2}B/mo)",
                    SizeFlagsHorizontal = SizeFlags.ExpandFill
                };
                row.AddChild(nameLabel);

                var minusBtn = new Button { Text = "-1%", CustomMinimumSize = new Vector2(60, 0) };
                var plusBtn = new Button { Text = "+1%", CustomMinimumSize = new Vector2(60, 0) };

                var taxType = tax.Type;
                var currentRate = tax.Rate;

                minusBtn.Pressed += () =>
                {
                    double nextRate = Math.Clamp(currentRate - 0.01, 0.0, 1.0);
                    _engine.Dispatch(new AdjustTaxRateCommand(country.Id, taxType, nextRate));
                    RefreshEconomyView();
                    RefreshHud();
                };

                plusBtn.Pressed += () =>
                {
                    double nextRate = Math.Clamp(currentRate + 0.01, 0.0, 1.0);
                    _engine.Dispatch(new AdjustTaxRateCommand(country.Id, taxType, nextRate));
                    RefreshEconomyView();
                    RefreshHud();
                };

                row.AddChild(minusBtn);
                row.AddChild(plusBtn);
                _taxesContainer.AddChild(row);
            }
        }

        // 2. Facilities Container
        if (_facilitiesContainer != null)
        {
            foreach (Node child in _facilitiesContainer.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var f in vm.Facilities)
            {
                var row = new HBoxContainer { CustomMinimumSize = new Vector2(0, 36) };
                var infoLabel = new Label
                {
                    Text = $"{f.FacilityType}: {f.CurrentCount} built ({f.PrimaryProduction}) • Cost: ${f.MoneyCost / 1_000_000m:F0}M, {f.IronCost:F0} Iron",
                    SizeFlagsHorizontal = SizeFlags.ExpandFill
                };
                row.AddChild(infoLabel);

                var buildBtn = new Button
                {
                    Text = "+ Build 1x",
                    CustomMinimumSize = new Vector2(100, 0),
                    Disabled = !f.CanAfford
                };

                var facType = f.FacilityType;
                buildBtn.Pressed += () =>
                {
                    _engine.Dispatch(new ConstructFacilityCommand(country.Id, facType, 1));
                    RefreshEconomyView();
                    RefreshHud();
                };

                row.AddChild(buildBtn);
                _facilitiesContainer.AddChild(row);
            }
        }

        // 3. Policies Container
        if (_policiesContainer != null)
        {
            foreach (Node child in _policiesContainer.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var p in vm.Policies)
            {
                var row = new HBoxContainer { CustomMinimumSize = new Vector2(0, 40) };
                var pLabel = new Label
                {
                    Text = $"[{p.Category.ToUpperInvariant()}] {p.Name} {(p.IsActive ? "★ ACTIVE" : "")}\n  Upfront: ${p.UpfrontCost / 1_000_000m:F0}M | Upkeep: ${p.MonthlyUpkeep / 1_000_000m:F0}M/mo | {p.EffectSummary}",
                    SizeFlagsHorizontal = SizeFlags.ExpandFill
                };
                row.AddChild(pLabel);

                var polId = p.Id;
                if (p.IsActive)
                {
                    var revokeBtn = new Button
                    {
                        Text = "Revoke",
                        CustomMinimumSize = new Vector2(90, 0)
                    };
                    revokeBtn.Pressed += () =>
                    {
                        _engine.Dispatch(new RevokeEconomicPolicyCommand(country.Id, polId));
                        RefreshEconomyView();
                        RefreshHud();
                    };
                    row.AddChild(revokeBtn);
                }
                else
                {
                    var enactBtn = new Button
                    {
                        Text = "Enact",
                        CustomMinimumSize = new Vector2(90, 0),
                        Disabled = !p.CanAfford
                    };
                    enactBtn.Pressed += () =>
                    {
                        _engine.Dispatch(new EnactEconomicPolicyCommand(country.Id, polId));
                        RefreshEconomyView();
                        RefreshHud();
                    };
                    row.AddChild(enactBtn);
                }

                _policiesContainer.AddChild(row);
            }
        }
    }

    private void CheckEmergencyPrompt()
    {
        if (_emergencyDialog == null) return;

        var prompt = _session.PendingEvent;
        if (prompt == null)
        {
            _emergencyDialog.Visible = false;
            return;
        }

        if (!_emergencyDialog.Visible)
        {
            _emergencyDialog.Visible = true;
            if (_eventTitle != null) _eventTitle.Text = prompt.Title;
            if (_eventDescription != null) _eventDescription.Text = prompt.Description;

            if (_choicesContainer != null)
            {
                foreach (Node child in _choicesContainer.GetChildren())
                {
                    child.QueueFree();
                }

                foreach (var choice in prompt.Choices)
                {
                    var btn = new Button
                    {
                        Text = $"{choice.Title} (${choice.TreasuryCost / 1_000_000m:F0}M) - {choice.Description}"
                    };
                    var eventId = prompt.EventId;
                    var choiceId = choice.Id;
                    btn.Pressed += () =>
                    {
                        _engine.Dispatch(new ResolveEventChoiceCommand(_session.PlayerCountryId, eventId, choiceId));
                        _emergencyDialog.Visible = false;
                        RefreshHud();
                    };
                    _choicesContainer.AddChild(btn);
                }
            }
        }
    }

    private void RefreshDiplomacyView()
    {
        var vm = DiplomacyViewModel.FromSession(_session);
        if (_diplomacySummaryLabel != null)
        {
            _diplomacySummaryLabel.Text = $"Foreign Nations: {vm.TotalForeignNations} | Active Non-Aggression Pacts: {vm.ActiveNapCount} | Trade Deals: {vm.TotalActiveTradeContracts} | At War: {vm.AtWarCount}";
        }

        if (_nationsList == null) return;
        foreach (Node child in _nationsList.GetChildren())
        {
            child.QueueFree();
        }

        var playerCountry = _session.GetPlayerCountry();
        if (playerCountry == null) return;

        foreach (var nation in vm.ForeignNations)
        {
            var card = new PanelContainer();
            var vbox = new VBoxContainer();

            var topRow = new HBoxContainer();
            topRow.AddThemeConstantOverride("separation", 12);

            var nameLabel = new Label
            {
                Text = $"{nation.CountryName} [{nation.CountryId}]",
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            topRow.AddChild(nameLabel);

            var statusBadge = new Label
            {
                Text = $"Stance: {nation.StatusText} ({nation.RelationsText})"
            };
            topRow.AddChild(statusBadge);

            var napBadge = new Label
            {
                Text = nation.NapExpiryText
            };
            topRow.AddChild(napBadge);

            vbox.AddChild(topRow);

            var actionRow = new HBoxContainer();
            actionRow.AddThemeConstantOverride("separation", 8);


            var aidBtn = new Button
            {
                Text = "+ Aid $250M (+5 Rel)",
                Disabled = !nation.CanSendAid
            };
            var targetId = nation.CountryId;
            aidBtn.Pressed += () =>
            {
                _engine.Dispatch(new SendDiplomaticGiftCommand(playerCountry.Id, targetId, DiplomaticGiftTier.Small));
                RefreshDiplomacyView();
                RefreshHud();
            };
            actionRow.AddChild(aidBtn);

            var napBtn = new Button
            {
                Text = "Sign NAP 24mo ($50M)",
                Disabled = !nation.CanSignNap
            };
            napBtn.Pressed += () =>
            {
                _engine.Dispatch(new SignNonAggressionPactCommand(playerCountry.Id, targetId, 24));
                RefreshDiplomacyView();
                RefreshHud();
            };
            actionRow.AddChild(napBtn);

            var tradeBtn = new Button
            {
                Text = "+ Export 5k Food ($200M/mo)",
                Disabled = nation.IsAtWar || nation.ActiveTradeContractCount >= 2
            };
            tradeBtn.Pressed += () =>
            {
                _engine.Dispatch(new SignTradeContractCommand(
                    exporterCountryId: playerCountry.Id,
                    importerCountryId: targetId,
                    resourceType: "Food",
                    monthlyQuantity: 5_000.0,
                    monthlyPayment: 200_000_000m,
                    durationMonths: 24
                ));
                RefreshDiplomacyView();
                RefreshHud();
            };
            actionRow.AddChild(tradeBtn);

            if (nation.IsAtWar)
            {
                var peaceBtn = new Button
                {
                    Text = "Sign Peace Treaty"
                };
                peaceBtn.Pressed += () =>
                {
                    _engine.Dispatch(new SignPeaceTreatyCommand(playerCountry.Id, targetId));
                    RefreshDiplomacyView();
                    RefreshHud();
                };
                actionRow.AddChild(peaceBtn);

                var attackBtn = new Button
                {
                    Text = "Launch Offensive Strike"
                };
                attackBtn.Pressed += () =>
                {
                    _engine.Dispatch(new ResolveCombatCommand(playerCountry.Id, targetId));
                    RefreshDiplomacyView();
                    RefreshHud();
                };
                actionRow.AddChild(attackBtn);
            }
            else
            {
                var warBtn = new Button
                {
                    Text = "Declare War",
                    Disabled = !nation.CanDeclareWar
                };
                warBtn.Pressed += () =>
                {
                    _engine.Dispatch(new DeclareWarCommand(playerCountry.Id, targetId));
                    RefreshDiplomacyView();
                    RefreshHud();
                };
                actionRow.AddChild(warBtn);
            }

            var spyBtn = new Button
            {
                Text = "Spy Intel ($150M)"
            };
            spyBtn.Pressed += () =>
            {
                _engine.Dispatch(new DispatchCovertOperationCommand(playerCountry.Id, targetId, CovertOperationType.GatherIntelligence));
                RefreshDiplomacyView();
                RefreshHud();
            };
            actionRow.AddChild(spyBtn);

            vbox.AddChild(actionRow);
            card.AddChild(vbox);
            _nationsList.AddChild(card);
        }
    }

    private void RefreshMilitaryView()
    {
        var vm = MilitaryViewModel.FromSession(_session);
        if (_forceStrengthLabel != null)
        {
            _forceStrengthLabel.Text = $"Force: {vm.TotalCombatPower:N0} ATK / {vm.TotalDefensePower:N0} DEF | Units: {vm.TotalUnits:N0} (Land: {vm.LandUnitsCount}, Air: {vm.AirUnitsCount}, Sea: {vm.NavalUnitsCount}) | Upkeep: ${vm.TotalMonthlyUpkeep / 1_000_000m:F0}M/mo";
        }

        if (_drillsStatusLabel != null)
        {
            _drillsStatusLabel.Text = vm.DrillReadinessExpiryText;
        }

        var playerCountry = _session.GetPlayerCountry();
        if (playerCountry != null && _protectionLevelBtn != null)
        {
            _protectionLevelBtn.Text = $"Presidential Protection: {playerCountry.PresidentialProtectionLevel}";
        }

        if (_unitsList == null) return;
        foreach (Node child in _unitsList.GetChildren())
        {
            child.QueueFree();
        }

        if (playerCountry == null) return;

        foreach (var unit in vm.Units)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 12);


            var nameLabel = new Label
            {
                Text = $"{unit.UnitName} [{unit.Branch}] - In Inventory: {unit.ActiveCount}x (Power: {unit.CombatPower} Atk / {unit.DefensePower} Def, Upkeep: ${unit.MonthlyUpkeepTreasury / 1_000_000m:F1}M/mo)",
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            row.AddChild(nameLabel);

            var costLabel = new Label
            {
                Text = $"Cost: ${unit.CostTreasury / 1_000_000m:F0}M + {unit.CostIron:F0} Fe + {unit.CostOil:F0} Oil"
            };
            row.AddChild(costLabel);

            var uId = unit.UnitId;

            var btn1 = new Button
            {
                Text = "+1",
                Disabled = !unit.CanAfford1
            };
            btn1.Pressed += () =>
            {
                _engine.Dispatch(new RecruitMilitaryUnitsCommand(playerCountry.Id, uId, 1));
                RefreshMilitaryView();
                RefreshHud();
            };
            row.AddChild(btn1);

            var btn5 = new Button
            {
                Text = "+5",
                Disabled = !unit.CanAfford5
            };
            btn5.Pressed += () =>
            {
                _engine.Dispatch(new RecruitMilitaryUnitsCommand(playerCountry.Id, uId, 5));
                RefreshMilitaryView();
                RefreshHud();
            };
            row.AddChild(btn5);

            var btn10 = new Button
            {
                Text = "+10",
                Disabled = !unit.CanAfford10
            };
            btn10.Pressed += () =>
            {
                _engine.Dispatch(new RecruitMilitaryUnitsCommand(playerCountry.Id, uId, 10));
                RefreshMilitaryView();
                RefreshHud();
            };
            row.AddChild(btn10);

            _unitsList.AddChild(row);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Space:
                    TogglePause();
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.Key1:
                    SetSpeed(ClockSpeed.Normal);
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.Key2:
                    SetSpeed(ClockSpeed.Fast);
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.Key3:
                    SetSpeed(ClockSpeed.Ultra);
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.F1:
                    ShowPanel(null);
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.F2:
                    ShowPanel(_cabinetPanel);
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.F3:
                    ShowPanel(_tasksPanel);
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.F4:
                    ShowPanel(_economyPanel);
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.F6:
                    ShowPanel(_diplomacyPanel);
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.F7:
                    ShowPanel(_militaryPanel);
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.F5:
                    _saveManager.QuickSave(_session);
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.F9:
                    var loadResult = _saveManager.QuickLoad();
                    if (loadResult.IsSuccess && loadResult.Session != null)
                    {
                        _session = loadResult.Session;
                        _engine = new SimulationEngine(_session);
                        _engine.OnMonthRollover += date => _saveManager.AutoSave(_session);
                        RefreshHud();
                    }
                    GetViewport().SetInputAsHandled();
                    break;
                case Key.Escape:
                    HandleEscape();
                    GetViewport().SetInputAsHandled();
                    break;
            }
        }
    }

    private void SetSpeed(ClockSpeed speed)
    {
        _session.Clock.Speed = speed;
        RefreshHud();
    }

    private void TogglePause()
    {
        if (_session.Clock.Speed == ClockSpeed.Paused)
        {
            _session.Clock.Speed = ClockSpeed.Normal;
        }
        else
        {
            _session.Clock.Speed = ClockSpeed.Paused;
        }
        RefreshHud();
    }

    private void HandleEscape()
    {
        if (_cabinetPanel != null && _cabinetPanel.Visible)
        {
            ShowPanel(null);
            return;
        }

        if (_tasksPanel != null && _tasksPanel.Visible)
        {
            ShowPanel(null);
            return;
        }

        if (_economyPanel != null && _economyPanel.Visible)
        {
            ShowPanel(null);
            return;
        }

        if (_diplomacyPanel != null && _diplomacyPanel.Visible)
        {
            ShowPanel(null);
            return;
        }

        if (_militaryPanel != null && _militaryPanel.Visible)
        {
            ShowPanel(null);
            return;
        }

        if (_nav.CanNavigateBack)
        {
            _nav.NavigateBack();
        }
        else
        {
            GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
        }
    }


    private void RefreshHud()
    {
        var vm = HudViewModel.FromSession(_session);

        if (_dateLabel != null) _dateLabel.Text = vm.DateText;
        if (_treasuryLabel != null) _treasuryLabel.Text = $"TREASURY: {vm.TreasuryText}";
        if (_popLabel != null) _popLabel.Text = $"POP: {vm.PopulationText}";
        if (_approvalLabel != null) _approvalLabel.Text = $"APPROVAL: {vm.ApprovalText}";
        if (_livingLabel != null) _livingLabel.Text = $"LIVING: {vm.LivingStandardText}";
        if (_speedLabel != null) _speedLabel.Text = $"[{vm.SpeedText}]";

        var playerCountry = _session.GetPlayerCountry();
        var mapTitle = GetNodeOrNull<Label>("CentralMapPlaceholder/VBox/MapTitle");
        var mapInfo = GetNodeOrNull<Label>("CentralMapPlaceholder/VBox/MapInfo");
        if (mapTitle != null && playerCountry != null)
        {
            mapTitle.Text = $"WORLD MAP • {playerCountry.Name.ToUpperInvariant()} [{playerCountry.Id}]";
        }
        if (mapInfo != null && playerCountry != null)
        {
            mapInfo.Text = $"Sovereign Administration Active • Scenario: {_session.ScenarioId} • Stability: {playerCountry.Stability:F0}%";
        }
    }
}
