using System;
using System.Text.RegularExpressions;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal static class NameFixer
    {
        public static Func<string, string> Default { get; } = DefaultFixer;
        
        private static readonly Regex Fix = new Regex(@"[\W]+");
        private static string DefaultFixer(string original) => Fix.Replace(original ?? string.Empty, "_");
    }
}