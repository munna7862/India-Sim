namespace WorldState.Domain.Map;

public readonly record struct MapPoint(double Longitude, double Latitude);

public readonly record struct BoundingBox(double MinLon, double MinLat, double MaxLon, double MaxLat)
{
    public bool Contains(MapPoint pt) =>
        pt.Longitude >= MinLon && pt.Longitude <= MaxLon &&
        pt.Latitude >= MinLat && pt.Latitude <= MaxLat;

    public static BoundingBox FromPoints(IEnumerable<MapPoint> points)
    {
        double minLon = double.MaxValue;
        double minLat = double.MaxValue;
        double maxLon = double.MinValue;
        double maxLat = double.MinValue;

        bool hasPoints = false;
        foreach (var pt in points)
        {
            hasPoints = true;
            if (pt.Longitude < minLon) minLon = pt.Longitude;
            if (pt.Latitude < minLat) minLat = pt.Latitude;
            if (pt.Longitude > maxLon) maxLon = pt.Longitude;
            if (pt.Latitude > maxLat) maxLat = pt.Latitude;
        }

        if (!hasPoints)
        {
            return new BoundingBox(0, 0, 0, 0);
        }

        return new BoundingBox(minLon, minLat, maxLon, maxLat);
    }

    public static BoundingBox Combine(IEnumerable<BoundingBox> boxes)
    {
        double minLon = double.MaxValue;
        double minLat = double.MaxValue;
        double maxLon = double.MinValue;
        double maxLat = double.MinValue;

        bool hasBoxes = false;
        foreach (var b in boxes)
        {
            hasBoxes = true;
            if (b.MinLon < minLon) minLon = b.MinLon;
            if (b.MinLat < minLat) minLat = b.MinLat;
            if (b.MaxLon > maxLon) maxLon = b.MaxLon;
            if (b.MaxLat > maxLat) maxLat = b.MaxLat;
        }

        if (!hasBoxes)
        {
            return new BoundingBox(0, 0, 0, 0);
        }

        return new BoundingBox(minLon, minLat, maxLon, maxLat);
    }
}

public class MapPolygon
{
    public IReadOnlyList<MapPoint> Ring { get; }
    public BoundingBox Bbox { get; }

    public MapPolygon(IReadOnlyList<MapPoint> ring)
    {
        Ring = ring ?? throw new ArgumentNullException(nameof(ring));
        Bbox = BoundingBox.FromPoints(ring);
    }

    /// <summary>
    /// Evaluates whether a point is inside the polygon using Jordan curve theorem (ray casting).
    /// </summary>
    public bool Contains(MapPoint pt)
    {
        if (!Bbox.Contains(pt))
        {
            return false;
        }

        int count = Ring.Count;
        if (count < 3)
        {
            return false;
        }

        bool inside = false;
        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            var p1 = Ring[i];
            var p2 = Ring[j];

            // Check if point is on horizontal vertex or between y-extents
            if (((p1.Latitude > pt.Latitude) != (p2.Latitude > pt.Latitude)) &&
                (pt.Longitude < (p2.Longitude - p1.Longitude) * (pt.Latitude - p1.Latitude) / (p2.Latitude - p1.Latitude) + p1.Longitude))
            {
                inside = !inside;
            }
        }

        return inside;
    }
}

public class CountryGeometry
{
    public string CountryId { get; }
    public string Name { get; }
    public BoundingBox Bbox { get; }
    public IReadOnlyList<MapPolygon> Polygons { get; }
    public MapPoint Centroid { get; }

    public CountryGeometry(string countryId, string name, IReadOnlyList<MapPolygon> polygons, MapPoint centroid)
    {
        CountryId = countryId ?? throw new ArgumentNullException(nameof(countryId));
        Name = name ?? string.Empty;
        Polygons = polygons ?? throw new ArgumentNullException(nameof(polygons));
        Bbox = BoundingBox.Combine(polygons.Select(p => p.Bbox));
        Centroid = centroid;
    }

    public bool Contains(MapPoint pt)
    {
        if (!Bbox.Contains(pt))
        {
            return false;
        }

        foreach (var poly in Polygons)
        {
            if (poly.Contains(pt))
            {
                return true;
            }
        }

        return false;
    }
}

public class WorldMapData
{
    private readonly Dictionary<string, CountryGeometry> _countries = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<CountryGeometry> Countries => _countries.Values;

    public void AddCountry(CountryGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        _countries[geometry.CountryId] = geometry;
    }

    public bool TryGet(string countryId, out CountryGeometry? geometry) =>
        _countries.TryGetValue(countryId, out geometry);

    public string? HitTest(MapPoint pt)
    {
        foreach (var country in _countries.Values)
        {
            if (country.Contains(pt))
            {
                return country.CountryId;
            }
        }

        return null;
    }
}
