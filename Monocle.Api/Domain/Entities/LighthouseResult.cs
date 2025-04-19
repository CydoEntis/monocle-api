namespace Monocle.Api.Domain.Entities;

public class LighthouseResult
{
    public double TTFB { get; set; }
    public double LCP { get; set; }
    public double CLS { get; set; }
    public double TotalJsSizeKb { get; set; }
    public List<string> Suggestions { get; set; } = [];
}