namespace Monocle.Api.Domain.Entities;

public class PerformanceResult
{
    public double TTFB { get; set; }
    public double LCP { get; set; }
    public double CLS { get; set; }
    public double JSBundleSizeKb { get; set; }
    public List<NetworkResource> BlockingResources { get; set; }
    public List<NetworkResource> Waterfall { get; set; }
    public List<string> Suggestions { get; set; }
}