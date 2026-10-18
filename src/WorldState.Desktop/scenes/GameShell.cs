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

    private Panel? _cabinetPanel;
    private Panel? _tasksPanel;
    private Panel? _emergencyDialog;

    private Label? _ideologyLabel;
    private Button? _changeIdeologyBtn;
    private VBoxContainer? _ministryList;
    private Label? _tasksSummaryLabel;
    private VBoxContainer? _tasksList;

    private Label? _eventTitle;
    private Label? _eventDescription;
    private VBoxContainer? _choicesContainer;

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

        if (_btnOverview != null) _btnOverview.Pressed += () => ShowPanel(null);
        if (_btnCabinet != null) _btnCabinet.Pressed += () => ShowPanel(_cabinetPanel);
        if (_btnTasks != null) _btnTasks.Pressed += () => ShowPanel(_tasksPanel);

        // Panels
        _cabinetPanel = GetNodeOrNull<Panel>("CabinetPanel");
        _tasksPanel = GetNodeOrNull<Panel>("TasksPanel");
        _emergencyDialog = GetNodeOrNull<Panel>("EmergencyDialog");

        var closeCabinet = GetNodeOrNull<Button>("CabinetPanel/VBox/Header/CloseButton");
        if (closeCabinet != null) closeCabinet.Pressed += () => ShowPanel(null);

        var closeTasks = GetNodeOrNull<Button>("TasksPanel/VBox/Header/CloseButton");
        if (closeTasks != null) closeTasks.Pressed += () => ShowPanel(null);

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
