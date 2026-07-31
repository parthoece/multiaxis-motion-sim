using System.Text.Json;
using MotionControl.Application;

namespace MotionControl.Persistence;

public sealed class JsonLineEventLog : IOperationEventLog
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false,
        };

    private readonly string _path;
    private readonly SemaphoreSlim _writeGate = new(1, 1);

    public JsonLineEventLog(string path)
    {
        _path = path;
        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path)) ??
            throw new InvalidOperationException("A log directory is required."));
    }

    public async Task WriteAsync(
        string level,
        string eventName,
        object context,
        CancellationToken cancellationToken)
    {
        var record = new
        {
            timestamp = DateTimeOffset.UtcNow,
            level,
            eventName,
            context,
        };

        var line = JsonSerializer.Serialize(record, JsonOptions) + Environment.NewLine;

        await _writeGate.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(_path, line, cancellationToken);
        }
        finally
        {
            _writeGate.Release();
        }
    }
}
