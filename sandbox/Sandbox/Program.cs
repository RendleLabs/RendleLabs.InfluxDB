using System;
using System.Diagnostics;

namespace Sandbox
{
    class Program
    {
        private static readonly DiagnosticSource _diagnostics = new DiagnosticListener(typeof(Program).FullName);

        static void Main(string[] args)
        {
            if (_diagnostics.IsEnabled("Main"))
            {
                _diagnostics.Write("Main", new { arg_count = args.Length });
            }
            
            Console.WriteLine("Hello World!");
        }
    }

}
