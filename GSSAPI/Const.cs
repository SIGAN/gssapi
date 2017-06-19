using System;
using GSSAPI.Native;
using GSSAPI.Utility;

namespace GSSAPI
{
    /// <summary>
    /// GSS API constants
    /// </summary>
    public static class Const
    {
        /// <summary>
        /// GSS_C_NT_HOSTBASED_SERVICE
        /// </summary>
        private static readonly byte[] GssNtServiceName = { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x12, 0x01, 0x02, 0x01, 0x04 };

        /// <summary>
        /// Returns GSS OID of GSS_C_NT_HOSTBASED_SERVICE
        /// </summary>
        public static AutoDisposing<GssOidDescStruct> GetGssKrb5NtServiceName() =>
            AutoDisposing.From(
                Pinned.From(GssNtServiceName),
                p => new GssOidDescStruct
                {
                    length = (uint)p.Value.Length,
                    elements = p.Addr
                });

        /// <summary>
        /// GSS_KRB5_MECH_OID_DESC
        /// </summary>
        private static readonly byte[] GssKrb5MechOidDesc = { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x12, 0x01, 0x02, 0x02 };

        /// <summary>
        /// Returns GSS OID of GSS_KRB5_MECH_OID_DESC
        /// </summary>
        public static AutoDisposing<GssOidDescStruct> GetGssKrb5MechOidDesc() =>
            AutoDisposing.From(
                Pinned.From(GssKrb5MechOidDesc),
                p => new GssOidDescStruct
                {
                    length = (uint)p.Value.Length,
                    elements = p.Addr
                });

        /// <summary>
        /// GSS_SPNEGO_MECH_OID_DESC
        /// </summary>
        private static readonly byte[] GssSpnegoMechOidDesc = { 0x2b, 0x06, 0x01, 0x05, 0x05, 0x02 };

        /// <summary>
        /// Returns GSS OID of GSS_SPNEGO_MECH_OID_DESC
        /// </summary>
        public static AutoDisposing<GssOidDescStruct> GetGssSpnegoMechOidDesc() =>
            AutoDisposing.From(
                Pinned.From(GssSpnegoMechOidDesc),
                p => new GssOidDescStruct
                {
                    length = (uint)p.Value.Length,
                    elements = p.Addr
                });

        /// <summary>
        /// GSS major status error mask
        /// </summary>
        public const uint GssErrorMask = 0xFFFF0000U;

        /// <summary>
        /// GSS indefinite time
        /// </summary>
        public const uint GssIndefinite = 0xFFFFFFFFU;

        /// <summary>
        /// GSS_S_COMPLETE
        /// </summary>
        public const uint GssComplete = 0U;

        /// <summary>
        /// GSS_S_CONTINUE_NEEDED
        /// </summary>
        public const uint GssContinueNeeded = 1U;
    }
}
