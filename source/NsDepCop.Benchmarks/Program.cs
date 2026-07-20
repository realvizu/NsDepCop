using BenchmarkDotNet.Running;

namespace NsDepCop.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        // Pass CLI args through so runs can be filtered/configured, eg.
        //   dotnet run -c Release -- --filter *PlaceholderRule* --job short
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}