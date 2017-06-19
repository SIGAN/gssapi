using System;
using System.Runtime.InteropServices;

namespace GSSAPI.Native
{
    public static class NativeMethods32
    {
        /// Return Type: OM_uint32->gss_uint32->unsigned int
        ///param0: OM_uint32*
        ///param1: gss_buffer_t->gss_buffer_desc_struct*
        ///param2: gss_OID->gss_OID_desc_struct*
        ///param3: gss_name_t*
        [DllImport("gssapi32.dll", EntryPoint = "gss_import_name")]
        public static extern uint gss_import_name(ref uint minorStatus, ref GssBufferDescStruct inputNameBuffer, ref GssOidDescStruct inputNameType, ref IntPtr outputName);

        /// Return Type: OM_uint32->gss_uint32->unsigned int
        ///param0: OM_uint32*
        ///param1: gss_cred_id_t->gss_cred_id_struct*
        ///param2: gss_ctx_id_t*
        ///param3: gss_name_t->gss_name_struct*
        ///param4: gss_OID->gss_OID_desc_struct*
        ///param5: OM_uint32->gss_uint32->unsigned int
        ///param6: OM_uint32->gss_uint32->unsigned int
        ///param7: gss_channel_bindings_t->gss_channel_bindings_struct*
        ///param8: gss_buffer_t->gss_buffer_desc_struct*
        ///param9: gss_OID*
        ///param10: gss_buffer_t->gss_buffer_desc_struct*
        ///param11: OM_uint32*
        ///param12: OM_uint32*
        [DllImport("gssapi32.dll", EntryPoint = "gss_init_sec_context")]
        public static extern uint gss_init_sec_context(ref uint minorStatus, IntPtr claimantCredHandle, ref IntPtr contextHandle, IntPtr targetName, ref GssOidDescStruct mechType, uint reqFlags, uint timeReq, IntPtr inputChanBindings, ref GssBufferDescStruct inputToken, IntPtr actualMechType, ref GssBufferDescStruct outputToken, IntPtr retFlags, IntPtr timeRec);

        /// Return Type: OM_uint32->gss_uint32->unsigned int
        ///param0: OM_uint32*
        ///param1: gss_buffer_t->gss_buffer_desc_struct*
        [DllImport("gssapi32.dll", EntryPoint = "gss_release_buffer")]
        public static extern uint gss_release_buffer(ref uint minorStatus, ref GssBufferDescStruct buffer);



        /// Return Type: OM_uint32->gss_uint32->unsigned int
        ///param0: OM_uint32*
        ///param1: gss_ctx_id_t*
        ///param2: gss_buffer_t->gss_buffer_desc_struct*
        [DllImport("gssapi32.dll", EntryPoint = "gss_delete_sec_context")]
        public static extern uint gss_delete_sec_context(ref uint minorStatus, ref IntPtr contextHandle, IntPtr outputToken);

        /// Return Type: OM_uint32->gss_uint32->unsigned int
        ///param0: OM_uint32*
        ///param1: gss_name_t*
        [DllImport("gssapi32.dll", EntryPoint = "gss_release_name")]
        public static extern uint gss_release_name(ref uint minorStatus, ref IntPtr inputName);
    }
}