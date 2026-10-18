using System;
using System.Collections.Generic;

namespace WorldState.Domain.Map;

public readonly record struct ColorRgba(float R, float G, float B, float A = 1.0f)
{
    public static readonly ColorRgba Transparent = new(0f, 0f, 0f, 0f);
    public static readonly ColorRgba White = new(1f, 1f, 1f, 1f);
    public static readonly ColorRgba Black = new(0f, 0f, 0f, 1f);

    public static ColorRgba FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return White;
        string clean = hex.Trim().TrimStart('#');
        if (clean.Length == 6)
        {
            byte r = Convert.ToByte(clean.Substring(0, 2), 16);
            byte g = Convert.ToByte(clean.Substring(2, 2), 16);
            byte b = Convert.ToByte(clean.Substring(4, 2), 16);
            return new ColorRgba(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f);
        }
        if (clean.Length == 8)
        {
            byte r = Convert.ToByte(clean.Substring(0, 2), 16);
            byte g = Convert.ToByte(clean.Substring(2, 2), 16);
            byte b = Convert.ToByte(clean.Substring(4, 2), 16);
            byte a = Convert.ToByte(clean.Substring(6, 2), 16);
            return new ColorRgba(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }
        return White;
    }

    public string ToHex(bool includeAlpha = false)
    {
        byte r = (byte)Math.Clamp((int)Math.Round(R * 255), 0, 255);
        byte g = (byte)Math.Clamp((int)Math.Round(G * 255), 0, 255);
        byte b = (byte)Math.Clamp((int)Math.Round(B * 255), 0, 255);
        byte a = (byte)Math.Clamp((int)Math.Round(A * 255), 0, 255);

        return includeAlpha || a < 255
            ? $"#{r:X2}{g:X2}{b:X2}{a:X2}"
            : $"#{r:X2}{g:X2}{b:X2}";
    }
}

public record MapPalette(
    string Name,
    string DisplayName,
    ColorRgba OceanColor,
    ColorRgba GridColor,
    ColorRgba NeutralFill,
    ColorRgba NeutralBorder,
    ColorRgba PlayerFill,
    ColorRgba PlayerBorder,
    ColorRgba FriendlyFill,
    ColorRgba FriendlyBorder,
    ColorRgba HostileFill,
    ColorRgba HostileBorder,
    ColorRgba AlliedFill,
    ColorRgba AlliedBorder,
    ColorRgba HoverBorder,
    ColorRgba SelectionBorder,
    ColorRgba TextColor
);

public static class MapThemeRegistry
{
    public const string DefaultPaletteName = "TacticalDark";

    public static readonly MapPalette TacticalDark = new(
        Name: "TacticalDark",
        DisplayName: "Tactical Dark",
        OceanColor: ColorRgba.FromHex("#0B0F17"),
        GridColor: ColorRgba.FromHex("#161F2E"),
        NeutralFill: ColorRgba.FromHex("#1F2937"),
        NeutralBorder: ColorRgba.FromHex("#374151"),
        PlayerFill: ColorRgba.FromHex("#13382C"),
        PlayerBorder: ColorRgba.FromHex("#10B981"),
        FriendlyFill: ColorRgba.FromHex("#1E2E4A"),
        FriendlyBorder: ColorRgba.FromHex("#3B82F6"),
        HostileFill: ColorRgba.FromHex("#3E1821"),
        HostileBorder: ColorRgba.FromHex("#EF4444"),
        AlliedFill: ColorRgba.FromHex("#1A3344"),
        AlliedBorder: ColorRgba.FromHex("#06B6D4"),
        HoverBorder: ColorRgba.FromHex("#F59E0B"),
        SelectionBorder: ColorRgba.FromHex("#FF9933"),
        TextColor: ColorRgba.FromHex("#F9FAFB")
    );

