using System.Runtime.InteropServices;

namespace GSSAPI.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GssChannelBindingsStruct
    {
        /// OM_uint32->gss_uint32->unsigned int
        public uint initiator_addrtype;

        /// gss_buffer_desc->gss_buffer_desc_struct
        public GssBufferDescStruct initiator_address;

        /// OM_uint32->gss_uint32->unsigned int
        public uint acceptor_addrtype;

        /// gss_buffer_desc->gss_buffer_desc_struct
        public GssBufferDescStruct acceptor_address;

        /// gss_buffer_desc->gss_buffer_desc_struct
        public GssBufferDescStruct application_data;
    }
}