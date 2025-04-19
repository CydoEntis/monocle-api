using Monocle.Api.Domain.Entities;

namespace Monocle.Api.Application.Services;

public class BrowserProfilerService
{
    public async Task<ProfilerResult> ProfileAsync(string url)
    {
        // TODO: Use PuppeteerSharp or Playwright for profiling
        await Task.CompletedTask;

        return new ProfilerResult
        {
            BlockingResources = new List<NetworkResource>(),
            NetworkTimings = new List<NetworkResource>()
        };
    }
}