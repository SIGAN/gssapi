using System;
using System.Runtime.InteropServices;

namespace GSSAPI.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GssBufferDescStruct
    {
        /// size_t->unsigned int
        public uint length;

        /// void*
        public IntPtr value;
    }
}