using Monocle.Api.Application.Services;

namespace Monocle.Api.Application.Features.Analyze;

public static class AnalyzeEndpoint
{
    public static void MapAnalyzeEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/analyze", async (
                AnalyzeRequest request,
                LighthouseService lighthouse,
                BrowserProfilerService profiler) =>
            {
                var handler = new AnalyzeHandler(lighthouse, profiler);
                var response = await handler.HandleAsync(request);
                return Results.Ok(response);
            })
            .WithName("AnalyzeWebsite")
            .WithOpenApi();
    }
}