namespace Codartis.NsDepCop.Core.Analyzer.Common
{
    public static class MathHelper
    {
        public static double CalculatePercent(double part, double total)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return total == 0 
                ? 0 
                : part / total;
        }
    }
}