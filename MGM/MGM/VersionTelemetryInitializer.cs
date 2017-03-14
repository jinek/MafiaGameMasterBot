using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace MGM
{
    public class VersionTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _version;

        public VersionTelemetryInitializer(string version)
        {
            _version = version;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Component.Version = _version;
        }
    }
}