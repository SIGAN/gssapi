using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using GSSAPI.Utility;

namespace GSSAPI.AuthenticationModule
{
    /// <summary>
    /// Stores pre-authentication information for WebRequests
    /// </summary>
    public sealed class AuthenticationStateStore
    {
        /// <summary>
        /// Current storage
        /// </summary>
        public static readonly AuthenticationStateStore Current = new AuthenticationStateStore();

        /// <summary>
        /// Default time-to-live for tokens
        /// </summary>
        public static readonly TimeSpan TokenTimeToLive = new TimeSpan(0, 1, 0);
        /// <summary>
        /// Default time-to-live for clients
        /// </summary>
        public static readonly TimeSpan ClientTimeToLive = new TimeSpan(0, 0, 10);
        /// <summary>
        /// Cleanup interval
        /// </summary>
        public static readonly TimeSpan CleanupInterval = new TimeSpan(0, 0, 15);

        private class CacheEntry<T>
        {
            public DateTime LastUsed;
            public T Value;
        }

        private Timer _cleanupTimer;

        private ConcurrentDictionary<string, CacheEntry<string>> _tokenCache;
        private ConcurrentDictionary<string, CacheEntry<GssClient>> _clientCache;

        /// <summary>
        /// Initializes store
        /// </summary>
        public void Initialize()
        {
            if (_tokenCache == null)
                _tokenCache = new ConcurrentDictionary<string, CacheEntry<string>>(StringComparer.OrdinalIgnoreCase);

            if (_clientCache == null)
                _clientCache = new ConcurrentDictionary<string, CacheEntry<GssClient>>(StringComparer.OrdinalIgnoreCase);

            if (_cleanupTimer == null)
                _cleanupTimer = new Timer(CleanupCallback, null, CleanupInterval, CleanupInterval);
        }

        /// <summary>
        /// Returns safe keys for WebRequest cache
        /// </summary>
        private string GetKey(string requestId, string host, string userName)
        {
            return string.Concat(requestId ?? "-", ":", host, ":", userName);
            //return string.Concat(host, ":", userName);
        }

        /// <summary>
        /// Returns safe keys for client cache
        /// </summary>
        private string GetKey(string requestId, string host, string userName, GssClientAuth auth)
        {
            return string.Concat(requestId ?? "-", ":", host, ":", userName, ":", auth);
            //return string.Concat(host, ":", userName, ":", auth);
        }

        /// <summary>
        /// Returns token used last for the <paramref name="requestId"/> 
        /// </summary>
        public string GetToken(string requestId, string host, string userName)
        {
            if (_tokenCache == null)
                return null;

            var key = GetKey(requestId, host, userName);

            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationStateStore.GetToken requestId:{requestId} host:{host} userName:{userName} key:{key}");

            CacheEntry<string> entry;
            if (!_tokenCache.TryGetValue(key, out entry))
            {
                TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationStateStore.GetToken => null");
                return null;
            }

            if (entry != null)
                entry.LastUsed = DateTime.UtcNow;

            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationStateStore.GetToken entry?.Value:{entry?.Value}");

            return entry?.Value;
        }

        /// <summary>
        /// Updates token used last for the <paramref name="requestId"/> 
        /// </summary>
        public void SetToken(string requestId, string host, string userName, string token)
        {
            if (_tokenCache == null)
                return;

            var key = GetKey(requestId, host, userName);

            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationStateStore.SetToken requestId:{requestId} host:{host} userName:{userName} key:{key} token:{token}");

            _tokenCache[key] = new CacheEntry<string> { Value = token, LastUsed = DateTime.UtcNow };
        }

        /// <summary>
        /// Removes token
        /// </summary>
        public void DeleteToken(string requestId, string host, string userName)
        {
            if (_tokenCache == null)
                return;

            var key = GetKey(requestId, host, userName);

            CacheEntry<string> entry;
            _tokenCache.TryRemove(key, out entry);

            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationStateStore.DeleteToken requestId:{requestId} host:{host} userName:{userName} key:{key} entry?.Value:{entry?.Value}");
        }

