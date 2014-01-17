// --------------------------------------------------------------------------
// <copyright file="IMailDeliver.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.MailClient.API
{
    using System;
    using System.Collections.Generic;
    using Fanex.MailDeliver.Model;

    public interface IMailDeliver
    {
        void SendMail(MailRecord mail, bool canRetry = true);

        MailRecord GetMail(long mailID);

        IEnumerable<MailRecord> GetListMail(int appID, DateTime mailDate, int page, int counter);

        IEnumerable<MailRecord> GetListMailByDate(int appID, DateTime fromDate, DateTime toDate, ref int totalRows, int page, int counter, MailStatus status);

        MailConfig StoreConfig(MailConfig config);

        MailConfig GetConfig(int appID);

        MailSetting StoreSetting(MailSetting config);

        MailSetting GetSetting(int settingID);

        void ReSendMail(long mailID);

        MailFunction StoreFunction(MailFunction func);

        MailFunction GetFunction(int funcID);

        IEnumerable<MailSetting> GetListSetting();

        IEnumerable<MailConfig> GetListConfig(string serverIp);

        IEnumerable<MailFunction> GetListFuntion();

        // Fluent interface
        IMailDeliver CreateMail(string mailTitle = "");

        IMailDeliver AddSubject(string mailSubject = "");

        IMailDeliver To(string toMail = "");

        IMailDeliver AddSendMailApplication(int mailApplication = 0);

        IMailDeliver AddSendMailFunction(int mailFunction = 0);

        IMailDeliver AddBody(string mailBody = "");

        IMailDeliver AddTitle(string mailTitle = "");

        IMailDeliver ToCcMails(string toCCMails = "");

        IMailDeliver ToBccMails(string toBccMails = "");

        IMailDeliver AddAttachments(List<MailAttachment> mailAttachments);

        IMailDeliver AddAlternativeViews(List<MailLinkedResource> mailAlternativeViews);

        IMailDeliver SendMail(bool canRetry = true);

        IEnumerable<MailError> GetListError(long mailID);
    }
}