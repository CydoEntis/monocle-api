namespace Monocle.Api.Domain.Entities;

public class ProfilerResult
{
    public List<NetworkResource> BlockingResources { get; set; } = [];
    public List<NetworkResource> NetworkTimings { get; set; } = [];
}