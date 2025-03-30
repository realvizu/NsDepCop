using BenchmarkDotNet.Running;

namespace NsDepCop.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<RuleTypesBenchmarks>();
    }
}