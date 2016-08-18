using System;
using System.Collections.Generic;
using Microsoft.Cloud.InstrumentationFramework;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.IfxEvents
{
    public class GenevaWarmPathBatchingSink : PeriodicBatchingSink
    {
        readonly ITextFormatter _formatter;
        public GenevaWarmPathBatchingSink(
            ITextFormatter formatter,
            int batchSizeLimit,
            TimeSpan period)
            : base(batchSizeLimit, period)
        {
            if (batchSizeLimit < 1 || batchSizeLimit > 100)
            {
                throw new ArgumentException(
                    "batchSizeLimit must be between 1 and 100.");
            }
            _formatter = formatter;
        }
        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            foreach (var logEvent in events)
            {
                IfxEvent.Log(logEvent.GetIfxEvent(_formatter));
            }
        }
    }
}
