using System;
using System.Diagnostics;
using StardewModdingAPI;

namespace SpaceShared
{
    internal class Log
    {
        public static IMonitor Monitor;

        public static bool IsVerbose => Monitor.IsVerbose;

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public static void DebugOnlyLog(string str)
        {
            Log.Monitor.Log(str, LogLevel.Debug);
        }

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public static void DebugOnlyLog(string str, bool pred)
        {
            if (pred)
                Log.Monitor.Log(str, LogLevel.Debug);
        }

        [DebuggerHidden]
        public static void Verbose(string str)
        {
            Log.Monitor.VerboseLog(str);
        }

        /// <summary>
        /// Delegate form for verbose logging.
        /// </summary>
        /// <param name="del">Delegate that provides a string to log.</param>
        [DebuggerHidden]
        public static void Verbose(Func<string> del)
        {
            if (Log.Monitor.IsVerbose)
                Log.Trace(del());
        }

        [DebuggerHidden]
        public static void Trace(string str)
        {
            Log.Monitor.Log(str, LogLevel.Trace);
        }

        [DebuggerHidden]
        public static void Debug(string str)
        {
            Log.Monitor.Log(str, LogLevel.Debug);
        }

        [DebuggerHidden]
        public static void Info(string str)
        {
            Log.Monitor.Log(str, LogLevel.Info);
        }

        [DebuggerHidden]
        public static void Warn(string str)
        {
            Log.Monitor.Log(str, LogLevel.Warn);
        }

        [DebuggerHidden]
        public static void Error(string str)
        {
            Log.Monitor.Log(str, LogLevel.Error);
        }
    }
}
