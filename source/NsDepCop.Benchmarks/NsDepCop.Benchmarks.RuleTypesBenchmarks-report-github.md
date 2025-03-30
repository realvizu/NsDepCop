```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5608/22H2/2022Update)
Intel Core i7-10850H CPU 2.70GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.201
  [Host]     : .NET 8.0.14 (8.0.1425.11118), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.14 (8.0.1425.11118), X64 RyuJIT AVX2


```
| Method             | Iterations | regexCompilationMode | useStaticRegexCache | Mean           | Error        | StdDev      | Gen0      | Gen1      | Allocated |
|------------------- |----------- |--------------------- |-------------------- |---------------:|-------------:|------------:|----------:|----------:|----------:|
| SimpleRule         | 1000       | ?                    | ?                   |       274.7 μs |      2.82 μs |     2.50 μs |  187.5000 |    0.4883 |   1.12 MB |
| WildcardRule       | 1000       | ?                    | ?                   |       493.7 μs |      3.13 μs |     2.93 μs |  259.7656 |    0.9766 |   1.56 MB |
| RegexRule_Instance | 1000       | Interpreted          | ?                   |       507.8 μs |      4.79 μs |     4.24 μs |  187.5000 |         - |   1.13 MB |
| RegexRule_Static   | 1000       | Compiled             | True                |       536.1 μs |     10.63 μs |     9.94 μs |  221.6797 |         - |   1.33 MB |
| RegexRule_Static   | 1000       | Interpreted          | True                |       642.8 μs |      6.56 μs |     6.13 μs |  221.6797 |         - |   1.33 MB |
| RegexRule_Instance | 1000       | Compiled             | ?                   |     1,065.3 μs |      3.66 μs |     3.24 μs |  189.4531 |   19.5313 |   1.13 MB |
| RegexRule_Static   | 1000       | Interpreted          | False               |     2,859.5 μs |     28.90 μs |    24.13 μs | 1312.5000 |         - |   7.94 MB |
| RegexRule_Static   | 1000       | Compiled             | False               | 1,948,017.4 μs | 10,476.50 μs | 8,748.35 μs | 4000.0000 | 3000.0000 |  29.51 MB |
