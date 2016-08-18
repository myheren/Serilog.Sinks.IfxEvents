using System;
using Microsoft.Cloud.InstrumentationFramework;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Serilog.Sinks.IfxEvents
{
    public static class LoggerConfigurationIfxExtensions
    {
        private const string DefaultOutputTemplate = "{Message}{NewLine}{Exception}";

        /// <summary>
        /// A reasonable default for the number of events posted in each batch.
        /// </summary>
        private const int DefaultBatchPostingLimit = 50;

        /// <summary>
        /// A reasonable default time to wait between checking for event batches.
        /// </summary>
        private static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(2);

        public static LoggerConfiguration GenevaWarmPath(this LoggerSinkConfiguration loggerConfiguration, string tenant, string role, string roleInstance, string currentDirectory, string outputTemplate = DefaultOutputTemplate, IFormatProvider formatProvider = null, LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum, bool writeInBatches = false, TimeSpan? period = null, int? batchPostingLimit = null)
        {
            if (loggerConfiguration == null)
                throw new ArgumentNullException("loggerConfiguration");
            if (tenant == null || role == null || roleInstance == null)
                throw new Exception("invalid Ifx Identity property");
            if (outputTemplate == null)
                throw new ArgumentNullException("outputTemplate");
            IfxInitializer.Initialize(tenant, role, roleInstance, currentDirectory);
            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);

            var sink = writeInBatches ?
                (ILogEventSink)new GenevaWarmPathBatchingSink(
                    formatter,
                    batchPostingLimit ?? DefaultBatchPostingLimit,
                    period ?? DefaultPeriod) :
                new GenevaWarmPathSink(
                    formatter);

            return loggerConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }
}
