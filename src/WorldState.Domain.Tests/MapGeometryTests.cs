using WorldState.Domain.Map;
using Xunit;

namespace WorldState.Domain.Tests;

public class MapGeometryTests
{
    [Fact]
    public void BoundingBox_Contains_EvaluatesCorrectly()
    {
        var bbox = new BoundingBox(10.0, 10.0, 20.0, 20.0);

        Assert.True(bbox.Contains(new MapPoint(15.0, 15.0)));
        Assert.True(bbox.Contains(new MapPoint(10.0, 10.0)));
        Assert.True(bbox.Contains(new MapPoint(20.0, 20.0)));
        Assert.False(bbox.Contains(new MapPoint(9.9, 15.0)));
        Assert.False(bbox.Contains(new MapPoint(20.1, 15.0)));
    }

    [Fact]
    public void BoundingBox_Combine_MergesExtentsCorrectly()
    {
        var box1 = new BoundingBox(10.0, 10.0, 20.0, 20.0);
        var box2 = new BoundingBox(5.0, 15.0, 25.0, 30.0);

        var combined = BoundingBox.Combine(new[] { box1, box2 });

        Assert.Equal(5.0, combined.MinLon);
        Assert.Equal(10.0, combined.MinLat);
        Assert.Equal(25.0, combined.MaxLon);
        Assert.Equal(30.0, combined.MaxLat);
    }

    [Fact]
    public void MapPolygon_Contains_RayCasting_InsideAndOutsidePoints()
    {
        // Simple triangle: (0,0), (10,0), (5,10)
        var ring = new List<MapPoint>
        {
            new(0.0, 0.0),
            new(10.0, 0.0),
            new(5.0, 10.0),
            new(0.0, 0.0)
        };

        var poly = new MapPolygon(ring);

        // Inside point
        Assert.True(poly.Contains(new MapPoint(5.0, 3.0)));

        // Outside points
        Assert.False(poly.Contains(new MapPoint(1.0, 8.0)));
        Assert.False(poly.Contains(new MapPoint(-1.0, 0.0)));
        Assert.False(poly.Contains(new MapPoint(12.0, 5.0)));
    }

    [Fact]
    public void MapPolygon_ConcaveShape_CorrectlyDistinguishesEnclaves()
    {
        // U-shaped / concave polygon
        var ring = new List<MapPoint>
        {
            new(0, 0),
            new(10, 0),
            new(10, 10),
            new(7, 10),
            new(7, 3),
            new(3, 3),
            new(3, 10),
            new(0, 10),
            new(0, 0)
        };

        var poly = new MapPolygon(ring);

        // In left leg
        Assert.True(poly.Contains(new MapPoint(1.5, 5.0)));
        // In right leg
        Assert.True(poly.Contains(new MapPoint(8.5, 5.0)));
        // In bottom bar
        Assert.True(poly.Contains(new MapPoint(5.0, 1.5)));
        // In cavity/concavity (outside the polygon)
        Assert.False(poly.Contains(new MapPoint(5.0, 6.0)));
    }

    [Fact]
    public void CountryGeometry_MultiPolygon_ContainsAnyPolygon()
    {
        var poly1 = new MapPolygon(new List<MapPoint>
        {
            new(0, 0), new(5, 0), new(5, 5), new(0, 5), new(0, 0)
        });
        var poly2 = new MapPolygon(new List<MapPoint>
        {
            new(20, 20), new(25, 20), new(25, 25), new(20, 25), new(20, 20)
        });

        var country = new CountryGeometry("TST", "Testland", new[] { poly1, poly2 }, new MapPoint(12.5, 12.5));

        Assert.True(country.Contains(new MapPoint(2, 2)));
        Assert.True(country.Contains(new MapPoint(22, 22)));
        Assert.False(country.Contains(new MapPoint(10, 10)));
    }

    [Fact]
    public void WorldMapData_HitTest_ReturnsCorrectCountry()
    {
        var world = new WorldMapData();
        var polyInd = new MapPolygon(new List<MapPoint>
        {
            new(68, 8), new(97, 8), new(97, 35), new(68, 35), new(68, 8)
        });
        var polyPak = new MapPolygon(new List<MapPoint>
        {
            new(60, 23), new(67, 23), new(67, 37), new(60, 37), new(60, 23)
        });

        world.AddCountry(new CountryGeometry("IND", "India", new[] { polyInd }, new MapPoint(78, 20)));
        world.AddCountry(new CountryGeometry("PAK", "Pakistan", new[] { polyPak }, new MapPoint(64, 30)));

        Assert.Equal("IND", world.HitTest(new MapPoint(75.0, 22.0)));
        Assert.Equal("PAK", world.HitTest(new MapPoint(64.0, 28.0)));
        Assert.Null(world.HitTest(new MapPoint(0.0, 0.0))); // Ocean / unmapped point
    }

    [Fact]
    public void MapProjection_ProjectAndUnproject_RoundTripConsistency()
    {
        var originalPt = new MapPoint(77.2090, 28.6139); // New Delhi
        double width = 1920;
        double height = 1080;
        double zoom = 2.5;
        double panX = 150.0;
        double panY = -80.0;

        var (screenX, screenY) = MapProjection.Project(originalPt, width, height, zoom, panX, panY);
        var unprojected = MapProjection.Unproject(screenX, screenY, width, height, zoom, panX, panY);

        Assert.InRange(unprojected.Longitude, originalPt.Longitude - 0.001, originalPt.Longitude + 0.001);
        Assert.InRange(unprojected.Latitude, originalPt.Latitude - 0.001, originalPt.Latitude + 0.001);
    }

    [Fact]
    public void MapProjection_ClampsZoomWithinMinAndMax()
    {
        var pt = new MapPoint(0, 0);
        var (x1, _) = MapProjection.Project(pt, 1000, 500, zoom: 0.1); // Below MinZoom
        var (x2, _) = MapProjection.Project(pt, 1000, 500, zoom: 1.0); // Exactly MinZoom
        Assert.Equal(x1, x2);

        var (x3, _) = MapProjection.Project(pt, 1000, 500, zoom: 50.0); // Above MaxZoom
        var (x4, _) = MapProjection.Project(pt, 1000, 500, zoom: 10.0); // Exactly MaxZoom
        Assert.Equal(x3, x4);
    }

    [Fact]
    public void GeoJsonMapLoader_ParsesGeoJson_AccuratelyExtractsCountries()
    {
        string geoJson = """
        {
          "type": "FeatureCollection",
          "features": [
            {
              "type": "Feature",
              "properties": { "id": "IND", "name": "India" },
              "geometry": {
                "type": "Polygon",
                "coordinates": [
                  [ [70.0, 10.0], [80.0, 10.0], [80.0, 30.0], [70.0, 30.0], [70.0, 10.0] ]
                ]
              }
            }
          ]
        }
        """;

        var world = GeoJsonMapLoader.LoadFromJson(geoJson);

        Assert.Single(world.Countries);
        Assert.True(world.TryGet("IND", out var indGeom));
        Assert.NotNull(indGeom);
        Assert.Equal("IND", indGeom!.CountryId);
        Assert.Equal("India", indGeom.Name);
        Assert.Equal("IND", world.HitTest(new MapPoint(75.0, 20.0)));
        Assert.Null(world.HitTest(new MapPoint(60.0, 20.0)));
    }
}
