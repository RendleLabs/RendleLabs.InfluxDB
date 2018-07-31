# RendleLabs.InfluxDB

This is a work-in-progress. I'm trying to make a [DiagnosticSource](https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md)
listener that writes to InfluxDB with minimal overhead, using [the new Span/Memory APIs](https://www.codemag.com/Article/1807051/Introducing-.NET-Core-2.1-Flagship-Types-Span-T-and-Memory-T).
The default [InfluxDB C# client](https://github.com/influxdata/influxdb-csharp) is very allocate-y, creating lots of
`Dictionary<string,object>` and `Dictionary<string,string>` instances and other things for every write. This library instead
writes directly from the objects passed to `DiagnosticSource.Write` to a managed buffer of UTF8 bytes,
and sends the buffers to InfluxDB when they get full.
