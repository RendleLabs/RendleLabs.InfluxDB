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

Results from the benchmark:

`LineFormatter` is this project. `PointData` is the official client.

``` ini

BenchmarkDotNet=v0.11.0, OS=Windows 10.0.17134.165 (1803/April2018Update/Redstone4)
Intel Core i7-4770K CPU 3.50GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.302
  [Host]     : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT


```
|        Method |      Mean |     Error |    StdDev |   Gen 0 |  Gen 1 | Allocated |
|-------------- |----------:|----------:|----------:|--------:|-------:|----------:|
| LineFormatter |  99.08 us | 0.6337 us | 0.5927 us |  6.9580 |      - |  28.52 KB |
|     PointData | 129.67 us | 1.1886 us | 1.0537 us | 34.4238 | 0.2441 | 141.02 KB |

The raw performance difference is not that marked: 30 microseconds (for writing 128 entries) is neither here nor there. But the
difference in the amount of memory allocated and the number of GC heap allocations is pretty significant.

### Additional notes

In the official client, once these `PointData` instances have been constructed, they get buffered for however long the batch interval
is, and then used to create a `System.Net.Http.StringContent` via a process which involves creating a lot of intermediate strings.
That all happens on top of the benchmarked code above.

This project is using pooled `byte[]` arrays and writing directly to them from the benchmarked code, so once a buffer is full or the
batch interval is hit, that array is just wrapped in a `System.Net.Http.ByteArrayContent` and posted; no further processing or
allocations are necessary. As noted before, it's hard to benchmark that effectively, but maybe at some point I'll try to profile it with
dotTrace/dotMemory or maybe ANTS in a real application.
