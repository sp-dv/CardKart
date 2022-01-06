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
                logger.Log(level, $"{timestamp} ({levelString}): {message}");
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
        void Log(LogLevel level, string message);
    }

    class ConsoleLogger : Logger
    {
        public void Log(LogLevel level, string message)
        {
            Console.ForegroundColor = new Func<ConsoleColor>(() =>
            {
                switch (level)
                {
                    case LogLevel.Debug: return ConsoleColor.Green;
                    case LogLevel.Info: return ConsoleColor.Cyan;
                    case LogLevel.Warning: return ConsoleColor.Yellow;
                    case LogLevel.Error: return ConsoleColor.Red;

                    default: return ConsoleColor.White;
                }
            })();
            
            Console.WriteLine(message);

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
