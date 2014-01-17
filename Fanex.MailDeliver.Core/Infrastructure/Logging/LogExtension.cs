// --------------------------------------------------------------------------
// <copyright file="LogExtension.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.Core.Infrastructure.Logging
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using log4net;
    using log4net.Config;
    using log4net.Core;

    public enum LoggingMode
    {
        /// <summary>
        /// Console Log.
        /// </summary>
        Console,

        /// <summary>
        /// Service mode.
        /// </summary>
        Service
    }

    /// <summary>
    /// Log extension.
    /// </summary>
    public static class LogExtension
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static LogExtension()
        {
            LoggingMode = LoggingMode.Service;
            BasicConfigurator.Configure();
            XmlConfigurator.Configure();
        }

        public static LoggingMode LoggingMode { get; set; }

        /// <summary>
        /// Initialize Logging.
        /// </summary>
        /// <param name="mode">Logging model.</param>
        public static void InitLogging(LoggingMode mode)
        {
            LoggingMode = mode;
        }

        /// <summary>
        /// Log Error information.
        /// </summary>
        /// <param name="ex">Exception data.</param>
        public static void LogError(Exception ex)
        {
            if (LoggingMode.Equals(LoggingMode.Console))
            {
                Console.WriteLine(string.Format("Error: {0}", ex.ToString()));
                WriteLog(Level.Error, ex.ToString());
            }
            else
            {
                //// nLog
                WriteLog(Level.Error, ex.ToString());
                //// Fannex Logging
                Fanex.Logging.Logger.Log.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// Log Message.
        /// </summary>
        /// <param name="message">Message information.</param>
        public static void LogMessage(string message)
        {
            if (LoggingMode.Equals(LoggingMode.Console))
            {
                Console.WriteLine(string.Format("Message: {0}", message));
            }
            else
            {
                WriteLog(Level.Info, message);
            }
        }

        /// <summary>
        /// Write Log.
        /// </summary>
        /// <param name="logLevel">Log Level.</param>
        /// <param name="log">Message information.</param>
        public static void WriteLog(Level logLevel, object log)
        {
            // write asynchronous
            Task.Factory.StartNew(() =>
            {
                if (logLevel.Equals(Level.Debug))
                {
                    Logger.Debug(log);
                }
                else if (logLevel.Equals(Level.Error))
                {
                    Logger.Error(log);
                }
                else if (logLevel.Equals(Level.Fatal))
                {
                    Logger.Fatal(log);
                }
                else if (logLevel.Equals(Level.Info))
                {
                    Logger.Info(log);
                }
                else if (logLevel.Equals(Level.Warn))
                {
                    Logger.Warn(log);
                }
            });
        }
    }
}