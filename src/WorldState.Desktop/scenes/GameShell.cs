using Godot;
using WorldState.Domain.Models;
using WorldState.Domain.Navigation;
using WorldState.Domain.ViewModels;

namespace WorldState.Desktop.Scenes;

public partial class GameShell : Control
{
    public static GameSession? ActiveSession { get; set; }
    private GameSession _session = ActiveSession ?? GameSession.CreateNewGame("IND");
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

    public override void _Ready()
    {
        if (ActiveSession != null)
        {
            _session = ActiveSession;
        }

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

        _nav.ResetTo(ScreenRoute.WorldMap);
        RefreshHud();
    }

    public override void _Process(double delta)
    {
        _session.Clock.Tick(delta);
        RefreshHud();
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
