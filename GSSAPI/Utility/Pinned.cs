using System;
using System.Runtime.InteropServices;

namespace GSSAPI.Utility
{
    /// <summary>
    /// Memory pinned object
    /// </summary>
    public static class Pinned
    {
        /// <summary>
        /// Create memory pinned object from <paramref name="value"/>
        /// </summary>
        /// <typeparam name="T">Any class type</typeparam>
        /// <param name="value">Value to pin</param>
        /// <returns>Pinned value</returns>
        public static Pinned<T> From<T>(T value) where T : class => new Pinned<T>(value);
    }

    /// <summary>
    /// Memory pinned object
    /// </summary>
    /// <typeparam name="T">Any class type</typeparam>
    public sealed class Pinned<T> : IDisposable where T : class
    {
        /// <summary>
        /// Original object value, can be used with <code>ref</code>
        /// </summary>
        public T Value;
        /// <summary>
        /// In memory address of the object
        /// </summary>
        public IntPtr Addr { get; }

        private GCHandle _handle;

        /// <summary>
        /// Create memory pinned object from <paramref name="value"/>
        /// </summary>
        /// <param name="value">Value to pin</param>
        public Pinned(T value)
        {
            Value = value;
            _handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            Addr = _handle.AddrOfPinnedObject();
        }

        /// <summary>
        /// Returns address of object in memory
        /// </summary>
        public static implicit operator IntPtr(Pinned<T> p)
        {
            return p.Addr;
        }

        /// <summary>
        /// Returns original object value
        /// </summary>
        public static implicit operator T(Pinned<T> p)
        {
            return p.Value;
        }

        public void Dispose()
        {
            _handle.Free();
        }
    }
}
