// --------------------------------------------------------------------------
// <copyright file="SendMailWorker.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.Business.Worker
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Messaging;
    using System.Threading;
    using Fanex.MailDeliver.Business;
    using Fanex.MailDeliver.Business.Implement;
    using Fanex.MailDeliver.Business.Interface;
    using Fanex.MailDeliver.Core.Business.Implement;
    using Fanex.MailDeliver.Core.Business.Interface;
    using Fanex.MailDeliver.Core.Infrastructure.Configurations;
    using Fanex.MailDeliver.Core.Infrastructure.Logging;
    using Fanex.MailDeliver.Model;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SendMailWorker
    {
        private static IMailDeliverBusiness _mailDeliverBusiness;
        private static IMaintenanceService _maintenanceService;

        /// <summary>
        /// Is Break or not.
        /// </summary>
        private bool _isBreak = false;

        public SendMailWorker()
        {
            _mailDeliverBusiness = new MailDeliverBusiness();
            _maintenanceService = new MaintenanceService();
        }

        public bool IsBreak
        {
            get
            {
                return _isBreak;
            }

            set
            {
                _isBreak = value;
            }
        }

        /// <summary>
        /// Read from Queue, if has data, then parse to MailRecord, if MailId > 0 then ReSend mail
        /// else Store to Database and then send mail.
        /// </summary>
        /// <param name="fromService">Run from service or not.</param>
        [SuppressMessage(
            "StyleCop.CSharp.Nexcel.NexcelCustomRules",
            "SP2101:MethodMustNotContainMoreLinesThan",
            Justification = "Reviewed. Suppression is OK here.")]
        public void Run(bool fromService = false)
        {
            if (fromService)
            {
                LogExtension.LogMessage(string.Format("MSMQ worker's running at {0} on {1}", AppSettings.QueuePath, DateTime.Now));
            }

            try
            {
                var msmq = new MessageQueue(AppSettings.QueuePath, QueueAccessMode.SendAndReceive);
                //// Set the formatter to binary
                msmq.Formatter = new BinaryMessageFormatter();

                MailRecord data = null;
                ////MailRecord storeData = null;
                Message message = null;

                while (false == IsBreak)
                {
                    try
                    {
                        var downTime = _maintenanceService.GetDowntimeInfo();

                        if (!downTime.IsDowntime)
                        {
                            //// do not use timeout anymore
                            message = msmq.Receive();
                            data = message.Body as MailRecord;
                            if (data != null)
                            {
                                downTime = _maintenanceService.GetDowntimeInfo();

                                if (!downTime.IsDowntime)
                                {
                                    if (data.MailID > 0)
                                    {
                                        data.IsReady = true;
                                        SendMail(data);
                                    }
                                    else
                                    {
                                        //// TODO
                                        var mailId = 0;

                                        mailId = (int)_mailDeliverBusiness.InsertMailRecord(data);

                                        if (mailId > 0)
                                        {
                                            LogExtension.LogMessage(string.Format("Stored mail {0}", mailId));
                                            data.IsReady = true;
                                            data.MailID = mailId;
                                            SendMail(data);
                                        }
                                        else
                                        {
                                            LogExtension.LogMessage("Store mail failed.");
                                        }

                                        Thread.Sleep(1);
                                    }
                                }
                                else
                                {
                                    MailExtension.AddMailToQueueForSendingMail(data);
                                    Thread.Sleep(5000);
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(5000);
                        }
                    }
                    catch (MessageQueueException ex)
                    {
                        if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                        {
                            LogExtension.LogError(ex);
                        }

                        Thread.Sleep(5000);
                    }
                    catch (Exception ex)
                    {
                        LogExtension.LogError(ex);
                        Thread.Sleep(5000);
                    }
                }

                msmq.Close();
                GC.Collect();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        /// <summary>
        /// Send mail.
        /// </summary>
        /// <param name="record">Mail information.</param>
        public void SendMail(MailRecord record)
        {
            record = record.IsReady ? record : _mailDeliverBusiness.GetMailRecord(record.MailID, true);
            if (record != null && record.IsReady)
            {
                string error = string.Empty;
                MailExtension.SendMail(record, out error);
                LogExtension.LogMessage(string.Format("Sending mail: {0}. Error message: {1}", record.MailID, error));
            }
        }
    }
}