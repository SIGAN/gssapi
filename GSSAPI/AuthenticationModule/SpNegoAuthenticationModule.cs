namespace GSSAPI.AuthenticationModule
{
    /// <summary>
    /// SPNEGO negotiation via GSS API
    /// </summary>
    public class SpNegoAuthenticationModule : AuthenticationModuleBase
    {
        public const string AuthType = "Negotiate";

        public override GssClientAuth Auth => GssClientAuth.SpNego;
        public override string AuthenticationType => AuthType;
    }
}