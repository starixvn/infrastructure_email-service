using System;
using System.Collections.Generic;
using Fanex.MailDeliver.Entities;

namespace Fanex.MailDeliver.MailServer.Business
{
    internal interface IMailManager
    {
        long StoreMail(MailRecord mailRecord);

        void StoreConfig(MailConfig mailConfig);

        MailRecord GetMail(long mailID, bool isSend = false);

        IEnumerable<MailRecord> GetListMail(int appID, DateTime mailDate, int page, int counter);

        IEnumerable<MailRecord> GetListMailByDate(int appID, DateTime fromDate, DateTime toDate, int page, int counter);

        MailConfig GetConfig(int appID);

        void StoreSetting(MailSetting setting);

        MailSetting GetSetting(int settingID);

        void SaveFunction(MailFunction func);

        MailFunction GetFunction(int funcID);

        void SendMail(MailRecord record);

        void UpdateSendStatus(bool status, long mailID);

        #region get list

        IEnumerable<MailSetting> GetListSetting();

        IEnumerable<MailConfig> GetListConfig();

        IEnumerable<MailFunction> GetListFuntion();

        #endregion get list
    }
}