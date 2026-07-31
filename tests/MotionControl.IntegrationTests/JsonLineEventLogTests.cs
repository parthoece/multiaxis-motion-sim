using System.Text.Json;
using MotionControl.Persistence;

namespace MotionControl.IntegrationTests;

public sealed class JsonLineEventLogTests
{
    [Fact]
    public async Task EventLogWritesOneValidJsonObjectPerLine()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var path = Path.Combine(
            Path.GetTempPath(),
            $"multiaxis-motion-events-{Guid.NewGuid():N}.jsonl");

        try
        {
            var log = new JsonLineEventLog(path);

            await log.WriteAsync(
                "Information",
                "TestEvent",
                new { cycleId = "CY-001", result = "PASS" },
                cancellationToken);

            var lines = await File.ReadAllLinesAsync(
                path,
                cancellationToken);

            Assert.Single(lines);

            using var document = JsonDocument.Parse(lines[0]);

            Assert.Equal(
                "TestEvent",
                document.RootElement
                    .GetProperty("eventName")
                    .GetString());

            Assert.Equal(
                "Information",
                document.RootElement
                    .GetProperty("level")
                    .GetString());
        }
        finally
        {
            File.Delete(path);
        }
    }
}