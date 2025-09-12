using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Diagnostics;
using System.Net.Http;

namespace WorkoutTracker.Web.E2ETests;

/// <summary>
/// Base class for E2E tests that automatically manages the web server
/// for automatic server management without requiring manual server startup.
/// </summary>
public abstract class E2ETestBase : PageTest, IDisposable
{
    private Process? _serverProcess;
    private bool _disposed = false;

    protected string BaseUrl { get; private set; } = string.Empty;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Find an available port
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        
        BaseUrl = $"http://127.0.0.1:{port}";
        
        // Check if server is already running on this port
        if (!await IsServerRunning())
        {
            await StartServer();
            await WaitForServerToStart();
        }
        
        Console.WriteLine($"E2E Test server started at: {BaseUrl}");
    }

    private async Task<bool> IsServerRunning()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2);
            var response = await client.GetAsync(BaseUrl);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task StartServer()
    {
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WorkoutTracker.Web"));
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectPath}\" --urls \"{BaseUrl}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        _serverProcess = Process.Start(startInfo);
        
        if (_serverProcess == null)
        {
            throw new InvalidOperationException("Failed to start web server process");
        }
    }

    private async Task WaitForServerToStart()
    {
        var maxAttempts = 30;
        var delayMs = 1000;

        for (int i = 0; i < maxAttempts; i++)
        {
            if (await IsServerRunning())
            {
                return;
            }
            
            await Task.Delay(delayMs);
        }

        throw new TimeoutException($"Server did not start within {maxAttempts * delayMs / 1000} seconds");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Dispose();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                try
                {
                    _serverProcess?.Kill();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to kill server process: {ex.Message}");
                }
                finally
                {
                    _serverProcess?.Dispose();
                }
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}