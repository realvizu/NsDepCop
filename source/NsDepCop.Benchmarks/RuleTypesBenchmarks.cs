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
    /// Benchmark matching how a RegexDomain is created in production (see DomainSpecificationParser):
    /// a Regex instance (not the static Regex cache), interpreted (not compiled).
    /// </summary>
    [Benchmark]
    public void RegexRule()
    {
        var regexDomain = new RegexDomain("/[A-Z.]*/", validate: true, RegexUsageMode.Instance, RegexCompilationMode.Interpreted);
        var config = new DependencyRulesBuilder().AddAllowed(regexDomain, regexDomain);
        BenchmarkCore(config, Iterations);
    }

    [Benchmark]
    public void PlaceholderRule()
    {
        var config = new DependencyRulesBuilder().AddAllowed(new PlaceholderDomain("[M].Services"), new PlaceholderDomain("[M].Domain"));
        BenchmarkCore(config, Iterations, "A.Services", "A.Domain");
    }

    [Benchmark]
    public void PlaceholderRule_MultiCapture()
    {
        var config = new DependencyRulesBuilder().AddAllowed(new PlaceholderDomain("[M*].Services"), new PlaceholderDomain("[M*].Domain"));
        BenchmarkCore(config, Iterations, "A.B.Services", "A.B.Domain");
    }

    [Benchmark]
    public void PlaceholderRule_Negated()
    {
        var config = new DependencyRulesBuilder().AddAllowed(new PlaceholderDomain("[M].Services"), new PlaceholderDomain("[!M].Domain"));
        BenchmarkCore(config, Iterations, "A.Services", "B.Domain");
    }

    private static void BenchmarkCore(IDependencyRules config, int iterations)
        => BenchmarkCore(config, iterations, "A.B.C", "D.E.F");

    private static void BenchmarkCore(IDependencyRules config, int iterations, string fromNamespace, string toNamespace)
    {
        var typeDependencyValidator = new TypeDependencyValidator(config);

        for (var i = 0; i < iterations; i++)
        {
            var dependencyStatus = typeDependencyValidator.IsAllowedDependency(new TypeDependency(fromNamespace, "T", toNamespace, "T", default));
            dependencyStatus.IsAllowed.Should().BeTrue();
        }
    }
}