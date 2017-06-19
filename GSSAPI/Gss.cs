using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using GSSAPI.AuthenticationModule;
using GSSAPI.Native;
using GSSAPI.Utility;

namespace GSSAPI
{
    /// <summary>
    /// Usefull GSS functions
    /// </summary>
    public static class Gss
    {
        /// <summary>
        /// Original Negotiate Module
        /// </summary>
        public static IAuthenticationModule OriginalNegotiateModule;

        /// <summary>
        /// Original Kerberos Module
        /// </summary>
        public static IAuthenticationModule OriginalKerberosModule;

        /// <summary>
        /// Initilizes GSS API and replaces standard authentication modules with GSSAPI based
        /// </summary>
        /// <param name="negotiate">Flag to replace Negotiate module.</param>
        /// <param name="kerberos">Flag to replace Kerberos module.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void InitializeAndOverrideApi(bool negotiate = true, bool kerberos = false)
        {
            AuthenticationStateStore.Current.Initialize();

            if (negotiate && OriginalNegotiateModule == null)
            {
                var targetModule = default(IAuthenticationModule);

                var registeredModules = AuthenticationManager.RegisteredModules;
                while (registeredModules.MoveNext())
                {
                    var module = (IAuthenticationModule)registeredModules.Current;
                    if (string.Equals(module.AuthenticationType, SpNegoAuthenticationModule.AuthType, StringComparison.OrdinalIgnoreCase))
                    {
                        targetModule = module;
                        break;
                    }
                }

                OriginalNegotiateModule = targetModule;

                if (targetModule != null)
                    AuthenticationManager.Unregister(targetModule);

                AuthenticationManager.Register(new SpNegoAuthenticationModule());
            }

            if (kerberos && OriginalKerberosModule == null)
            {
                var targetModule = default(IAuthenticationModule);

                var registeredModules = AuthenticationManager.RegisteredModules;
                while (registeredModules.MoveNext())
                {
                    var module = (IAuthenticationModule)registeredModules.Current;
                    if (string.Equals(module.AuthenticationType, KerberosAuthenticationModule.AuthType, StringComparison.OrdinalIgnoreCase))
                    {
                        targetModule = module;
                        break;
                    }
                }

                OriginalKerberosModule = targetModule;

                if (targetModule != null)
                    AuthenticationManager.Unregister(targetModule);

                AuthenticationManager.Register(new KerberosAuthenticationModule());
            }
        }

        /// <summary>
        /// Terminates GSSAPI execution and removes overrides. Existing instances will continue execution normally. New instances will use old authentication mechanism.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void TerminateAndRemoveOverride()
        {
            if (OriginalNegotiateModule != null)
            {
                var targetModule = default(IAuthenticationModule);

                var registeredModules = AuthenticationManager.RegisteredModules;
                while (registeredModules.MoveNext())
                {
                    var module = (IAuthenticationModule)registeredModules.Current;
                    if (string.Equals(module.AuthenticationType, OriginalNegotiateModule.AuthenticationType, StringComparison.OrdinalIgnoreCase))
                    {
                        targetModule = module;
                        break;
                    }
                }

                if (targetModule != null)
                    AuthenticationManager.Unregister(targetModule);

                AuthenticationManager.Register(OriginalNegotiateModule);

                OriginalNegotiateModule = null;
            }

            if (OriginalKerberosModule != null)
            {
                var targetModule = default(IAuthenticationModule);

                var registeredModules = AuthenticationManager.RegisteredModules;
                while (registeredModules.MoveNext())
                {
                    var module = (IAuthenticationModule)registeredModules.Current;
                    if (string.Equals(module.AuthenticationType, OriginalKerberosModule.AuthenticationType, StringComparison.OrdinalIgnoreCase))
                    {
                        targetModule = module;
                        break;
                    }
                }


                if (targetModule != null)
                    AuthenticationManager.Unregister(targetModule);

                AuthenticationManager.Register(OriginalKerberosModule);

                OriginalKerberosModule = null;
            }

            AuthenticationStateStore.Current.Close();
        }

