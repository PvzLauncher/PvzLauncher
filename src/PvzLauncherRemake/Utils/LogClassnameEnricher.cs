using Serilog.Core;
using Serilog.Events;

namespace PvzLauncherRemake.Utils
{
    internal sealed class LogClassnameEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (!logEvent.Properties.TryGetValue("SourceContext", out var value))
                return;

            var fullName = value.ToString().Trim('"');
            var index = fullName.LastIndexOf('.');

            var className = index >= 0 ? fullName[(index + 1)..] : fullName;

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Classname", className));
        }
    }
}
