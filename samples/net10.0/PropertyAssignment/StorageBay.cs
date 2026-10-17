namespace Playbook.Samples.PropertyAssignment;

// snippet:storage-bay
public sealed class StorageBay
{
    public StorageBay(string code)
    {
        Code = code;
    }

    public string Code
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            string normalized = value.Trim().ToUpperInvariant();
            if (normalized.Length > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Bay codes must be at most 12 characters.");
            }

            field = normalized;
        }
    } = string.Empty;
}
// endsnippet:storage-bay
