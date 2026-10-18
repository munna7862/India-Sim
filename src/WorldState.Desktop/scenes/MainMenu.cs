using Godot;
using WorldState.Domain.Services;

namespace WorldState.Desktop.Scenes;

public partial class MainMenu : Control
{
    private readonly SaveManager _saveManager = new();
    private Button? _newGameBtn;
    private Button? _loadGameBtn;
    private Button? _settingsBtn;
    private Button? _quitBtn;

    public override void _Ready()
    {
        _newGameBtn = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/NewGameButton");
        _loadGameBtn = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/LoadGameButton");
        _settingsBtn = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/SettingsButton");
        _quitBtn = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/QuitButton");

        if (_newGameBtn != null)
        {
            _newGameBtn.Pressed += OnNewGamePressed;
        }

        if (_loadGameBtn != null)
        {
            var slots = _saveManager.ListSlots();
            if (slots.Count > 0 && !slots[0].IsCorrupted)
            {
                _loadGameBtn.Disabled = false;
                _loadGameBtn.Text = $"CONTINUE ({slots[0].CountryName.ToUpperInvariant()} - {slots[0].InGameDate:dd MMM yyyy})";
                _loadGameBtn.Pressed += OnLoadGamePressed;
            }
            else
            {
                _loadGameBtn.Disabled = true;
                _loadGameBtn.Text = "LOAD GAME (NO SAVES)";
            }
        }

        if (_settingsBtn != null)
        {
            _settingsBtn.Pressed += OnSettingsPressed;
        }

        if (_quitBtn != null)
        {
            _quitBtn.Pressed += OnQuitPressed;
        }
    }

    private void OnNewGamePressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/CountrySelect.tscn");
    }

    private void OnLoadGamePressed()
    {
        var slots = _saveManager.ListSlots();
        if (slots.Count > 0)
        {
            var result = _saveManager.Load(slots[0].SlotId);
            if (result.IsSuccess && result.Session != null)
            {
                GameShell.ActiveSession = result.Session;
                GetTree().ChangeSceneToFile("res://scenes/GameShell.tscn");
            }
        }
    }

    private void OnSettingsPressed()
    {
        var settingsScene = GD.Load<PackedScene>("res://scenes/SettingsDialog.tscn");
        if (settingsScene != null)
        {
            var dialog = settingsScene.Instantiate<Control>();
            AddChild(dialog);
        }
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
