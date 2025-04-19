using Monocle.Api.Application.Services;

namespace Monocle.Api.Application.Features.Analyze;

public class AnalyzeHandler
{
    private readonly LighthouseService _lighthouse;
    private readonly BrowserProfilerService _profiler;

    public AnalyzeHandler(LighthouseService lighthouse, BrowserProfilerService profiler)
    {
        _lighthouse = lighthouse;
        _profiler = profiler;
    }

    public async Task<AnalyzeResponse> HandleAsync(AnalyzeRequest request)
    {
        var lighthouseData = await _lighthouse.RunAsync(request.Url);
        var profilerData = await _profiler.ProfileAsync(request.Url);

        return new AnalyzeResponse
        {
            TTFB = lighthouseData.TTFB,
            LCP = lighthouseData.LCP,
            CLS = lighthouseData.CLS,
            JSBundleSizeKb = lighthouseData.TotalJsSizeKb,
            Suggestions = lighthouseData.Suggestions,
            BlockingResources = profilerData.BlockingResources,
            Waterfall = profilerData.NetworkTimings
        };
    }
}