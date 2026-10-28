// snippet:lifetime
using Microsoft.AspNetCore.Components.Forms;

namespace Playbook.Samples;

public sealed class RepairEditor : IDisposable
{
    private readonly IServiceProvider _services;
    private IDisposable? _subscription;
    public RepairJob Model { get; private set; } = default!;
    public EditContext Context { get; private set; } = default!;

    public RepairEditor(IServiceProvider services)
    {
        _services = services;
        Replace();
    }

    public void Replace()
    {
        _subscription?.Dispose();
        Model = new RepairJob();
        Context = new EditContext(Model);
        _subscription = Context.EnableDataAnnotationsValidation(_services);
    }

    public void Dispose() => _subscription?.Dispose();
}
// endsnippet:lifetime
