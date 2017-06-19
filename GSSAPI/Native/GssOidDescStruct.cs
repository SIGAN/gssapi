using System;
using System.Runtime.InteropServices;

namespace GSSAPI.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GssOidDescStruct
    {
        /// OM_uint32->gss_uint32->unsigned int
        public uint length;

        /// void*
        public IntPtr elements;
    }
}