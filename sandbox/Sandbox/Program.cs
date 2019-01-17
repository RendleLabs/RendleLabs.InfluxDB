using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sandbox
{
    class Program
    {
        private static readonly DiagnosticSource _diagnostics 
            = new DiagnosticListener(typeof(Program).FullName);

        static void Main(string[] args)
        {
            if (_diagnostics.IsEnabled("Main"))
            {
                _diagnostics.Write("Main", new { arg_count = args.Length });
            }
        }

        static async Task DoAThing(int id)
        {
            if (_diagnostics.IsEnabled("Things"))
            {
                var activity = new Activity("DoThing");
                var args = new { id };
                _diagnostics.StartActivity(activity, args);
                try
                {
                    await DoIt();
                }
                finally
                {
                    _diagnostics.StopActivity(activity, args);
                }
            }
        }

        static Task DoIt()
        {
            return Task.CompletedTask;
        }
    }

}
