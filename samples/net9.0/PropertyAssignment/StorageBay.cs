namespace Playbook.Samples.PropertyAssignment;

// snippet:storage-bay
public sealed class StorageBay
{
    private string _code = string.Empty;

    public StorageBay(string code)
    {
        Code = code;
    }

    public string Code
    {
        get => _code;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            string normalized = value.Trim().ToUpperInvariant();
            if (normalized.Length > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Bay codes must be at most 12 characters.");
            }

            _code = normalized;
        }
    }
}
// endsnippet:storage-bay
