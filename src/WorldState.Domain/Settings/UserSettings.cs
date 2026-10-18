using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorldState.Domain.Settings;

public enum WindowMode
{
    Windowed,
    BorderlessFullscreen,
    ExclusiveFullscreen
}

public record UserSettings(
    double MasterVolume = 0.8,
    double MusicVolume = 0.7,
    double SfxVolume = 0.9,
    WindowMode DisplayMode = WindowMode.Windowed,
    int ResolutionWidth = 1920,
    int ResolutionHeight = 1080,
    bool VSync = true,
    string Language = "en",
    bool AutosaveEnabled = true,
    int AutosaveIntervalMinutes = 5
)
{
    public static UserSettings CreateDefault() => new();
}

public class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public string SerializeSettings(UserSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return JsonSerializer.Serialize(settings, JsonOptions);
    }

    public UserSettings DeserializeSettings(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return UserSettings.CreateDefault();
        }

        try
        {
            return JsonSerializer.Deserialize<UserSettings>(json, JsonOptions) ?? UserSettings.CreateDefault();
        }
        catch
        {
            return UserSettings.CreateDefault();
        }
    }
}
