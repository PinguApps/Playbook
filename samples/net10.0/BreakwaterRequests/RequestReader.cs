// snippet:complete
using System.Net;
using Xamarin.Android.Net;

namespace BreakwaterRequests;

public sealed class RequestReader : IDisposable
{
    private readonly HttpClient client = new(new AndroidMessageHandler());

    public async Task<string> ReadAsync(Uri address, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await client.GetAsync(address,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (WebException legacy)
        {
            System.Diagnostics.Debug.WriteLine(legacy.Status);
            System.Diagnostics.Debug.WriteLine(legacy);
            throw;
        }
        catch (HttpRequestException failure)
        {
            // Log the complete exception locally; redact URLs/headers before sharing.
            System.Diagnostics.Debug.WriteLine(failure);
            if (failure.InnerException is WebException transport)
            {
                System.Diagnostics.Debug.WriteLine(transport.Status);
            }
            throw;
        }
    }

    public void Dispose() => client.Dispose();
}
// endsnippet:complete