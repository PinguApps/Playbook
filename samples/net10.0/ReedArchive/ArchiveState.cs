// snippet:complete
using Microsoft.AspNetCore.Components;
namespace ReedArchive;
public sealed class CollectionContext(string name)
{
    public string Name { get; } = name;
    public int Count { get; private set; }
    public void Add() => Count++;
}
public sealed class ArchiveState
{
    public CollectionContext Letters { get; } = new("letters");
    public CollectionContext Maps { get; } = new("maps");
    public CascadingValueSource<CollectionContext> LettersSource { get; }
    public CascadingValueSource<CollectionContext> MapsSource { get; }
    public ArchiveState()
    {
        LettersSource = new("letters", Letters, isFixed: false);
        MapsSource = new("maps", Maps, isFixed: false);
    }
    public Task AddLettersAsync() { Letters.Add(); return LettersSource.NotifyChangedAsync(); }
    public Task AddMapsAsync() { Maps.Add(); return MapsSource.NotifyChangedAsync(); }
}
// endsnippet:complete
