using System.Text.Json;
using System.Text.Json.Serialization;
using WorldState.Domain.Models;

namespace WorldState.Domain.Services;

public class SaveLoadService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public string SerializeSession(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return JsonSerializer.Serialize(session, JsonOptions);
    }

    public GameSession DeserializeSession(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var session = JsonSerializer.Deserialize<GameSession>(json, JsonOptions);
        if (session == null)
        {
            throw new InvalidOperationException("Failed to deserialize GameSession from JSON.");
        }

        return session;
    }
}
