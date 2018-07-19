using System;

namespace RendleLabs.DiagnosticSource.InfluxDBListener
{
    public class InfluxDBOptions
    {
        public Uri ServerUri { get; set; }
        public string Database { get; set; }
        public string RetentionPolicy { get; set; }
        public string Precision { get; set; }
    }
}