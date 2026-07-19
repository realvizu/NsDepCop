```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.8875)
Intel Core i7-10850H CPU 2.70GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.302
  [Host]     : .NET 8.0.29 (8.0.2926.32403), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.29 (8.0.2926.32403), X64 RyuJIT AVX2


```
| Method                       | Iterations | Mean     | Error   | StdDev  | Gen0     | Gen1   | Allocated |
|----------------------------- |----------- |---------:|--------:|--------:|---------:|-------:|----------:|
| SimpleRule                   | 1000       | 313.8 μs | 6.00 μs | 5.62 μs | 187.5000 | 0.4883 |   1.12 MB |
| RegexRule                    | 1000       | 536.0 μs | 6.74 μs | 6.30 μs | 187.5000 |      - |   1.13 MB |
| WildcardRule                 | 1000       | 566.1 μs | 6.00 μs | 5.32 μs | 259.7656 | 0.9766 |   1.56 MB |
| PlaceholderRule_Negated      | 1000       | 617.5 μs | 7.73 μs | 6.85 μs | 297.8516 | 1.9531 |   1.78 MB |
| PlaceholderRule              | 1000       | 623.7 μs | 8.71 μs | 8.94 μs | 292.9688 | 2.9297 |   1.76 MB |
| PlaceholderRule_MultiCapture | 1000       | 731.3 μs | 6.97 μs | 6.52 μs | 319.3359 | 3.9063 |   1.92 MB |
