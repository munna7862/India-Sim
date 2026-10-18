namespace WorldState.Domain.Map;

public class MapProjection
{
    public const double MinZoom = 1.0;
    public const double MaxZoom = 10.0;

    /// <summary>
    /// Projects geographic (Lon, Lat) into 2D viewport coordinates using equirectangular projection.
    /// (Lon: -180..180, Lat: -90..90)
    /// </summary>
    public static (double X, double Y) Project(
        MapPoint point,
        double viewportWidth,
        double viewportHeight,
        double zoom = 1.0,
        double panX = 0.0,
        double panY = 0.0)
    {
        zoom = Math.Clamp(zoom, MinZoom, MaxZoom);

        // Normalize lon [-180, 180] -> [0, 1]
        double normX = (point.Longitude + 180.0) / 360.0;
        // Normalize lat [90, -90] -> [0, 1] (top to bottom)
        double normY = (90.0 - point.Latitude) / 180.0;

        double centerX = viewportWidth / 2.0;
        double centerY = viewportHeight / 2.0;

        double mapWidth = viewportWidth * zoom;
        double mapHeight = viewportHeight * zoom;

        double x = centerX + (normX - 0.5) * mapWidth + panX;
        double y = centerY + (normY - 0.5) * mapHeight + panY;

        return (x, y);
    }

    /// <summary>
    /// Converts 2D viewport screen coordinates back into geographic (Lon, Lat) coordinates.
    /// </summary>
    public static MapPoint Unproject(
        double screenX,
        double screenY,
        double viewportWidth,
        double viewportHeight,
        double zoom = 1.0,
        double panX = 0.0,
        double panY = 0.0)
    {
        zoom = Math.Clamp(zoom, MinZoom, MaxZoom);

        double centerX = viewportWidth / 2.0;
        double centerY = viewportHeight / 2.0;

        double mapWidth = viewportWidth * zoom;
        double mapHeight = viewportHeight * zoom;

        double normX = ((screenX - centerX - panX) / mapWidth) + 0.5;
        double normY = ((screenY - centerY - panY) / mapHeight) + 0.5;

        double lon = (normX * 360.0) - 180.0;
        double lat = 90.0 - (normY * 180.0);

        lon = Math.Clamp(lon, -180.0, 180.0);
        lat = Math.Clamp(lat, -90.0, 90.0);

        return new MapPoint(lon, lat);
    }
}
