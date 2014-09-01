// --------------------------------------------------------------------------
// <copyright file="AppSettings.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.12.26">Created</change>
// </history>

namespace Fanex.MailDeliver.Core.Infrastructure.Configurations
{
    using System;
    using Fanex.MailDeliver.Core.Infrastructure.Logging;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class AppSettings
    {
        static AppSettings()
        {
            try
            {
                QueuePath = System.Configuration.ConfigurationManager.AppSettings["queuePath"];
                NumberOfResendMail = int.Parse(System.Configuration.ConfigurationManager.AppSettings["countResendMail"]);
                IntervalResendMail = int.Parse(System.Configuration.ConfigurationManager.AppSettings["intervalSendFailedMail"]);
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        public static string QueuePath { get; private set; }

        public static int NumberOfResendMail { get; private set; }

        public static int IntervalResendMail { get; private set; }
    }
}