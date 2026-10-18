using Godot;
using WorldState.Domain.Settings;

namespace WorldState.Desktop.Scenes;

public partial class SettingsDialog : Control
{
    private readonly SettingsService _settingsService = new();
    private UserSettings _currentSettings = UserSettings.CreateDefault();

    private HSlider? _masterSlider;
    private HSlider? _musicSlider;
    private HSlider? _sfxSlider;
    private Button? _closeButton;

    public override void _Ready()
    {
        _masterSlider = GetNodeOrNull<HSlider>("PanelContainer/MarginContainer/VBoxContainer/MasterVolumeSlider");
        _musicSlider = GetNodeOrNull<HSlider>("PanelContainer/MarginContainer/VBoxContainer/MusicVolumeSlider");
        _sfxSlider = GetNodeOrNull<HSlider>("PanelContainer/MarginContainer/VBoxContainer/SfxVolumeSlider");
        _closeButton = GetNodeOrNull<Button>("PanelContainer/MarginContainer/VBoxContainer/CloseButton");

        if (_masterSlider != null)
        {
            _masterSlider.Value = _currentSettings.MasterVolume * 100.0;
        }

        if (_musicSlider != null)
        {
            _musicSlider.Value = _currentSettings.MusicVolume * 100.0;
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.Value = _currentSettings.SfxVolume * 100.0;
        }

        if (_closeButton != null)
        {
            _closeButton.Pressed += OnClosePressed;
        }
    }

    private void OnClosePressed()
    {
        QueueFree();
    }
}
