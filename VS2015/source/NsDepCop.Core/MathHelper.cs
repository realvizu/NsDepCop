namespace Codartis.NsDepCop.Core
{
    /// <summary>
    /// Static helper class for simple math operations.
    /// </summary>
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