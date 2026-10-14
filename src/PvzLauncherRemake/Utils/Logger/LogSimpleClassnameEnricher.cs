using Serilog.Core;
using Serilog.Events;

namespace PvzLauncherRemake.Utils.Logger
{
    internal sealed class LogSimpleClassnameEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (!logEvent.Properties.TryGetValue("SourceContext", out var value))
                return;

            var fullName = value.ToString().Trim('"');
            var simpleClassName = fullName.Replace("PvzLauncherRemake", "@");

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("SimpleClassname", simpleClassName));
        }
    }
}
