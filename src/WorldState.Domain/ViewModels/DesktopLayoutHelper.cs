using System;
using System.Collections.Generic;
using System.Linq;
using WorldState.Domain.Data;
using WorldState.Domain.Models;
using WorldState.Domain.Settings;

namespace WorldState.Domain.ViewModels;

public enum DesktopDisplayTier
{
    Compact,   // 1280x720 or lower
    Standard,  // 1920x1080
    Large,     // 2560x1440
    UltraWide  // 3840x2160 or 4K
}

public record DesktopLayoutMetrics(
    DesktopDisplayTier Tier,
    int WindowWidth,
    int WindowHeight,
    double EffectiveScale,
    float TopBarHeight,
    float LeftNavWidth,
    float DrawerWidth,
    int BaseFontSize,
    int HeaderFontSize
);

public static class DesktopLayoutHelper
{
    public static DesktopLayoutMetrics CalculateLayout(int windowWidth, int windowHeight, double uiScale = 1.0)
    {
        windowWidth = Math.Max(1024, windowWidth);
        windowHeight = Math.Max(600, windowHeight);
        uiScale = Math.Clamp(uiScale, 0.75, 2.0);

        DesktopDisplayTier tier;
        if (windowWidth <= 1366)
        {
            tier = DesktopDisplayTier.Compact;
        }
        else if (windowWidth <= 2160)
        {
            tier = DesktopDisplayTier.Standard;
        }
        else if (windowWidth <= 2880)
        {
            tier = DesktopDisplayTier.Large;
        }
        else
        {
            tier = DesktopDisplayTier.UltraWide;
        }

        float baseTopBar = tier switch
        {
            DesktopDisplayTier.Compact => 48f,
            DesktopDisplayTier.Standard => 56f,
            DesktopDisplayTier.Large => 64f,
            DesktopDisplayTier.UltraWide => 72f,
            _ => 56f
        };

        float baseNavWidth = tier switch
        {
            DesktopDisplayTier.Compact => 56f,
            DesktopDisplayTier.Standard => 64f,
            DesktopDisplayTier.Large => 72f,
            DesktopDisplayTier.UltraWide => 80f,
            _ => 64f
        };

        float baseDrawerWidth = tier switch
        {
            DesktopDisplayTier.Compact => 380f,
            DesktopDisplayTier.Standard => 480f,
            DesktopDisplayTier.Large => 560f,
            DesktopDisplayTier.UltraWide => 640f,
            _ => 480f
        };

        int baseFont = tier switch
        {
            DesktopDisplayTier.Compact => 12,
            DesktopDisplayTier.Standard => 14,
            DesktopDisplayTier.Large => 16,
            DesktopDisplayTier.UltraWide => 18,
            _ => 14
        };

        int headerFont = tier switch
        {
            DesktopDisplayTier.Compact => 18,
            DesktopDisplayTier.Standard => 22,
            DesktopDisplayTier.Large => 26,
            DesktopDisplayTier.UltraWide => 30,
            _ => 22
        };

        return new DesktopLayoutMetrics(
            Tier: tier,
            WindowWidth: windowWidth,
            WindowHeight: windowHeight,
            EffectiveScale: uiScale,
            TopBarHeight: (float)Math.Round(baseTopBar * uiScale),
            LeftNavWidth: (float)Math.Round(baseNavWidth * uiScale),
            DrawerWidth: (float)Math.Round(baseDrawerWidth * uiScale),
            BaseFontSize: (int)Math.Round(baseFont * uiScale),
            HeaderFontSize: (int)Math.Round(headerFont * uiScale)
        );
    }

    public static IEnumerable<GameTask> FilterTasks(IEnumerable<GameTask> tasks, string? query, string? statusFilter)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        var result = tasks;

        if (!string.IsNullOrWhiteSpace(statusFilter) && !statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            if (statusFilter.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(t => t.Status == GameTaskStatus.Active);
            }
            else if (statusFilter.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(t => t.Status == GameTaskStatus.Completed);
            }
            else if (statusFilter.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(t => t.Status == GameTaskStatus.Failed);
            }
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string q = query.Trim();
            result = result.Where(t =>
                t.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.Category.ToString().Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return result;
    }

    public static IEnumerable<CountryState> FilterNations(
        IEnumerable<CountryState> countries,
        string playerCountryId,
        CountryState? playerCountry,
        string? query,
        string? sortMode)
    {
        ArgumentNullException.ThrowIfNull(countries);

        var result = countries.Where(c => !string.Equals(c.Id, playerCountryId, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(query))
        {
            string q = query.Trim();
            result = result.Where(c =>
                c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.Id.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        // Sorting
        sortMode ??= "Name";
        result = sortMode.ToLowerInvariant() switch
        {
            "relations" when playerCountry != null => result.OrderByDescending(c => playerCountry.GetRelationsWith(c.Id).Relations),
            "treasury" => result.OrderByDescending(c => c.Treasury),
            "population" => result.OrderByDescending(c => c.Population),
            _ => result.OrderBy(c => c.Name)
        };

        return result;
    }
}
