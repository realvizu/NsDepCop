using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Codartis.NsDepCop.Analysis;
using Codartis.NsDepCop.Analysis.Implementation;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Test.Implementation.Analysis;
using FluentAssertions;

namespace NsDepCop.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class RuleTypesBenchmarks
{
    [Params(1000)]
    public int Iterations { get; set; }

    [Benchmark]
    public void SimpleRule()
    {
        var config = new DependencyRulesBuilder().AddAllowed(new Domain("A.B.C"), new Domain("D.E.F"));
        BenchmarkCore(config, Iterations);
    }

    [Benchmark]
    public void WildcardRule()
    {
        var config = new DependencyRulesBuilder().AddAllowed(new WildcardDomain("A.?.?"), new WildcardDomain("D.*"));
        BenchmarkCore(config, Iterations);
    }

    /// <summary>
    /// Benchmark using a Regex object instance, with or without RegexOptions.Compiled.
    /// </summary>
    [Benchmark]
    [Arguments(RegexCompilationMode.Compiled)]
    [Arguments(RegexCompilationMode.Interpreted)]
    public void RegexRule_Instance(RegexCompilationMode regexCompilationMode)
    {
        var regexDomain = new RegexDomain("/[A-Z.]*/", validate: true, RegexUsageMode.Instance, regexCompilationMode);
        var config = new DependencyRulesBuilder().AddAllowed(regexDomain, regexDomain);
        BenchmarkCore(config, Iterations);
    }

    /// <summary>
    /// Benchmark using the static Regex.IsMatch method, with or without RegexOptions.Compiled,
    /// and with or without using the built-in cache for static Regex.IsMatch calls.
    /// The reason behind measuring performance also with the static Regex cache turned off,
    /// is that in real-world usage there probably will be more than 15 different patterns,
    /// so the cache won't be effective anyway.
    /// </summary>
    /// <remarks>
    /// See: https://learn.microsoft.com/en-us/dotnet/standard/base-types/best-practices-regex
    /// By default, the last 15 most recently used static regular expression patterns are cached.
    /// For applications that require a larger number of cached static regular expressions,
    /// the size of the cache can be adjusted by setting the Regex.CacheSize property.
    /// </remarks>
    [Benchmark]
    [Arguments(RegexCompilationMode.Compiled, true)]
    [Arguments(RegexCompilationMode.Compiled, false)]
    [Arguments(RegexCompilationMode.Interpreted, true)]
    [Arguments(RegexCompilationMode.Interpreted, false)]
    public void RegexRule_Static(RegexCompilationMode regexCompilationMode, bool useStaticRegexCache)
    {
        Regex.CacheSize = useStaticRegexCache ? 15 : 0;

        var regexDomain = new RegexDomain("/[A-Z.]*/", validate: true, RegexUsageMode.Static, regexCompilationMode);
        var config = new DependencyRulesBuilder().AddAllowed(regexDomain, regexDomain);
        BenchmarkCore(config, Iterations);
    }

    private static void BenchmarkCore(IDependencyRules config, int iterations)
    {
        var typeDependencyValidator = new TypeDependencyValidator(config);

        for (var i = 0; i < iterations; i++)
        {
            var dependencyStatus = typeDependencyValidator.IsAllowedDependency(new TypeDependency("A.B.C", "T", "D.E.F", "T", default));
            dependencyStatus.IsAllowed.Should().BeTrue();
        }
    }
}