# RendleLabs.InfluxDB

This is a work-in-progress. I'm trying to make a [DiagnosticSource](https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md)
listener that writes to InfluxDB with minimal overhead, using [the new Span/Memory APIs](https://www.codemag.com/Article/1807051/Introducing-.NET-Core-2.1-Flagship-Types-Span-T-and-Memory-T).
The default [InfluxDB C# client](https://github.com/influxdata/influxdb-csharp) is very allocate-y, creating lots of
`Dictionary<string,object>` and `Dictionary<string,string>` instances and other things for every write. This library instead
writes directly from the objects passed to `DiagnosticSource.Write` to a managed buffer of UTF8 bytes,
and sends the buffers to InfluxDB when they get full.

## Benchmarks

It's tricky to benchmark this, because sending the data to InfluxDB involves a lot of async background batching stuff,
but I've created a [BenchmarkDotNet](https://benchmarkdotnet.org) project (in the `perf` directory) to compare the conversion
of objects to InfluxDB format.
The official client exposes a `PointData` type which takes two `IReadOnlyDictionary` instances, and then actually converts those
to new `Dictionary` instances in the constructor. So the benchmark compares constructing `PointData` instances to using the
`InfluxLineFormatter` to write directly to a `Span<byte>`.

The benchmark code writes 128 

Results from the benchmark:

`LineFormatter` is this project. `PointData` is the official client, run with 128 items and 1024 items for each.

``` ini

BenchmarkDotNet=v0.11.0, OS=Windows 10.0.17134.165 (1803/April2018Update/Redstone4)
Intel Core i7-4770K CPU 3.50GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.302
  [Host]     : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT


```
|            Method |        Mean |      Error |     StdDev |      Median |    Gen 0 |   Gen 1 |  Allocated |
|------------------ |------------:|-----------:|-----------:|------------:|---------:|--------:|-----------:|
|  LineFormatter128 |    60.76 us |  0.4592 us |  0.4295 us |    60.69 us |   5.0049 |       - |   20.52 KB |
|      PointData128 |   128.14 us |  1.4037 us |  1.3130 us |   127.47 us |  34.4238 |  0.9766 |  141.02 KB |
| LineFormatter1024 |   493.18 us |  4.7369 us |  4.1991 us |   493.09 us |  39.0625 |       - |  163.98 KB |
|     PointData1024 | 1,204.12 us | 25.6625 us | 48.8256 us | 1,181.42 us | 183.5938 | 91.7969 | 1128.02 KB |

`LineFormatter` is significantly faster, with more than double the throughput, and only 15% of the memory usage and allocations.
I'm not quite sure where those allocations are happening, tbh, because once the Formatter is created, it should be allocation free
from then on. I'm doing some profiling to try and track those down.

Also notice with the `PointData` code with 1024 items, we start to get a lot of objects moving to `Gen 1`, which we should
try to avoid in hot-path code. All of `LineFormatter`'s heap-allocated objects are created for the lifetime of the application,
so they should just end up in `Gen 2` and not get in anybody's way.

(**Aside:** why isn't there a separate heap for objects that will never be collected until the process exits?)

### Additional notes

In the official client, once these `PointData` instances have been constructed, they get buffered for however long the batch interval
is, and then used to create a `System.Net.Http.StringContent` via a process which involves creating a lot of intermediate strings.
That all happens on top of the benchmarked code above.

This project is using pooled `byte[]` arrays and writing directly to them from the instrumentation code, so once a buffer is full or the
batch interval is hit, that array is just wrapped in a `System.Net.Http.ByteArrayContent` and posted; no further processing or
allocations are necessary. As noted before, it's hard to benchmark that effectively, but maybe at some point I'll try to profile it with
dotTrace/dotMemory or maybe ANTS in a real application.
