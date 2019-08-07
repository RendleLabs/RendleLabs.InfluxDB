using System;
using System.Diagnostics;

namespace RendleLabs.Diagnostics
{
    public static class DiagnosticSourceExtensions
    {
        private static readonly Writable? Null = null;

        public static Writable? IfEnabled(this DiagnosticSource source, string name) =>
            source.IsEnabled(name) ? new Writable(source, name) : Null;

        public static Writable? IfEnabled(this DiagnosticSource source, string name, object? arg1, object? arg2 = null) =>
            source.IsEnabled(name, arg1, arg2) ? new Writable(source, name) : Null;


        public readonly struct Writable
        {
            private readonly DiagnosticSource _source;
            private readonly string _name;

            public Writable(DiagnosticSource source, string name)
            {
                _source = source;
                _name = name;
            }

            public void Write(object args)
            {
                _source.Write(_name, args);
            }

            public void StartActivity(Activity activity, object args)
            {
                _source.StartActivity(activity, args);
            }

            public void StopActivity(Activity activity, object args)
            {
                _source.StopActivity(activity, args);
            }
        }
    }
}