        /// <summary>
        /// Returns client used last for the <paramref name="requestId"/> or loads using <paramref name="loader"/>
        /// </summary>
        public GssClient GetOrAddClient(string requestId, string host, string userName, GssClientAuth auth, Func<GssClient> loader)
        {
            if (loader == null)
                throw new ArgumentNullException(nameof(loader));

            if (_clientCache == null)
                return loader();

            var key = GetKey(requestId, host, userName, auth);
            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationStateStore.GetOrAddClient requestId:{requestId} host:{host} userName:{userName} auth:{auth} key:{key}");

            var entry = _clientCache.GetOrAdd(
                key,
                k => new CacheEntry<GssClient> { Value = loader(), LastUsed = DateTime.UtcNow });

            if (entry != null)
                entry.LastUsed = DateTime.UtcNow;

            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationStateStore.GetOrAddClient requestId:{requestId} host:{host} userName:{userName} auth:{auth} entry?.Value:{entry?.Value.GetHashCode():X8}");

            return entry?.Value;
        }

        /// <summary>
        /// Removes client
        /// </summary>
        public void DeleteClient(string requestId, string host, string userName, GssClientAuth auth)
        {
            if (_clientCache == null)
                return;

            var key = GetKey(requestId, host, userName, auth);

            CacheEntry<GssClient> entry;
            _clientCache.TryRemove(key, out entry);

            TraceLog.WriteLine($"[{GetHashCode():X8}] AuthenticationStateStore.DeleteClient requestId:{requestId} host:{host} userName:{userName} auth:{auth} key:{key} entry?.Value:{entry?.Value.GetHashCode():X8}");

            entry?.Value?.Dispose();
        }

        private volatile bool _cleaningUp = false;

        /// <summary>
        /// Cleanups old requests, non-blocking implementation
        /// </summary>
        private void CleanupCallback(object state)
        {
            // overload protection
            if (_cleaningUp)
                return;

            try
            {
                _cleaningUp = true;

                CleanupTokens(DateTime.UtcNow.Subtract(TokenTimeToLive));
                CleanupClients(DateTime.UtcNow.Subtract(ClientTimeToLive));
            }
            finally
            {
                _cleaningUp = false;
            }
        }

        private void CleanupTokens(DateTime cleanUpBefore)
        {
            if (_tokenCache == null)
                return;

            var toRemove = default(List<string>);
            foreach (var entry in _tokenCache)
            {
                if (entry.Value.LastUsed >= cleanUpBefore)
                    continue;

                if (toRemove == null)
                    toRemove = new List<string>();
                toRemove.Add(entry.Key);
            }

            if (toRemove == null)
                return;

            foreach (var key in toRemove)
            {
                CacheEntry<string> entry;
                _tokenCache.TryRemove(key, out entry);
            }
        }

        private void CleanupClients(DateTime cleanUpBefore)
        {
            if (_clientCache == null)
                return;

            var toRemove = default(List<string>);
            foreach (var entry in _clientCache)
            {
                if (entry.Value.LastUsed >= cleanUpBefore)
                    continue;

                if (toRemove == null)
                    toRemove = new List<string>();
                toRemove.Add(entry.Key);
            }

            if (toRemove == null)
                return;

            foreach (var key in toRemove)
            {
                CacheEntry<GssClient> entry;
                _clientCache.TryRemove(key, out entry);

                entry?.Value?.Dispose();
            }
        }

        public void Close()
        {
            _cleanupTimer?.Dispose();
            _cleanupTimer = null;

            ClearTokens();
            ClearClients();
        }

        public void ClearClients()
        {
            CleanupClients(DateTime.MaxValue);
        }

        public void ClearTokens()
        {
            CleanupTokens(DateTime.MaxValue);
        }
    }
}