```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.8655)
12th Gen Intel Core i5-12400, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.301
  [Host]   : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX2
  ShortRun : .NET 10.0.9 (10.0.926.27113), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method       | Mean     | Error    | StdDev    | Gen0   | Gen1   | Allocated |
|------------- |---------:|---------:|----------:|-------:|-------:|----------:|
| SimpleXml    | 1.149 μs | 1.332 μs | 0.0730 μs | 0.9480 | 0.0267 |   8.73 KB |
| ComplexXml   | 9.103 μs | 9.432 μs | 0.5170 μs | 2.1667 | 0.0458 |  19.96 KB |
| NamespaceXml | 2.617 μs | 2.533 μs | 0.1388 μs | 1.0529 | 0.0305 |   9.77 KB |
