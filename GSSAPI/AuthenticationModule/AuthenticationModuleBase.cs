using System;
using System.Net;
using System.Security.Principal;
using GSSAPI.Utility;

namespace GSSAPI.AuthenticationModule
{
    /// <summary>
    /// GSS Authentication Module Base
    /// </summary>
    public abstract class AuthenticationModuleBase : IAuthenticationModule
    {
        public bool CanPreAuthenticate => true;

        public abstract GssClientAuth Auth { get; }
        public abstract string AuthenticationType { get; }

        public Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials)
        {
            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.Authenticate challenge:{challenge} request:{request.GetHashCode():X8} credentials:{credentials.GetHashCode():X8}");
            return DoAuthenticate(challenge, request, credentials, false);
        }

        public Authorization PreAuthenticate(WebRequest request, ICredentials credentials)
        {
            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.PreAuthenticate request:{request.GetHashCode():X8} credentials:{credentials.GetHashCode():X8}");
            return DoAuthenticate(string.Empty, request, credentials, true);
        }

        private Authorization DoAuthenticate(string challenge, WebRequest request, ICredentials credentials, bool preAuthenticate)
        {
            if (request == null)
                return null;

            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate challenge:{challenge} request:{request.GetHashCode():X8} credentials:{credentials.GetHashCode():X8} preAuthenticate:{preAuthenticate}");

            var networkCredential = credentials?.GetCredential(request.RequestUri, AuthenticationType);
            var userName = networkCredential?.UserName ?? WindowsIdentity.GetCurrent().Name;

            if (string.IsNullOrWhiteSpace(userName))
                userName = WindowsIdentity.GetCurrent().Name;

            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate networkCredential:{networkCredential?.UserName} @ {networkCredential?.Domain} userName:{userName}");

            if (challenge == null)
                challenge = string.Empty;

            var uri = request.RequestUri;
            var host = uri.Host;
            var requestId =
                ((request as HttpWebRequest)?.UnsafeAuthenticatedConnectionSharing ?? false)
                    ? "SHARED"
                    : request.GetHashCode().ToString("X8");

            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate host:{host} requestId:{requestId}");

            var finished = true;

            // pre-auth - read-only!
            // !pre-auth - writes!
            // pre-auth called before query!
            // !pre-auth called on 401 after query!

            string token;

            if (preAuthenticate)
            {
                token = AuthenticationStateStore.Current.GetToken(requestId, host, userName);

                TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate preAuthenticate token:{token}");
            }
            else
            {
                TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate authenticate");

                finished = false;

                var policy = AuthenticationManager.CredentialPolicy;
                if (policy != null && !policy.ShouldSendCredential(uri, request, networkCredential, this))
                {
                    TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate policy => null");
                    return null;
                }

                var client = AuthenticationStateStore.Current.GetOrAddClient(requestId,
                    host,
                    userName,
                    Auth,
                    () => GssClient.InitHttp(host, Auth));
                TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate client:{client.GetHashCode():X8}");

                var incomingToken = Gss.GetChallenge(challenge, AuthenticationType);
                TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate incomingToken:{incomingToken}");

                var nextToken = client.NextToken(incomingToken);
                TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate nextToken:{nextToken}");

                if (client.Completed)
                {
                    TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate Completed");

                    finished = true;
                    AuthenticationStateStore.Current.SetToken(requestId, host, userName, nextToken);
                    AuthenticationStateStore.Current.DeleteClient(requestId, host, userName, Auth);
                }

                token = nextToken;
            }

            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate token:{token}");
            if (string.IsNullOrWhiteSpace(token))
            {
                TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate token => null");
                return null;
            }

            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationModuleBase.DoAuthenticate Message:{AuthenticationType + " " + token}");
            return new Authorization(AuthenticationType + " " + token, finished)
            {
                MutuallyAuthenticated = true
            };
        }
    }
}