    public static readonly MapPalette StrategicNavy = new(
        Name: "StrategicNavy",
        DisplayName: "Strategic Navy",
        OceanColor: ColorRgba.FromHex("#0A192F"),
        GridColor: ColorRgba.FromHex("#142947"),
        NeutralFill: ColorRgba.FromHex("#172A45"),
        NeutralBorder: ColorRgba.FromHex("#30486D"),
        PlayerFill: ColorRgba.FromHex("#0D3B3A"),
        PlayerBorder: ColorRgba.FromHex("#20D6AC"),
        FriendlyFill: ColorRgba.FromHex("#1D3557"),
        FriendlyBorder: ColorRgba.FromHex("#457B9D"),
        HostileFill: ColorRgba.FromHex("#4A1525"),
        HostileBorder: ColorRgba.FromHex("#E63946"),
        AlliedFill: ColorRgba.FromHex("#1E3F5A"),
        AlliedBorder: ColorRgba.FromHex("#64DFDF"),
        HoverBorder: ColorRgba.FromHex("#FFD166"),
        SelectionBorder: ColorRgba.FromHex("#F4A261"),
        TextColor: ColorRgba.FromHex("#E6F1FF")
    );

    public static readonly MapPalette AmberCartographic = new(
        Name: "AmberCartographic",
        DisplayName: "Amber Cartographic",
        OceanColor: ColorRgba.FromHex("#14120E"),
        GridColor: ColorRgba.FromHex("#26211A"),
        NeutralFill: ColorRgba.FromHex("#26231E"),
        NeutralBorder: ColorRgba.FromHex("#443E35"),
        PlayerFill: ColorRgba.FromHex("#362C16"),
        PlayerBorder: ColorRgba.FromHex("#E5A93C"),
        FriendlyFill: ColorRgba.FromHex("#222D25"),
        FriendlyBorder: ColorRgba.FromHex("#588B69"),
        HostileFill: ColorRgba.FromHex("#381F1A"),
        HostileBorder: ColorRgba.FromHex("#C84B31"),
        AlliedFill: ColorRgba.FromHex("#2B352E"),
        AlliedBorder: ColorRgba.FromHex("#7FB069"),
        HoverBorder: ColorRgba.FromHex("#F6C90E"),
        SelectionBorder: ColorRgba.FromHex("#FF884B"),
        TextColor: ColorRgba.FromHex("#F5E8D0")
    );

    private static readonly Dictionary<string, MapPalette> Palettes = new(StringComparer.OrdinalIgnoreCase)
    {
        [TacticalDark.Name] = TacticalDark,
        [StrategicNavy.Name] = StrategicNavy,
        [AmberCartographic.Name] = AmberCartographic
    };

    public static IReadOnlyCollection<MapPalette> GetAllPalettes() => Palettes.Values;

    public static MapPalette GetPalette(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name) && Palettes.TryGetValue(name, out var palette))
        {
            return palette;
        }
        return TacticalDark;
    }

    public static (ColorRgba Fill, ColorRgba Border, float BorderWidth) GetCountryColors(
        MapPalette palette,
        bool isPlayer,
        bool isSelected,
        bool isHovered,
        int relationScore = 0,
        bool isAtWar = false,
        bool hasNonAggressionPact = false)
    {
        ArgumentNullException.ThrowIfNull(palette);

        ColorRgba fill;
        ColorRgba border;
        float width = isSelected ? 3.0f : (isHovered ? 2.4f : 1.4f);

        if (isPlayer)
        {
            fill = palette.PlayerFill;
            border = isSelected ? palette.SelectionBorder : (isHovered ? palette.HoverBorder : palette.PlayerBorder);
            return (fill, border, width);
        }

        if (isAtWar || relationScore <= -50)
        {
            fill = palette.HostileFill;
            border = isSelected ? palette.SelectionBorder : (isHovered ? palette.HoverBorder : palette.HostileBorder);
            return (fill, border, width);
        }

        if (hasNonAggressionPact)
        {
            fill = palette.AlliedFill;
            border = isSelected ? palette.SelectionBorder : (isHovered ? palette.HoverBorder : palette.AlliedBorder);
            return (fill, border, width);
        }

        if (relationScore >= 50)
        {
            fill = palette.FriendlyFill;
            border = isSelected ? palette.SelectionBorder : (isHovered ? palette.HoverBorder : palette.FriendlyBorder);
            return (fill, border, width);
        }

        // Neutral
        fill = palette.NeutralFill;
        border = isSelected ? palette.SelectionBorder : (isHovered ? palette.HoverBorder : palette.NeutralBorder);
        return (fill, border, width);
    }
}
