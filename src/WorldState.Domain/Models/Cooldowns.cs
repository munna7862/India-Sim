namespace WorldState.Domain.Models;

public class CooldownTracker
{
    // Key: "{CountryId}:{ActionKey}" -> ExpiryDate
    public Dictionary<string, DateOnly> Cooldowns { get; set; } = new();

    public bool IsOnCooldown(string countryId, string actionKey, DateOnly currentDate)
    {
        string key = $"{countryId}:{actionKey}";
        return Cooldowns.TryGetValue(key, out var expiry) && currentDate < expiry;
    }

    public DateOnly? GetExpiry(string countryId, string actionKey)
    {
        string key = $"{countryId}:{actionKey}";
        return Cooldowns.TryGetValue(key, out var expiry) ? expiry : null;
    }

    public void SetCooldown(string countryId, string actionKey, DateOnly expiryDate)
    {
        Cooldowns[$"{countryId}:{actionKey}"] = expiryDate;
    }

    public void ClearExpired(DateOnly currentDate)
    {
        var expiredKeys = Cooldowns.Where(kv => kv.Value <= currentDate).Select(kv => kv.Key).ToList();
        foreach (var key in expiredKeys)
        {
            Cooldowns.Remove(key);
        }
    }
}
