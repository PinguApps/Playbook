// snippet:complete
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Media;
using Microsoft.Maui.Hosting;
using Microsoft.AspNetCore.Components.WebView.Maui;

internal static class BackendContracts
{
    public static async Task VerifyAsync()
    {
        var builder = MauiApp.CreateBuilder();
        builder.Services.AddMauiBlazorWebView().UsePlatformHandler<QuayBlazorHandler>();
        using var app = builder.Build();
        var handlers = app.Services.GetRequiredService<IMauiHandlersFactory>();
        if (handlers.GetHandler(typeof(IBlazorWebView)) is not QuayBlazorHandler)
            throw new InvalidOperationException("Custom Blazor handler registration lost.");
        var services = new ServiceCollection();
        services.AddSingleton<IScreenshot, ContractScreenshot>();
        services.AddSingleton<IGesturePlatformManagerFactory, QuayGestureFactory>();
        services.AddScoped<IAlertManager, QuayAlerts>();
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();
        var alerts = (QuayAlerts)scope.ServiceProvider.GetRequiredService<IAlertManager>();
        alerts.Subscribe();
        alerts.Subscribe();
        if (alerts.Subscriptions != 1) throw new InvalidOperationException("Duplicate alert subscription.");
        alerts.Unsubscribe();
        alerts.Unsubscribe();
        if (alerts.Subscriptions != 0) throw new InvalidOperationException("Alert subscription leaked.");

        var factory = provider.GetRequiredService<IGesturePlatformManagerFactory>();
        var handler = new QuayViewHandler();
        var first = (QuayGestures)factory.CreateGesturePlatformManager(handler);
        var second = (QuayGestures)factory.CreateGesturePlatformManager(handler);
        if (ReferenceEquals(first, second)) throw new InvalidOperationException("Gesture manager reused across connections.");
        first.Dispose();
        if (!first.Disposed || second.Disposed) throw new InvalidOperationException("Connection disposal is not isolated.");
        second.Dispose();

        // A contract token stands for a backend view. It is deliberately not a rendered native view.
        var screenshot = provider.GetRequiredService<IScreenshot>();
        var capture = (IViewScreenshot)screenshot;
        if (!screenshot.IsCaptureSupported || await capture.CaptureViewAsync(new QuayViewToken()) is null
            || await capture.CaptureViewAsync(new object()) is not null)
            throw new InvalidOperationException("Supported/unsupported screenshot dispatch failed.");
        var label = new Label();
        handler.SetMauiContext(new MauiContext(provider));
        handler.SetVirtualView(label);
        label.Handler = handler;
        if (await ((IView)label).CaptureAsync() is null)
            throw new InvalidOperationException("MAUI did not dispatch to registered IScreenshot/IViewScreenshot.");
        label.Handler = null;
        if (await ((IView)label).CaptureAsync() is not null)
            throw new InvalidOperationException("Handler-free view should have no screenshot.");
        Console.WriteLine("PASS: real MAUI interfaces; scoped idempotent alerts, per-connection gesture disposal, IScreenshot/IViewScreenshot contract dispatch. Rendering and image capture excluded.");
    }
}

internal sealed class QuayBlazorHandler : BlazorWebViewHandler
{
    protected override object CreatePlatformView() => throw new NotSupportedException("Contract-only backend has no WebView renderer.");
}

internal sealed class QuayAlerts : IAlertManager
{
    public int Subscriptions { get; private set; }
    public void Subscribe() => Subscriptions = 1;
    public void Unsubscribe() => Subscriptions = 0;
    public void RequestAlert(Page page, AlertArguments arguments) => throw new NotSupportedException("Contract demonstration has no dialog renderer.");
    public void RequestActionSheet(Page page, ActionSheetArguments arguments) => throw new NotSupportedException("Contract demonstration has no dialog renderer.");
    public void RequestPrompt(Page page, PromptArguments arguments) => throw new NotSupportedException("Contract demonstration has no dialog renderer.");
}

internal sealed class QuayGestureFactory : IGesturePlatformManagerFactory
{
    public IGesturePlatformManager CreateGesturePlatformManager(IViewHandler handler) => new QuayGestures();
}
internal sealed class QuayGestures : IGesturePlatformManager
{
    public bool Disposed { get; private set; }
    public void Dispose() => Disposed = true;
}
internal sealed class QuayViewToken;
internal sealed class QuayViewHandler : ViewHandler<IView, object>
{
    public QuayViewHandler() : base(new PropertyMapper<IView, QuayViewHandler>()) { }
    protected override object CreatePlatformView() => new QuayViewToken();
}
internal sealed class ContractScreenshot : IScreenshot, IViewScreenshot
{
    public bool IsCaptureSupported => true;
    public Task<IScreenshotResult> CaptureAsync() => throw new NotSupportedException("Whole-screen capture is outside this contract demonstration.");
    public Task<IScreenshotResult?> CaptureViewAsync(object platformView) =>
        Task.FromResult<IScreenshotResult?>(platformView is QuayViewToken ? new ContractScreenshotResult() : null);
}
internal sealed class ContractScreenshotResult : IScreenshotResult
{
    public int Width => 1;
    public int Height => 1;
    public Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100) => throw new NotSupportedException("No pixels captured by this contract double.");
    public Task CopyToAsync(Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100) => throw new NotSupportedException("No pixels captured by this contract double.");
}
// endsnippet:complete