        /// <summary>
        /// Issues new token for the first request
        /// <remarks>Internally calls ExchangeHttpToken with empty challenge</remarks>
        /// </summary>
        /// <param name="host">host to issue token to</param>
        /// <param name="auth">type of authentication mechanism</param>
        /// <returns>token</returns>
        public static string GetHttpToken(string host, GssClientAuth auth = GssClientAuth.SpNego)
        {
            return ExchangeHttpToken(string.Empty, host, auth);
        }

        /// <summary>
        /// Issues token for the <paramref name="challenge"/>
        /// <remarks>Send empty <paramref name="challenge"/> for the first request</remarks>
        /// </summary>
        /// <param name="challenge">Received challenge or empty string</param>
        /// <param name="host">host to issue token to</param>
        /// <param name="auth">type of authentication mechanism</param>
        /// <returns>token</returns>
        public static string ExchangeHttpToken(string challenge, string host, GssClientAuth auth = GssClientAuth.SpNego)
        {
            using (var client = GssClient.InitHttp(host, auth))
                return client.NextToken(challenge);
        }

        /// <summary>
        /// Extracts challenge from 401 <paramref name="response"/> and ensures it is not <paramref name="authenticationType"/>
        /// </summary>
        public static string GetChallenge(WebResponse response, string authenticationType)
        {
            var value = response.Headers[HttpResponseHeader.WwwAuthenticate];
            return GetChallenge(value, authenticationType);
        }

        /// <summary>
        /// Extracts challenge from <paramref name="authorizationHeaderValue"/> and ensures it is not <paramref name="authenticationType"/>
        /// </summary>
        public static string GetChallenge(string authorizationHeaderValue, string authenticationType)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
                return null;

            var spaceIndx = authorizationHeaderValue.IndexOf(" ", StringComparison.Ordinal);
            if (spaceIndx < 0)
                spaceIndx = -1;

            var commaIdx = authorizationHeaderValue.IndexOf(",", spaceIndx + 1, StringComparison.Ordinal);
            if (commaIdx < 0)
                commaIdx = authorizationHeaderValue.Length;

            var challenge = authorizationHeaderValue.Substring(spaceIndx + 1, commaIdx - spaceIndx - 1).Trim();

            if (string.Equals(challenge, authenticationType, StringComparison.OrdinalIgnoreCase))
                challenge = string.Empty;

            return challenge;
        }

        /// <summary>
        /// Returns if <paramref name="majorStatus"/> is erroneous
        /// </summary>
        /// <param name="majorStatus">Status to check</param>
        /// <returns>true, if error</returns>
        public static bool IsError(uint majorStatus)
        {
            return (majorStatus & Const.GssErrorMask) != 0;
        }

        /// <summary>
        /// Checks status and fails with message and status codes
        /// </summary>
        /// <param name="majStat">Major Status Code</param>
        /// <param name="minStat">Minor Status Code</param>
        /// <param name="message">Exception message</param>
        public static void CheckAndThrow(uint majStat, uint minStat, string message)
        {
            if (IsError(majStat))
                throw new AuthenticationException($"{message} Status codes major: {majStat} minor: {minStat}!");
        }

        /// <summary>
        /// Returns authentication mechanism OID
        /// </summary>
        /// <param name="auth">Authentication mechanism</param>
        /// <returns>GSS OID</returns>
        public static AutoDisposing<GssOidDescStruct> GetAuthOid(GssClientAuth auth)
        {
            switch (auth)
            {
                case GssClientAuth.SpNego:
                    return Const.GetGssSpnegoMechOidDesc();

                case GssClientAuth.Kerberos:
                    return Const.GetGssKrb5MechOidDesc();

                default:
                    throw new ArgumentOutOfRangeException(nameof(auth), auth, null);
            }
        }
    }
}
