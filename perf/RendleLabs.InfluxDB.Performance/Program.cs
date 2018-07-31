using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis.Operations;
using RendleLabs.InfluxDB.DiagnosticSourceListener;

namespace RendleLabs.InfluxDB.Performance
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<FormatterBenchmark>();
        }
    }
}
