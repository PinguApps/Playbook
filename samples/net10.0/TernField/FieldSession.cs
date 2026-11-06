// snippet:session
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace TernField;

public sealed class FieldSession
{

    private bool listening;
    private Location? lastAccepted;
    public string? LatestFix { get; private set; }

    public async Task<string> StartAsync()
    {
        if (listening) return "Already listening";
        PermissionStatus location = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (location != PermissionStatus.Granted)
            location = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (location != PermissionStatus.Granted) return $"Location: {location}";

        PermissionStatus notifications = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
        if (notifications != PermissionStatus.Granted)
            notifications = await Permissions.RequestAsync<Permissions.PostNotifications>();

        Geolocation.LocationChanged += OnLocation;
        Geolocation.ListeningFailed += OnFailure;
        try
        {
            listening = await Geolocation.StartListeningForegroundAsync(new GeolocationListeningRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                MinimumTime = TimeSpan.FromSeconds(5)
            });
            if (!listening) Stop();
        }
        catch { Stop(); throw; }
        return $"Listening: {listening}; notification authorization: {notifications}";
    }

    private void OnLocation(object? sender, GeolocationLocationChangedEventArgs args)
    {
        Location fix = args.Location;
        if (lastAccepted is not null && Location.CalculateDistance(lastAccepted, fix, DistanceUnits.Kilometers) < 0.035) return;
        lastAccepted = fix;
        LatestFix = $"{fix.Latitude}, {fix.Longitude}; altitude {fix.Altitude} m " +
            $"({fix.AltitudeReferenceSystem}); vertical accuracy {fix.VerticalAccuracy} m";
    }
    private void OnFailure(object? sender, GeolocationListeningFailedEventArgs args) => Stop();
    public void Stop()
    {
        Geolocation.LocationChanged -= OnLocation;
        Geolocation.ListeningFailed -= OnFailure;
        Geolocation.StopListeningForeground();
        listening = false;
    }
}
// endsnippet:session
