using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.ObjectPool;
using RendleLabs.Diagnostics.Internal;

namespace RendleLabs.Diagnostics
{
    public static class DiagnosticSourceExtensions
    {
        private static readonly ObjectPool<DisposableActivity> Pool = new DefaultObjectPool<DisposableActivity>(new DefaultPooledObjectPolicy<DisposableActivity>());
        private static readonly Writable? Null = null;
        private static readonly NullDisposableActivity NullActivity = new NullDisposableActivity();

        public static Writable? IfEnabled(this DiagnosticSource source, string name) =>
            source.IsEnabled(name) ? new Writable(source, name) : Null;

        public static Writable? IfEnabled(this DiagnosticSource source, string name, object? arg1, object? arg2 = null) =>
            source.IsEnabled(name, arg1, arg2) ? new Writable(source, name) : Null;

        public static IDisposableActivity StartActivity(this Writable? writable, string operationName)
        {
            if (!writable.HasValue) return NullActivity;
            
            return Pool.Get().Initialize(Pool, new Activity(operationName), writable.Value.Source);
        }

        public readonly struct Writable
        {
            internal readonly DiagnosticSource Source;
            private readonly string _name;

            public Writable(DiagnosticSource source, string name)
            {
                Source = source;
                _name = name;
            }

            public void Write(object args)
            {
                Source.Write(_name, args);
            }

            public void StartActivity(Activity activity, object args)
            {
                Source.StartActivity(activity, args);
            }

            public void StopActivity(Activity activity, object args)
            {
                Source.StopActivity(activity, args);
            }
        }
    }
}