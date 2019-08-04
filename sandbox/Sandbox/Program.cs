using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sandbox
{
    class Program
    {
        private static readonly DiagnosticSource Diagnostics 
            = new DiagnosticListener(typeof(Program).FullName);

        static void Main(string[] args)
        {
            if (Diagnostics.IsEnabled("Main"))
            {
                Diagnostics.Write("Main", new { arg_count = args.Length });
            }
        }

        static async Task DoAThing(int id)
        {
            if (Diagnostics.IsEnabled("Things"))
            {
                var activity = new Activity("DoThing");
                var args = new { id };
                Diagnostics.StartActivity(activity, args);
                try
                {
                    await DoIt();
                }
                finally
                {
                    Diagnostics.StopActivity(activity, args);
                }
            }
        }

        static Task DoIt()
        {
            return Task.CompletedTask;
        }
    }

}
