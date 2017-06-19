using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using GSSAPI.Native;
using GSSAPI.Utility;

namespace GSSAPI
{
    /// <summary>
    /// GSS API client library
    /// </summary>
    public class GssClient : IDisposable
    {
        /// <summary>
        /// Current state
        /// </summary>
        private GssClientState _state;

        /// <summary>
        /// Current token (created by last call to NextToken)
        /// </summary>
        public string Token => _state?.Token;

        /// <summary>
        /// Flag if authentication is completed
        /// </summary>
        public bool Completed => _state?.Completed ?? false;

        /// <summary>
        /// Initializes authentication client
        /// </summary>
        /// <param name="service">Service name</param>
        /// <param name="flags">Request flags</param>
        /// <param name="auth">Authentication mechanism</param>
        /// <returns>Initialized client</returns>
        public static GssClient Init(string service, GssContextFlags flags, GssClientAuth auth = GssClientAuth.SpNego)
        {
            var client = new GssClient();
            client.InitState(service, flags, auth);
            return client;
        }

        /// <summary>
        /// Initializes authentication client for HTTP service
        /// </summary>
        /// <param name="host">Host name</param>
        /// <param name="auth">Authentication mechanism</param>
        /// <returns>Initialized client</returns>
        public static GssClient InitHttp(string host, GssClientAuth auth = GssClientAuth.SpNego)
        {
            return Init("HTTP@" + host, GssContextFlags.None, auth);
        }

        /// <summary>
        /// Initializes client, creates "state"
        /// </summary>
        /// <param name="service">Service name</param>
        /// <param name="flags">Request flags</param>
        /// <param name="auth">Authentication mechanism</param>
        public void InitState(string service, GssContextFlags flags, GssClientAuth auth = GssClientAuth.SpNego)
        {
            if (string.IsNullOrWhiteSpace(service))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(service));

            TraceLog.WriteLine($"[{GetHashCode():X8}] GssClient.InitState service:{service} flags:{flags} auth:{auth}");

            var state = new GssClientState
            {
                Service = service,
                Context = IntPtr.Zero,
                ServerName = IntPtr.Zero,
                Auth = auth,
                Flags = flags,
                Completed = false,
                Token = null
            };

            uint minStat = 0;
            uint majStat = 0;

            using (var nameToken = Cvt.GetBufferFromString(service))
            using (var inputNameType = Const.GetGssKrb5NtServiceName())
                majStat = NativeMethods.gss_import_name(ref minStat, ref nameToken.Value, ref inputNameType.Value, ref state.ServerName);
            Gss.CheckAndThrow(majStat, minStat, $"gss_import_name failed for {service}!");

            TraceLog.WriteLine($"[{GetHashCode():X8}] GssClient.InitState gss_import_name majStat:{majStat:X8} minStat:{minStat:X8} serverName:{state.ServerName}");

            _state = state;
        }

        /// <summary>
        /// Processes 
        /// </summary>
        /// <param name="challenge"></param>
        /// <returns></returns>
        public string NextToken(string challenge)
        {
            if (_state == null)
                throw new ArgumentNullException(nameof(_state));
            if (challenge == null)
                throw new ArgumentNullException(nameof(challenge));

            challenge = challenge.Trim();

            TraceLog.WriteLine($"[{GetHashCode():X8}] GssClient.NextToken challenge:{challenge}");

            uint minStat = 0;
            uint majStat = 0;

            var outputToken = Cvt.GetEmptyBuffer();

            try
            {
                using (var mechType = Gss.GetAuthOid(_state.Auth))
                using (var inputToken = Cvt.GetBufferFromBase64StringOrEmpty(challenge))
                    majStat = NativeMethods.gss_init_sec_context(
                        ref minStat,
                        IntPtr.Zero,
                        ref _state.Context,
                        _state.ServerName,
                        ref mechType.Value,
                        (uint)_state.Flags,
                        Const.GssIndefinite,
                        IntPtr.Zero,
                        ref inputToken.Value,
                        IntPtr.Zero,
                        ref outputToken,
                        IntPtr.Zero,
                        IntPtr.Zero);
                Gss.CheckAndThrow(majStat, minStat, $"gss_init_sec_context failed for {_state.Service}!");

                TraceLog.WriteLine($"[{GetHashCode():X8}] GssClient.NextToken gss_init_sec_context majStat:{majStat:X8} minStat:{minStat:X8} challenge:{challenge} outputToken:{outputToken.length}");

                if (outputToken.length > 0)
                {
                    var buffer = new byte[outputToken.length];
                    Marshal.Copy(outputToken.value, buffer, 0, (int)outputToken.length);
                    _state.Token = Convert.ToBase64String(buffer);

                    TraceLog.WriteLine($"[{GetHashCode():X8}] GssClient.NextToken gss_init_sec_context majStat:{majStat:X8} minStat:{minStat:X8} challenge:{challenge} outputToken:{outputToken.length} token:{_state.Token}");
                }

                if (majStat == Const.GssComplete)
                    _state.Completed = true;

                return _state.Token;
            }
            finally
            {
                if (outputToken.value != IntPtr.Zero)
                    majStat = NativeMethods.gss_release_buffer(ref minStat, ref outputToken);
            }
        }

        /// <summary>
        /// Cleanups unmanaged resources
        /// </summary>
        public void Cleanup()
        {
            uint minStat = 0;
            uint majStat = 0;

            if (_state.Context != IntPtr.Zero)
                majStat = NativeMethods.gss_delete_sec_context(ref minStat, ref _state.Context, IntPtr.Zero);

            if (_state.ServerName != IntPtr.Zero)
                majStat = NativeMethods.gss_release_name(ref minStat, ref _state.ServerName);
        }

        private void ReleaseUnmanagedResources()
        {
            Cleanup();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~GssClient()
        {
            ReleaseUnmanagedResources();
        }
    }
}