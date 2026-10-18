using System.Text.Json;

namespace WorldState.Domain.Map;

public static class GeoJsonMapLoader
{
    public static WorldMapData LoadFromJson(string geoJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(geoJson);

        using var doc = JsonDocument.Parse(geoJson);
        var root = doc.RootElement;

        var mapData = new WorldMapData();

        if (!root.TryGetProperty("features", out var featuresElement) || featuresElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("GeoJSON must contain a 'features' array.");
        }

        foreach (var feature in featuresElement.EnumerateArray())
        {
            if (!feature.TryGetProperty("geometry", out var geometryElement) ||
                !feature.TryGetProperty("properties", out var propertiesElement))
            {
                continue;
            }

            string countryId = ExtractCountryId(propertiesElement);
            if (string.IsNullOrWhiteSpace(countryId))
            {
                continue;
            }

            string countryName = ExtractCountryName(propertiesElement, countryId);

            var polygons = ParsePolygons(geometryElement);
            if (polygons.Count == 0)
            {
                continue;
            }

            var centroid = CalculateCentroid(polygons);
            var countryGeom = new CountryGeometry(countryId, countryName, polygons, centroid);
            mapData.AddCountry(countryGeom);
        }

        return mapData;
    }

    private static string ExtractCountryId(JsonElement properties)
    {
        string[] candidateProps = { "id", "ISO_A3", "ADM0_A3", "iso_a3", "adm0_a3", "ISO3" };
        foreach (var prop in candidateProps)
        {
            if (properties.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
            {
                string s = val.GetString()?.Trim().ToUpperInvariant() ?? string.Empty;
                if (s.Length == 3 && s != "-99")
                {
                    return s;
                }
            }
        }
        return string.Empty;
    }

    private static string ExtractCountryName(JsonElement properties, string fallback)
    {
        string[] candidateProps = { "name", "NAME", "admin", "ADMIN", "NAME_EN" };
        foreach (var prop in candidateProps)
        {
            if (properties.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
            {
                string s = val.GetString()?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(s))
                {
                    return s;
                }
            }
        }
        return fallback;
    }

    private static List<MapPolygon> ParsePolygons(JsonElement geometry)
    {
        var polygons = new List<MapPolygon>();
        if (!geometry.TryGetProperty("type", out var typeElement) ||
            !geometry.TryGetProperty("coordinates", out var coordsElement))
        {
            return polygons;
        }

        string type = typeElement.GetString() ?? string.Empty;

        if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
        {
            // coordinates: [ [ [lon, lat], [lon, lat], ... ], ...interior rings ]
            foreach (var ringElement in coordsElement.EnumerateArray())
            {
                var ring = ParseRing(ringElement);
                if (ring.Count >= 3)
                {
                    polygons.Add(new MapPolygon(ring));
                }
            }
        }
        else if (string.Equals(type, "MultiPolygon", StringComparison.OrdinalIgnoreCase))
        {
            // coordinates: [ [ [ [lon, lat], ... ] ] ]
            foreach (var polyElement in coordsElement.EnumerateArray())
            {
                foreach (var ringElement in polyElement.EnumerateArray())
                {
                    var ring = ParseRing(ringElement);
                    if (ring.Count >= 3)
                    {
                        polygons.Add(new MapPolygon(ring));
                    }
                }
            }
        }

        return polygons;
    }

    private static List<MapPoint> ParseRing(JsonElement ringElement)
    {
        var points = new List<MapPoint>();
        if (ringElement.ValueKind != JsonValueKind.Array)
        {
            return points;
        }

        foreach (var ptElement in ringElement.EnumerateArray())
        {
            if (ptElement.ValueKind == JsonValueKind.Array && ptElement.GetArrayLength() >= 2)
            {
                double lon = ptElement[0].GetDouble();
                double lat = ptElement[1].GetDouble();
                points.Add(new MapPoint(lon, lat));
            }
        }

        return points;
    }

    private static MapPoint CalculateCentroid(IReadOnlyList<MapPolygon> polygons)
    {
        double sumLon = 0.0;
        double sumLat = 0.0;
        long totalPoints = 0;

        foreach (var poly in polygons)
        {
            foreach (var pt in poly.Ring)
            {
                sumLon += pt.Longitude;
                sumLat += pt.Latitude;
                totalPoints++;
            }
        }

        if (totalPoints == 0)
        {
            return new MapPoint(0, 0);
        }

        return new MapPoint(sumLon / totalPoints, sumLat / totalPoints);
    }
}
