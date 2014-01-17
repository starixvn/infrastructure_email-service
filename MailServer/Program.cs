// --------------------------------------------------------------------------
// <copyright file="Program.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.MailServer
{
    using System;
    using System.Reflection;
    using System.ServiceProcess;
    using Fanex.MailDeliver.Core.Infrastructure.Logging;
    using Fanex.MailDeliver.MailServer.Service;

    /// <summary>
    /// Main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main function.
        /// </summary>
        /// <param name="args">Function arguments.</param>
        public static void Main(string[] args)
        {
            ServiceBase[] servicesToRun = new ServiceBase[]
            {
                new WinMailService()
            };

            if (Environment.UserInteractive)
            {
                ////LogExtension.LoggingMode = LoggingMode.Console;
                //// run console
                LogExtension.LoggingMode = LoggingMode.Console;
                Type type = typeof(ServiceBase);
                BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                MethodInfo method = type.GetMethod("OnStart", flags);
                foreach (ServiceBase service in servicesToRun)
                {
                    method.Invoke(service, new object[] { args });
                }

                Console.WriteLine("Mail service has been started.");
                Console.WriteLine("Press any key to exit!");
                Console.ReadLine();

                foreach (ServiceBase service in servicesToRun)
                {
                    service.Stop();
                }
            }
            else
            {
                LogExtension.LoggingMode = LoggingMode.Service;
                //// run window service             

                ServiceBase.Run(servicesToRun);
            }
        }
    }
}