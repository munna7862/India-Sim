using System;
using System.IO;
using Godot;
using WorldState.Domain.Map;
using WorldState.Domain.Settings;

namespace WorldState.Desktop.Scenes;

public partial class SettingsDialog : Control
{
    private readonly SettingsService _settingsService = new();
    private UserSettings _currentSettings = UserSettings.CreateDefault();

    private HSlider? _masterSlider;
    private HSlider? _musicSlider;
    private HSlider? _sfxSlider;
    private OptionButton? _displayModeOption;
    private OptionButton? _resolutionOption;
    private OptionButton? _scaleOption;
    private OptionButton? _paletteOption;
    private CheckBox? _autosaveCheckbox;
    private Button? _closeButton;

    public event Action<UserSettings>? OnSettingsApplied;

    public override void _Ready()
    {
        LoadSettingsFromFile();

        _masterSlider = GetNodeOrNull<HSlider>("PanelContainer/MarginContainer/VBoxContainer/MasterHBox/MasterVolumeSlider");
        _musicSlider = GetNodeOrNull<HSlider>("PanelContainer/MarginContainer/VBoxContainer/MusicHBox/MusicVolumeSlider");
        _sfxSlider = GetNodeOrNull<HSlider>("PanelContainer/MarginContainer/VBoxContainer/SfxHBox/SfxVolumeSlider");

        _displayModeOption = GetNodeOrNull<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/DisplayModeHBox/DisplayModeOption");
        _resolutionOption = GetNodeOrNull<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/ResolutionHBox/ResolutionOption");
        _scaleOption = GetNodeOrNull<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/ScaleHBox/ScaleOption");
        _paletteOption = GetNodeOrNull<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/PaletteHBox/PaletteOption");
        _autosaveCheckbox = GetNodeOrNull<CheckBox>("PanelContainer/MarginContainer/VBoxContainer/AutosaveHBox/AutosaveCheckbox");

        _closeButton = GetNodeOrNull<Button>("PanelContainer/MarginContainer/VBoxContainer/CloseButton");

        // Audio values
        if (_masterSlider != null) _masterSlider.Value = _currentSettings.MasterVolume * 100.0;
        if (_musicSlider != null) _musicSlider.Value = _currentSettings.MusicVolume * 100.0;
        if (_sfxSlider != null) _sfxSlider.Value = _currentSettings.SfxVolume * 100.0;

        // Display Mode
        if (_displayModeOption != null)
        {
            _displayModeOption.Clear();
            _displayModeOption.AddItem("Windowed", (int)WindowMode.Windowed);
            _displayModeOption.AddItem("Borderless Fullscreen", (int)WindowMode.BorderlessFullscreen);
            _displayModeOption.AddItem("Exclusive Fullscreen", (int)WindowMode.ExclusiveFullscreen);
            _displayModeOption.Select((int)_currentSettings.DisplayMode);
        }

        // Resolutions
        if (_resolutionOption != null)
        {
            _resolutionOption.Clear();
            for (int i = 0; i < UserSettings.ResolutionPresets.Count; i++)
            {
                var preset = UserSettings.ResolutionPresets[i];
                _resolutionOption.AddItem(preset.Label, i);
                if (preset.Width == _currentSettings.ResolutionWidth && preset.Height == _currentSettings.ResolutionHeight)
                {
                    _resolutionOption.Select(i);
                }
            }
        }

        // UI Scaling
        if (_scaleOption != null)
        {
            _scaleOption.Clear();
            _scaleOption.AddItem("100% (Standard)", 0);
            _scaleOption.AddItem("125% (Comfortable)", 1);
            _scaleOption.AddItem("150% (High-DPI)", 2);

            int selectedScaleIdx = _currentSettings.UiScale switch
            {
                >= 1.4 => 2,
                >= 1.2 => 1,
                _ => 0
            };
            _scaleOption.Select(selectedScaleIdx);
        }

        // Map Palettes
        if (_paletteOption != null)
        {
            _paletteOption.Clear();
            int idx = 0;
            int selIdx = 0;
            foreach (var p in MapThemeRegistry.GetAllPalettes())
            {
                _paletteOption.AddItem(p.DisplayName, idx);
                if (string.Equals(p.Name, _currentSettings.MapPalette, StringComparison.OrdinalIgnoreCase))
                {
                    selIdx = idx;
                }
                idx++;
            }
            _paletteOption.Select(selIdx);
        }

        if (_autosaveCheckbox != null)
        {
            _autosaveCheckbox.ButtonPressed = _currentSettings.AutosaveEnabled;
        }

        if (_closeButton != null)
        {
            _closeButton.Pressed += OnApplyAndClose;
        }
    }

    private void LoadSettingsFromFile()
    {
        string path = GetSettingsFilePath();
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                _currentSettings = _settingsService.DeserializeSettings(json);
            }
            catch
            {
                _currentSettings = UserSettings.CreateDefault();
            }
        }
    }

    private void OnApplyAndClose()
    {
        double master = _masterSlider != null ? _masterSlider.Value / 100.0 : _currentSettings.MasterVolume;
        double music = _musicSlider != null ? _musicSlider.Value / 100.0 : _currentSettings.MusicVolume;
        double sfx = _sfxSlider != null ? _sfxSlider.Value / 100.0 : _currentSettings.SfxVolume;

        var displayMode = _displayModeOption != null
            ? (WindowMode)_displayModeOption.GetSelectedId()
            : _currentSettings.DisplayMode;

        int resWidth = _currentSettings.ResolutionWidth;
        int resHeight = _currentSettings.ResolutionHeight;
        if (_resolutionOption != null && _resolutionOption.Selected >= 0 && _resolutionOption.Selected < UserSettings.ResolutionPresets.Count)
        {
            var p = UserSettings.ResolutionPresets[_resolutionOption.Selected];
            resWidth = p.Width;
            resHeight = p.Height;
        }

        double uiScale = _scaleOption?.Selected switch
        {
            1 => 1.25,
            2 => 1.5,
            _ => 1.0
        };

        string mapPalette = _paletteOption != null && _paletteOption.Selected >= 0
            ? _paletteOption.GetItemText(_paletteOption.Selected).Replace(" ", "")
            : _currentSettings.MapPalette;

        bool autosave = _autosaveCheckbox?.ButtonPressed ?? _currentSettings.AutosaveEnabled;

        var updated = _currentSettings with
        {
            MasterVolume = master,
            MusicVolume = music,
            SfxVolume = sfx,
            DisplayMode = displayMode,
            ResolutionWidth = resWidth,
            ResolutionHeight = resHeight,
            UiScale = uiScale,
            MapPalette = mapPalette,
            AutosaveEnabled = autosave
        };

        // Save to file
        try
        {
            string path = GetSettingsFilePath();
            string json = _settingsService.SerializeSettings(updated);
            File.WriteAllText(path, json);
        }
        catch { }

        // Apply window mode and resolution if supported in current runtime
        try
        {
            switch (displayMode)
            {
                case WindowMode.ExclusiveFullscreen:
                    DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                    break;
                case WindowMode.BorderlessFullscreen:
                    DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                    break;
                default:
                    DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                    DisplayServer.WindowSetSize(new Vector2I(resWidth, resHeight));
                    break;
            }
        }
        catch { }

        OnSettingsApplied?.Invoke(updated);
        QueueFree();
    }

    private static string GetSettingsFilePath()
    {
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "user_data");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return Path.Combine(dir, "settings.json");
    }
}
