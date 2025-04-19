using Monocle.Api.Application.Features.Analyze;
using Monocle.Api.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<LighthouseService>();
builder.Services.AddScoped<BrowserProfilerService>();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapAnalyzeEndpoint();

app.Run();