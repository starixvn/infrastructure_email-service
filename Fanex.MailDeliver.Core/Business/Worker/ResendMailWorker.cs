// --------------------------------------------------------------------------
// <copyright file="ResendMailWorker.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2013.11.11">Created</change>
// </history>

namespace Fanex.MailDeliver.Core.Business.Worker
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fanex.MailDeliver.Business;
    using Fanex.MailDeliver.Core.Infrastructure.Configurations;
    using Fanex.MailDeliver.Core.Infrastructure.Logging;
    using Fanex.MailDeliver.Model;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ResendMailWorker
    {
        public ResendMailWorker()
        {
            ListMailFailed = new List<MailRecord>();
            Stop = true;
            ListMailLockObject = new object();
        }

        public static List<MailRecord> ListMailFailed { get; set; }

        public static object ListMailLockObject { get; set; }

        public bool Stop { get; set; }

        /// <summary>
        /// Run thread.
        /// </summary>
        public void Run()
        {
            while (true == Stop)
            {
                Thread.Sleep(AppSettings.IntervalResendMail * 1000 * 60);
                try
                {
                    if (ListMailFailed.Count > 0)
                    {
                        List<MailRecord> listMailFailCopied = null;
                        lock (ListMailLockObject)
                        {
                            listMailFailCopied = new List<MailRecord>(ListMailFailed);
                            ListMailFailed.Clear();
                        }

                        if (listMailFailCopied != null && listMailFailCopied.Count > 0)
                        {
                            for (int mailIndex = 0; mailIndex < listMailFailCopied.Count; mailIndex++)
                            {
                                MailRecord currentMail = listMailFailCopied[mailIndex];
                                if (currentMail.ResentCount > 0)
                                {
                                    listMailFailCopied[mailIndex].ResentCount--;
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(MailExtension.AddMailToQueueForSendingMail), (object)currentMail);
                                    ////LogExtension.LogError(new Exception(string.Format("Fanex Email Service: Application Id {0} was error.", currentMail.AppID)));
                                }
                            }

                            listMailFailCopied = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogExtension.LogError(ex);
                }
            }
        }
    }
}