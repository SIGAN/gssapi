namespace GSSAPI.AuthenticationModule
{
    /// <summary>
    /// KERBEROS negotiation via GSS API
    /// </summary>
    public class KerberosAuthenticationModule : AuthenticationModuleBase
    {
        public const string AuthType = "Kerberos";

        public override GssClientAuth Auth => GssClientAuth.Kerberos;
        public override string AuthenticationType => AuthType;
    }
}