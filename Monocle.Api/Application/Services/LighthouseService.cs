using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monocle.Api.Domain.Entities;

namespace Monocle.Api.Application.Services
{
    public class LighthouseService
    {
        public async Task<LighthouseResult> RunAsync(string url)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "lh-output.json");

            var psi = new ProcessStartInfo
            {
                FileName = @"C:\Users\cstin\AppData\Roaming\npm\lighthouse.cmd",
                Arguments = $"{url} --output=json --output-path=\"{tempPath}\" --chrome-flags=\"--headless\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            // Set a timeout to avoid indefinite hanging
            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(5)); // Timeout after 5 minutes
            var processTask = ProcessRunAsync(process);

            var completedTask = await Task.WhenAny(processTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                process.Kill(); // Kill the process if it exceeds the timeout
                throw new TimeoutException("The Lighthouse process timed out.");
            }

            // Process completed successfully, return the result
            return await processTask;
        }

        private async Task<LighthouseResult> ProcessRunAsync(Process process)
        {
            string stdout = await process.StandardOutput.ReadToEndAsync();
            string stderr = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            // Handle the output
            Console.WriteLine("==== Lighthouse STDOUT ====");
            Console.WriteLine(stdout);
            Console.WriteLine("==== Lighthouse STDERR ====");
            Console.WriteLine(stderr);

            // ✅ Check if the file exists
            string tempPath = Path.Combine(Path.GetTempPath(), "lh-output.json");
            if (!File.Exists(tempPath))
            {
                Console.WriteLine($"File {tempPath} not found.");
                throw new FileNotFoundException("Lighthouse did not generate a report.", tempPath);
            }

            // ✅ Check if the file is empty
            string output = await File.ReadAllTextAsync(tempPath);
            if (string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine("Lighthouse output file is empty.");
                throw new Exception("Lighthouse output was empty.");
            }

            var json = JsonDocument.Parse(output);

            // Extract the relevant Lighthouse metrics directly from the JSON
            var lighthouseResult = new LighthouseResult
            {
                TTFB = GetMetricValue(json, "first-meaningful-paint"),
                LCP = GetMetricValue(json, "largest-contentful-paint"),
                CLS = GetMetricValue(json, "cumulative-layout-shift"),
                TotalJsSizeKb = GetJsSize(json),
                Suggestions = ExtractSuggestions(json)
            };

            return lighthouseResult;
        }

        // Helper method to extract specific metric values by audit ID
        private double GetMetricValue(JsonDocument json, string metric)
        {
            if (json.RootElement.TryGetProperty("audits", out var audits) &&
                audits.TryGetProperty(metric, out var audit) &&
                audit.TryGetProperty("numericValue", out var value) &&
                value.ValueKind == JsonValueKind.Number)
            {
                return value.GetDouble();
            }

            return 0;
        }

        // Helper method to extract the total JS size
        private double GetJsSize(JsonDocument json)
        {
            if (json.RootElement.TryGetProperty("audits", out var audits) &&
                audits.TryGetProperty("total-byte-weight", out var audit) &&
                audit.TryGetProperty("numericValue", out var value) &&
                value.ValueKind == JsonValueKind.Number)
            {
                return value.GetDouble() / 1024; // Convert bytes to KB
            }

            return 0;
        }

        // Helper method to extract performance suggestions
        private List<string> ExtractSuggestions(JsonDocument json)
        {
            var suggestions = new List<string>();
            if (json.RootElement.TryGetProperty("audits", out var audits))
            {
                foreach (var audit in audits.EnumerateObject())
                {
                    // Only look at audits with a "score" and a low score value (e.g., < 0.9)
                    if (audit.Value.TryGetProperty("score", out var score) && score.ValueKind == JsonValueKind.Number)
                    {
                        double scoreValue = score.GetDouble();
                        if (scoreValue < 0.9)
                        {
                            // If the audit is low, add its title to the suggestions
                            suggestions.Add(audit.Value.GetProperty("title").GetString());
                        }
                    }
                }
            }

            return suggestions;
        }
    }
}