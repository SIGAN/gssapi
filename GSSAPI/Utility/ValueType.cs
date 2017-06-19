namespace GSSAPI.Utility
{
    /// <summary>
    /// Object wrapper for value types
    /// </summary>
    public static class ValueType
    {
        /// <summary>
        /// Create wrapped object from <paramref name="value"/>
        /// </summary>
        /// <typeparam name="T">Any value type</typeparam>
        /// <param name="value">Value to wrap</param>
        /// <returns>Wrapped value</returns>
        public static ValueType<T> From<T>(T value) where T : struct => new ValueType<T>(value);
    }

    /// <summary>
    /// Object wrapper for value types
    /// </summary>
    /// <typeparam name="T">Any value type</typeparam>
    public sealed class ValueType<T>  where T : struct 
    {
        /// <summary>
        /// Original object value, can be used with <code>ref</code>
        /// </summary>
        public T Value;

        /// <summary>
        /// Create wrapped object from <paramref name="value"/>
        /// </summary>
        /// <param name="value">Value to wrap</param>
        public ValueType(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns original value
        /// </summary>
        public static implicit operator T(ValueType<T> p)
        {
            return p.Value;
        }
    }
}
