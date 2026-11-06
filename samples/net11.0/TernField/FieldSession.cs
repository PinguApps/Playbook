// snippet:session
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace TernField;

public sealed class FieldSession(IPermissions permissions)
{
    public FieldSession() : this(Permissions.Current) { }
    private bool listening;
    public string? LatestFix { get; private set; }

    public async Task<string> StartAsync()
    {
        if (listening) return "Already listening";
        PermissionStatus location = await permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (location != PermissionStatus.Granted)
            location = await permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (location != PermissionStatus.Granted) return $"Location: {location}";

        PermissionStatus notifications = await permissions.CheckStatusAsync<Permissions.PostNotifications>();
        if (notifications != PermissionStatus.Granted)
            notifications = await permissions.RequestAsync<Permissions.PostNotifications>();

        Geolocation.LocationChanged += OnLocation;
        Geolocation.ListeningFailed += OnFailure;
        try
        {
            listening = await Geolocation.StartListeningForegroundAsync(new GeolocationListeningRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                MinimumTime = TimeSpan.FromSeconds(5),
                MinimumDistance = 35
            });
            if (!listening) Stop();
        }
        catch { Stop(); throw; }
        return $"Listening: {listening}; notification authorization: {notifications}";
    }

    private void OnLocation(object? sender, GeolocationLocationChangedEventArgs args)
    {
        Location fix = args.Location;
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
