using System;
using System.Collections.Generic;

namespace CardKartShared.Util
{
    public static class Logging
    {
        private static List<Logger> Loggers = new List<Logger>();

        public static void Log(LogLevel level, string message)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH:mm:ss");
            var levelString = level.ToString();

            foreach (var logger in Loggers)
            {
                logger.Log($"{timestamp} ({levelString}): {message}");
            }
        }

        public static void AddConsoleLogger()
        {
            Loggers.Add(new ConsoleLogger());
        }
    }

    public enum LogLevel
    {
        None,

        Debug,
        Info,
        Warning,
        Error,
    }

    interface Logger
    {
        void Log(string message);
    }

    class ConsoleLogger : Logger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
