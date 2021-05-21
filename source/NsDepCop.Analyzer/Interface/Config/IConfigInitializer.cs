namespace Codartis.NsDepCop.Interface.Config
{
    /// <summary>
    /// Provides operations to initialize an analyzer config.
    /// </summary>
    /// <typeparam name="TImplementer">The implementer type. Used for fluent interface style.</typeparam>
    public interface IConfigInitializer<out TImplementer>
    {
        TImplementer SetDefaultInfoImportance(Importance? defaultInfoImportance);
    }
}
