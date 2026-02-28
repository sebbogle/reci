namespace Tests.Fixtures;

using System.Diagnostics;
using System.Net;
using Tests.Utilities;

public class AppFixture : IAsyncLifetime
{
    private Process? _appProcess;

    public string BaseUrl { get; } = "http://localhost:5265";

    public async ValueTask InitializeAsync()
    {
        string projectRoot = FindProjectRoot();

        _appProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{Path.Combine(projectRoot, "Reci.csproj")}\" --urls {BaseUrl}",
                WorkingDirectory = projectRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            }
        };

        _appProcess.Start();

        _appProcess.BeginOutputReadLine();
        _appProcess.BeginErrorReadLine();

        using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(5) };

        for (int attempt = 0; attempt < 120; attempt++)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(BaseUrl);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch (Exception) when (attempt < 119)
            {
                // Server not ready yet
            }

            await Task.Delay(1000);
        }

        throw new TimeoutException($"App did not become available at {BaseUrl} within 120 seconds.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_appProcess is not null && !_appProcess.HasExited)
        {
            _appProcess.Kill(entireProcessTree: true);
            await _appProcess.WaitForExitAsync();
        }

        _appProcess?.Dispose();
    }

    private static string FindProjectRoot()
    {
        return ProjectPaths.FindRoot();
    }
}
