using System;

namespace GSSAPI
{
    /// <summary>
    /// GSS client state
    /// </summary>
    internal class GssClientState
    {
        /// <summary>
        /// Service name
        /// </summary>
        public string Service;
        /// <summary>
        /// Unmanaged resource link to GSS Context
        /// </summary>
        public IntPtr Context;
        /// <summary>
        /// Unmanaged resource link to GSS server name
        /// </summary>
        public IntPtr ServerName;
        /// <summary>
        /// Authentication mechanism
        /// </summary>
        public GssClientAuth Auth;
        /// <summary>
        /// Request flags
        /// </summary>
        public GssContextFlags Flags;
        /// <summary>
        /// Flag if authentication is completed
        /// </summary>
        public bool Completed;
        /// <summary>
        /// Last token
        /// </summary>
        public string Token;
    }
}