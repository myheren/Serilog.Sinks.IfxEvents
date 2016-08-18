using Microsoft.Cloud.InstrumentationFramework;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.IfxEvents
{
    public class GenevaWarmPathSink:ILogEventSink
    {
        readonly ITextFormatter _formatter;
        public GenevaWarmPathSink(
            ITextFormatter formatter)
        {
            _formatter = formatter;
        }
        
        public void Emit(LogEvent logEvent)
        {
            var e = logEvent.GetIfxEvent(_formatter);
            IfxEvent.Log(e);
        }
    }
}
