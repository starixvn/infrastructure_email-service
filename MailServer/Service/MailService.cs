// --------------------------------------------------------------------------
// <copyright file="MailService.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.MailServer.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fanex.MailDeliver.Business;
    using Fanex.MailDeliver.Business.Implement;
    using Fanex.MailDeliver.Business.Interface;
    using Fanex.MailDeliver.Business.Worker;
    using Fanex.MailDeliver.Core.Business.Worker;
    using Fanex.MailDeliver.Core.Infrastructure.Configurations;
    using Fanex.MailDeliver.MailClient.Util;
    using Fanex.MailDeliver.Model;

    /// <summary>
    /// Mail service.
    /// </summary>
    public class MailService : IMailService
    {
        private static IMailDeliverBusiness _mailDeliverBusiness;

        public MailService()
        {
            if (_mailDeliverBusiness == null)
            {
                _mailDeliverBusiness = new MailDeliverBusiness();
            }
        }

        public void SendMail(byte[] buffer)
        {
            var record = MailFormatter.Deserialize<MailRecord>(buffer);
            if (record != null)
            {
                record.ResentCount = AppSettings.NumberOfResendMail;
                MailExtension.AddMailToQueueForSendingMail((object)record);
            }
        }

        public byte[] StoreConfig(byte[] buffer)
        {
            return _mailDeliverBusiness.SaveMailConfig(buffer);
        }

        public byte[] GetMail(long mailID)
        {
            return _mailDeliverBusiness.GetMailRecordByStream(mailID);
        }

        public IEnumerable<byte[]> GetListMail(
            int appID,
            DateTime mailDate,
            int page,
            int counter)
        {
            return _mailDeliverBusiness.GetListMailRecordByDate(appID, mailDate, page, counter);
        }

        public IEnumerable<byte[]> GetListMailByDate(
            int appID,
            DateTime fromDate,
            DateTime toDate,
            ref int totalRows,
            int page,
            int counter,
            int status)
        {
            return _mailDeliverBusiness.GetListMailRecordByDateRange(appID, fromDate, toDate, ref totalRows, page, counter, status);
        }

        public byte[] GetConfig(int appID)
        {
            return _mailDeliverBusiness.GetMailConfig(appID);
        }

        public byte[] StoreSetting(byte[] buffer)
        {
            return _mailDeliverBusiness.SaveMailSetting(buffer);
        }

        public byte[] GetSetting(int settingID)
        {
            return _mailDeliverBusiness.GetMailSetting(settingID);
        }

        /// <summary>
        /// Resend mail when mail failed.
        /// </summary>
        /// <param name="mailID">Mail id need be resent.</param>
        public void ReSendMail(long mailID)
        {
            try
            {
                lock (ResendMailWorker.ListMailLockObject)
                {
                    if (ResendMailWorker.ListMailFailed.Any())
                    {
                        MailRecord mailRemoveFromResendList = null;
                        foreach (var mail in ResendMailWorker.ListMailFailed)
                        {
                            if (mail.MailID == mailID)
                            {
                                mailRemoveFromResendList = mail;
                                break;
                            }
                        }

                        if (mailRemoveFromResendList != null)
                        {
                            ResendMailWorker.ListMailFailed.Remove(mailRemoveFromResendList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Fanex.Logging.Logger.Log.Error(ex.Message, ex);
            }

            (new SendMailWorker()).SendMail(
                new MailRecord
                {
                    MailID = mailID,
                    IsReady = false
                });
        }

        #region mail function service

        public byte[] StoreFunction(byte[] buffer)
        {
            return _mailDeliverBusiness.SaveMailFunction(buffer);
        }

        public byte[] GetFunction(int funcID)
        {
            return _mailDeliverBusiness.GetMailFunction(funcID);
        }

        #endregion mail function service

        #region get list

        public IEnumerable<byte[]> GetListSetting()
        {
            return _mailDeliverBusiness.GetListMailSetting();
        }

        public IEnumerable<byte[]> GetListConfig(string serverIp)
        {
            return _mailDeliverBusiness.GetListMailConfig(serverIp);
        }

        public IEnumerable<byte[]> GetListFuntion()
        {
            return _mailDeliverBusiness.GetListMailFuntion();
        }

        public IEnumerable<byte[]> GetListError(long mailID)
        {
            return _mailDeliverBusiness.GetListMailError(mailID);
        }

        #endregion get list
    }
}