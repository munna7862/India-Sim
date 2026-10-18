using Godot;

namespace WorldState.Desktop.Scenes;

public partial class MainMenu : Control
{
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
        GetTree().ChangeSceneToFile("res://scenes/GameShell.tscn");
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
