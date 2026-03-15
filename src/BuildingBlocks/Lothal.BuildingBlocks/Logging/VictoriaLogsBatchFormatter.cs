using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http;

namespace Lothal.BuildingBlocks.Logging;

public class VictoriaLogsBatchFormatter : IBatchFormatter
{
    public void Format(IEnumerable<LogEvent> logEvents, ITextFormatter formatter, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(logEvents);
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(output);

        foreach (var logEvent in logEvents)
        {
            formatter.Format(logEvent, output);
        }
    }

    public void Format(IEnumerable<string> logEvents, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(logEvents);
        ArgumentNullException.ThrowIfNull(output);

        foreach (var logEvent in logEvents)
        {
            output.Write(logEvent);
            if (!logEvent.EndsWith("\n") && !logEvent.EndsWith("\r\n"))
            {
                output.WriteLine();
            }
        }
    }
}
