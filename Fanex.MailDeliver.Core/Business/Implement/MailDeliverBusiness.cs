// --------------------------------------------------------------------------
// <copyright file="MailDeliverBusiness.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.Business.Implement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Common;
    using Fanex.MailDeliver.Business.Interface;
    using Fanex.MailDeliver.Business.TableCaching;
    using Fanex.MailDeliver.Core.Criterias;
    using Fanex.MailDeliver.Core.Infrastructure.Logging;
    using Fanex.MailDeliver.DataAccess.Implement;
    using Fanex.MailDeliver.DataAccess.Interface;
    using Fanex.MailDeliver.MailClient.Util;
    using Fanex.MailDeliver.Model;
    using Fanex.NAP.Framework.Data;
    using log4net.Core;

    /// <summary>
    /// Handle Management Mail.
    /// </summary>
    public class MailDeliverBusiness : IMailDeliverBusiness
    {
        private IMailDeliverDataAccess _mailDeliverDataAccess;
        private DynamicRepository _mailDynamicRepository;

        public MailDeliverBusiness(IMailDeliverDataAccess mailDeliverDataAccess = null)
        {
            if (mailDeliverDataAccess == null)
            {
                _mailDeliverDataAccess = new MailDeliverDataAccess();
            }

            _mailDynamicRepository = new DynamicRepository();
        }

        /// <summary>
        /// Insert Mail to Database.
        /// </summary>
        /// <param name="mailRecord">Mail Record Contains mail information.</param>
        /// <returns>Mail Record ID after insert success.</returns>
        public long InsertMailRecord(MailRecord mailRecord)
        {
            try
            {
                if ((mailRecord.Config != null && mailRecord.Config.AppID <= 0) && mailRecord.AppID <= 0)
                {
                    return 0;
                }

                //// Get Mail Config
                var currentAppId = mailRecord.Config != null ? mailRecord.Config.AppID : mailRecord.AppID;
                var currentApp = _mailDeliverDataAccess.GetMailConfig(currentAppId);
                if (currentApp != null && currentApp.IsEncrypt)
                {
                    mailRecord.IsEncrypt = currentApp.IsEncrypt;
                }

                return _mailDeliverDataAccess.InsertMailRecord(mailRecord);
            }
            catch (Exception ex)
            {
                //// Log Email Error
                //// nLog
                LogExtension.WriteLog(Level.Error, ex.ToString());
                //// Fannex Logging
                Fanex.Logging.Logger.Log.Error(ex.Message + " - " + mailRecord.Subject, ex);
            }

            return 0;
        }

        /// <summary>
        /// Get Mail from Mail ID.
        /// </summary>
        /// <param name="mailID">Mail need get data id.</param>
        /// <returns>Mail Record contains information.</returns>
        public byte[] GetMailRecordByStream(long mailID)
        {
            try
            {
                if (mailID <= 0)
                {
                    return null;
                }

                var record = _mailDeliverDataAccess.GetMailRecord(mailID, false);

                if (record != null)
                {
                    var stream = MailFormatter.Serialize(record);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Get Record from Database.
        /// </summary>
        /// <param name="mailID">Mail Record ID.</param>
        /// <param name="isSend">Is sent or not, if sent, will get attachment file.</param>
        /// <returns>Mail Record.</returns>
        public MailRecord GetMailRecord(long mailID, bool isSend = false)
        {
            try
            {
                var mailRecord = _mailDeliverDataAccess.GetMailRecord(mailID, isSend);

                if (isSend && mailRecord.IsEncrypt)
                {
                    mailRecord.MailTo = Cryptography.CfsDeCrypt(mailRecord.MailTo);
                    mailRecord.MailCc = Cryptography.CfsDeCrypt(mailRecord.MailCc);
                    mailRecord.MailBcc = Cryptography.CfsDeCrypt(mailRecord.MailBcc);
                    mailRecord.Body = Cryptography.CfsDeCrypt(mailRecord.Body);
                }

                return mailRecord;
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Save Config of App send mail.
        /// </summary>
        /// <param name="buffer">Mail Config information.</param>
        /// <returns>Mail config id.</returns>
        public byte[] SaveMailConfig(byte[] buffer)
        {
            try
            {
                var config = MailFormatter.Deserialize<MailConfig>(buffer);
                config = _mailDeliverDataAccess.SaveMailConfig(config);
                if (config != null)
                {
                    MemInstance.Set<MemConfig>(config);
                }

                return MailFormatter.Serialize(config).ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Get List Mail.
        /// </summary>
        /// <param name="appID">Application ID.</param>
        /// <param name="mailDate">Date get Mail.</param>
        /// <param name="page">Number of Page.</param>
        /// <param name="counter">Number per page.</param>
        /// <returns>List Mail Record.</returns>
        public IEnumerable<byte[]> GetListMailRecordByDate(
            int appID,
            DateTime mailDate,
            int page,
            int counter)
        {
            try
            {
                List<byte[]> mailList = new List<byte[]>();
                if (appID <= 0)
                {
                    return mailList;
                }

                var records = _mailDeliverDataAccess.GetListMailRecordByDate(appID, mailDate, page, counter);
                if (records != null)
                {
                    foreach (var record in records)
                    {
                        var stream = MailFormatter.Serialize(record);
                        mailList.Add(stream.ToArray());
                    }
                }

                return mailList;
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Get List Mail from date to date.
        /// </summary>
        /// <param name="appID">Application ID.</param>
        /// <param name="fromDate">From Date query.</param>
        /// <param name="toDate">To Date query.</param>
        /// <param name="totalRows">Total rows.</param>
        /// <param name="page">Page Number.</param>
        /// <param name="counter">Record per page.</param>
        /// <param name="status">Mail status.</param>
        /// <returns>List Mail Record.</returns>
        public IEnumerable<byte[]> GetListMailRecordByDateRange(
            int appID,
            DateTime fromDate,
            DateTime toDate,
            ref int totalRows,
            int page,
            int counter,
            int status)
        {
            try
            {
                List<byte[]> mailList = new List<byte[]>();
                if (appID < 0)
                {
                    return mailList;
                }

                var records = _mailDeliverDataAccess.GetListMailRecordByDateRange(appID, fromDate, toDate, ref totalRows, page, counter, status);

                if (records != null)
                {
                    foreach (var record in records)
                    {
                        var stream = MailFormatter.Serialize(record);
                        mailList.Add(stream.ToArray());
                    }
                }

                return mailList;
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Get Config for send mail.
        /// </summary>
        /// <param name="appID">Application Id.</param>
        /// <returns>Mail Config.</returns>
        public byte[] GetMailConfig(int appID)
        {
            try
            {
                var config = _mailDeliverDataAccess.GetMailConfig(appID);
                if (config != null)
                {
                    var stream = MailFormatter.Serialize(config);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        #region mail setting

        /// <summary>
        /// Save Mail Setting.
        /// </summary>
        /// <param name="buffer">Mail setting information.</param>
        /// <returns>Mail setting after save success.</returns>
        public byte[] SaveMailSetting(byte[] buffer)
        {
            try
            {
                var setting = MailFormatter.Deserialize<MailSetting>(buffer);
                setting = _mailDeliverDataAccess.SaveMailSetting(setting);
                if (setting != null)
                {
                    MemInstance.Set<MemSetting>(setting);
                }

                return MailFormatter.Serialize(setting).ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Get Setting from Setting ID.
        /// </summary>
        /// <param name="settingID">Setting ID.</param>
        /// <returns>Mail Setting.</returns>
        public byte[] GetMailSetting(int settingID)
        {
            try
            {
                if (settingID <= 0)
                {
                    return null;
                }

                var mailSetting = MemInstance.Get<MemSetting>(settingID) as MailSetting;
                if (mailSetting == null)
                {
                    mailSetting = _mailDeliverDataAccess.GetMailSetting(settingID);
                }

                var stream = MailFormatter.Serialize(mailSetting);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        #endregion mail setting

        #region mail function

        /// <summary>
        /// Save setting.
        /// </summary>
        /// <param name="buffer">Mail function.</param>
        /// <returns>Mail function return.</returns>
        public byte[] SaveMailFunction(byte[] buffer)
        {
            try
            {
                var func = MailFormatter.Deserialize<MailFunction>(buffer);
                func = _mailDeliverDataAccess.SaveMailFunction(func);
                if (func != null)
                {
                    MemInstance.Set<MemFunction>(func);
                }

                return MailFormatter.Serialize(func).ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Get Function from Function ID.
        /// </summary>
        /// <param name="funcID">Function ID.</param>
        /// <returns>Mail Function.</returns>
        public byte[] GetMailFunction(int funcID)
        {
            try
            {
                if (funcID <= 0)
                {
                    return null;
                }

                var mailFunc = MemInstance.Get<MemFunction>(funcID) as MailFunction;
                if (mailFunc == null)
                {
                    mailFunc = _mailDeliverDataAccess.GetMailFunction(funcID);
                }

                var stream = MailFormatter.Serialize(mailFunc);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        #endregion mail function

        #region count sent

        /// <summary>
        /// Update Mail status after send mail.
        /// </summary>
        /// <param name="mailID">Mail ID need to update.</param>
        /// <param name="status">Send mail status.</param>
        public void UpdateMailStatus(long mailID, bool status)
        {
            try
            {
                _mailDeliverDataAccess.UpdateMailStatus(mailID, status);
                LogExtension.LogMessage(string.Format("Sent mail {0}. Status {1}", mailID, status));
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        #endregion count sent

        #region get list for mem

        /// <summary>
        /// Get List Setting for send mail.
        /// </summary>
        /// <returns>Return List Mail Setting.</returns>
        public IEnumerable<byte[]> GetListMailSetting()
        {
            List<byte[]> buffers = new List<byte[]>();
            try
            {
                var settings = _mailDeliverDataAccess.GetListMailSetting();
                if (settings != null)
                {
                    foreach (var setting in settings)
                    {
                        var stream = MailFormatter.Serialize(setting);
                        buffers.Add(stream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return buffers;
        }

        /// <summary>
        /// Get List Config for Send Mail From DB.
        /// </summary>
        /// <param name="serverIp">Server IP for query.</param>
        /// <returns>List Mail Config.</returns>
        public IEnumerable<byte[]> GetListMailConfig(string serverIp = "")
        {
            List<byte[]> buffers = new List<byte[]>();
            try
            {
                var configs = _mailDeliverDataAccess.GetListMailConfig(serverIp);
                if (configs != null)
                {
                    foreach (var config in configs)
                    {
                        var stream = MailFormatter.Serialize(config);
                        buffers.Add(stream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return buffers;
        }

        /// <summary>
        /// Get List Function from DB.
        /// </summary>
        /// <returns>Return list of Function.</returns>
        public IEnumerable<byte[]> GetListMailFuntion()
        {
            List<byte[]> buffers = new List<byte[]>();
            try
            {
                var functions = _mailDeliverDataAccess.GetListMailFuntion();
                if (functions != null)
                {
                    foreach (var function in functions)
                    {
                        var stream = MailFormatter.Serialize(function);
                        buffers.Add(stream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return buffers;
        }

        #endregion get list for mem

        /// <summary>
        /// Send mail, if Mail Record already in Database, will get from database and then send mail.
        /// </summary>
        /// <param name="record">Mail Record contains all information about Mail will be sent.</param>
        public void SendMail(MailRecord record)
        {
            record = record.IsReady ? record : _mailDeliverDataAccess.GetMailRecord(record.MailID, true);
            if (record != null && record.IsReady)
            {
                Task<string> task = Task.Factory.StartNew<string>(
                    () =>
                    {
                        string error = string.Empty;
                        MailExtension.SendMail(record, out error);
                        return error;
                    });

                string message = task.Result;
                LogExtension.LogMessage(string.Format("Sending mail: {0}. Error message: {1}", record.MailID, message));
            }
        }

        /// <summary>
        /// Get list mail error.
        /// </summary>
        /// <param name="mailID">Mail id for get error.</param>
        /// <returns>List error information of mail.</returns>
        public IEnumerable<byte[]> GetListMailError(long mailID)
        {
            List<byte[]> errors = new List<byte[]>();
            try
            {
                var errorList = _mailDynamicRepository.Fetch<MailError>(new GetMailErrorCriteria
                {
                    MailID = mailID
                });
                if (errorList != null && errorList.Any())
                {
                    foreach (var error in errorList)
                    {
                        var stream = MailFormatter.Serialize(error);
                        errors.Add(stream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return errors;
        }

        /// <summary>
        /// Save mail error.
        /// </summary>
        /// <param name="buffer">Binary data.</param>
        /// <returns>Save status.</returns>
        public byte[] SaveMailError(byte[] buffer)
        {
            return null;
        }

        /// <summary>
        /// Insert mail error.
        /// </summary>
        /// <param name="criteria">Error information.</param>
        /// <returns>Error inserted Id.</returns>
        public long InsertMailError(InsertMailErrorCriteria criteria)
        {
            try
            {
                return _mailDeliverDataAccess.InsertMailError(criteria);
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
                return 0;
            }
        }
    }
}