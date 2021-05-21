namespace Codartis.NsDepCop.Util
{
    /// <summary>
    /// Wraps an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the wrapped object.</typeparam>
    public class ObjectWrapper<T>
    {
        public T Value { get; }

        public ObjectWrapper(T value)
        {
            Value = value;
        }
    }
}