using backend.Domain.Exceptions;

namespace backend.Domain.ValueObjects;

public class GeoLocation
{
    private GeoLocation()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public GeoLocation(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new DomainException("Latitude must be between -90 and 90.");

        if (longitude < -180 || longitude > 180)
            throw new DomainException("Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }
}