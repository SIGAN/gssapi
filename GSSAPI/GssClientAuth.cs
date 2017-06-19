namespace GSSAPI
{
    /// <summary>
    /// GSS authentication mechanism
    /// </summary>
    public enum GssClientAuth
    {
        /// <summary>
        /// SPNEGO (negotiation)
        /// </summary>
        SpNego = 0,
        /// <summary>
        /// Kerberos
        /// </summary>
        Kerberos = 1
    }
}