using Playbook.Samples.PropertyAssignment;

StorageBay bay = new(" swan-7 ");
CheckCode(bay, "SWAN-7", "Constructor normalizes through the setter.");
bay.Code = " quay-2 ";
CheckCode(bay, "QUAY-2", "Assignments normalize identically.");
Reject<ArgumentNullException>(() => bay.Code = null!);
Reject<ArgumentException>(() => bay.Code = " \t ");
Reject<ArgumentOutOfRangeException>(() => bay.Code = "1234567890123");
CheckCode(bay, "QUAY-2", "Rejected assignments preserve the existing value.");
Reject<ArgumentException>(() => _ = new StorageBay(" "));
Console.WriteLine("PASS: constructor, normalization, null/blank/length validation and retained value.");
Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");

static void CheckCode(StorageBay bay, string expected, string message)
{
    if (bay.Code != expected)
    {
        throw new InvalidOperationException(message);
    }
}

static void Reject<TException>(Action action) where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}
