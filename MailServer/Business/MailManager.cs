using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.MailDeliver.Infrastructure.Logging;
using Fanex.MailDeliver.MailServer.Core;
using Fanex.MailDeliver.MailServer.Storage;
using Fanex.MailDeliver.Entities;

namespace Fanex.MailDeliver.MailServer.Business
{
    public class MailManager : IMailManager
    {
        private IStorageMail __storage;

        public MailManager()
        {
            //TODO
            __storage = BootManager.Locator.GetInstance<IStorageMail>();
        }

        public MailManager(IStorageMail storage)
        {
            __storage = storage;
        }

        /// <summary>
        /// StoreMail
        /// </summary>
        /// <param name="buffer"></param>
        public long StoreMail(MailRecord mailRecord)
        {
            long mailId = 0;
            try
            {
                if (mailRecord != null)
                {
                    mailId = __storage.Insert(mailRecord);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
            return mailId;
        }

        /// <summary>
        /// StoreConfig
        /// </summary>
        /// <param name="config"></param>
        public void StoreConfig(MailConfig mailConfig)
        {
            try
            {
                if (mailConfig != null)
                {
                    //__storage = BootManager.Locator.GetInstance<IStorageMail>();
                    __storage.SaveConfig(mailConfig);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        /// <summary>
        /// GetMail
        /// </summary>
        /// <param name="mailID"></param>
        /// <returns></returns>
        public MailRecord GetMail(long mailID, bool isSend = false)
        {
            try
            {
                if (mailID > 0)
                {
                    //__storage = BootManager.Locator.GetInstance<IStorageMail>();
                    return __storage.GetMail(mailID, isSend);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
            return null;
        }

        /// <summary>
        /// GetListMail
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="mailDate"></param>
        /// <param name="page"></param>
        /// <param name="counter"></param>
        /// <returns></returns>
        public IEnumerable<MailRecord> GetListMail(int appID, DateTime mailDate, int page = 1, int counter = 100)
        {
            try
            {
                if (appID > 0)
                {
                    //__storage = BootManager.Locator.GetInstance<IStorageMail>();
                    return __storage.GetListMail(appID, mailDate, page, counter);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
            return null;
        }

        public IEnumerable<MailRecord> GetListMailByDate(int appID, DateTime fromDate, DateTime toDate, int page, int counter)
        {
            try
            {
                if (appID > 0)
                {
                    //__storage = BootManager.Locator.GetInstance<IStorageMail>();
                    return __storage.GetListMailByDate(appID, fromDate, toDate, page, counter);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
            return null;
        }

        /// <summary>
        /// GetConfig
        /// </summary>
        /// <param name="appID"></param>
        /// <returns></returns>
        public MailConfig GetConfig(int appID)
        {
            try
            {
                if (appID > 0)
                {
                    //__storage = BootManager.Locator.GetInstance<IStorageMail>();
                    return __storage.GetConfig(appID);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
            return null;
        }

        #region mail setting

        public void StoreSetting(MailSetting setting)
        {
            try
            {
                if (setting != null)
                {
                    //__storage = BootManager.Locator.GetInstance<IStorageMail>();
                    __storage.SaveSetting(setting);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        public MailSetting GetSetting(int settingID)
        {
            try
            {
                if (settingID > 0)
                {
                    //__storage = BootManager.Locator.GetInstance<IStorageMail>();
                    return __storage.GetSetting(settingID);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
            return null;
        }

        #endregion mail setting

        #region mail funcion

        public void SaveFunction(MailFunction func)
        {
            try
            {
                if (func != null)
                {
                    //__storage = BootManager.Locator.GetInstance<IStorageMail>();
                    __storage.SaveFunction(func);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        public MailFunction GetFunction(int funcID)
        {
            try
            {
                if (funcID > 0)
                {
                    //__storage = BootManager.Locator.GetInstance<IStorageMail>();
                    return __storage.GetFunction(funcID);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
            return null;
        }

        #endregion mail funcion

        #region send mail

        /// <summary>
        /// SendMail
        /// </summary>
        /// <param name="record"></param>
        public void SendMail(MailRecord record)
        {
            record = record.IsReady ? record : GetMail(record.MailID, true);
            if (record != null && record.IsReady)
            {
                Task<string> task = Task.Factory.StartNew<string>(
                    () =>
                    {
                        string error = string.Empty;
                        MailExtention.SendMail(record, out error);
                        return error;
                    }
                );
                string message = task.Result;
                LogExtension.LogMessage(string.Format("Sending mail: {0}. Error message: {1}", record.MailID, message));
            }
        }

        /// <summary>
        /// UpdateSendStatus
        /// </summary>
        /// <param name="status"></param>
        /// <param name="mailID"></param>
        public void UpdateSendStatus(bool status, long mailID)
        {
            //TODO
            int option = false.Equals(status) ? -1 : 1;
            __storage.UpdateCountSent(mailID, option);
            LogExtension.LogMessage(string.Format("Sent mail {0}. Status {1}", mailID, status));
        }

        #endregion send mail

        #region get list

        public IEnumerable<MailSetting> GetListSetting()
        {
            return __storage.GetListSetting();
        }

        public IEnumerable<MailConfig> GetListConfig()
        {
            return __storage.GetListConfig();
        }

        public IEnumerable<MailFunction> GetListFuntion()
        {
            return __storage.GetListFuntion();
        }

        #endregion get list
    }
}