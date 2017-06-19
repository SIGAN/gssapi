using System;
using System.Text;
using GSSAPI.Native;

namespace GSSAPI.Utility
{
    internal static class Cvt
    {
        /// <summary>
        /// Character set according to GSS API documentation (UTF8 in another place)
        /// </summary>
        public static Encoding Iso = Encoding.GetEncoding("iso-8859-1");

        /// <summary>
        /// Returns buffer created from <paramref name="value"/> string
        /// </summary>
        public static AutoDisposing<GssBufferDescStruct> GetBufferFromString(string value) =>
            AutoDisposing.From(
                Pinned.From(Iso.GetBytes(value)),
                p => new GssBufferDescStruct
                {
                    length = (uint)p.Value.Length,
                    value = p.Addr
                });

        /// <summary>
        /// Returns buffer created from <paramref name="value"/> Base64 string
        /// </summary>
        public static AutoDisposing<GssBufferDescStruct> GetBufferFromBase64String(string value) =>
            AutoDisposing.From(
                Pinned.From(Convert.FromBase64String(value)),
                p => new GssBufferDescStruct
                {
                    length = (uint)p.Value.Length,
                    value = p.Addr
                });

        /// <summary>
        /// Returns buffer created from <paramref name="value"/> BASE64 string or empty buffer if <paramref name="value"/> is empty
        /// </summary>
        public static AutoDisposing<GssBufferDescStruct> GetBufferFromBase64StringOrEmpty(string value) =>
            string.IsNullOrWhiteSpace(value)
                ? AutoDisposing.From(GetEmptyBuffer())
                : GetBufferFromBase64String(value);

        /// <summary>
        /// Returns new empty buffer
        /// </summary>
        public static GssBufferDescStruct GetEmptyBuffer()
        {
            return new GssBufferDescStruct { length = 0, value = IntPtr.Zero };
        }
    }
}
