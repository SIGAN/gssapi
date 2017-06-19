using System.Diagnostics;

namespace GSSAPI.Utility
{
    public static class TraceLog
    {
        public static bool LoggingEnabled = false;

        public static void WriteLine(string message)
        {
            if (LoggingEnabled)
                Trace.WriteLine(message);
        }
    }
}