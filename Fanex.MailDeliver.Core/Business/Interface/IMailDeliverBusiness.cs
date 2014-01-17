// --------------------------------------------------------------------------
// <copyright file="IMailDeliverBusiness.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.Business.Interface
{
    using System;
    using System.Collections.Generic;
    using Fanex.MailDeliver.Core.Criterias;
    using Fanex.MailDeliver.Model;

    public interface IMailDeliverBusiness
    {
        long InsertMailRecord(MailRecord mailItem);

        byte[] SaveMailConfig(byte[] buffer);

        byte[] GetMailRecordByStream(long mailID);

        MailRecord GetMailRecord(long mailID, bool isSend = false);

        IEnumerable<byte[]> GetListMailRecordByDate(int appID, DateTime mailDate, int page, int counter);

        IEnumerable<byte[]> GetListMailRecordByDateRange(
            int appID,
            DateTime fromDate,
            DateTime toDate,
            ref int totalRows,
            int page,
            int counter,
            int status);

        byte[] GetMailConfig(int appID);

        byte[] SaveMailSetting(byte[] buffer);

        byte[] GetMailSetting(int settingID);

        byte[] SaveMailFunction(byte[] buffer);

        byte[] GetMailFunction(int funcID);

        void UpdateMailStatus(long mailID, bool status);

        IEnumerable<byte[]> GetListMailSetting();

        IEnumerable<byte[]> GetListMailConfig(string serverIp = "");

        IEnumerable<byte[]> GetListMailFuntion();

        void SendMail(MailRecord record);

        IEnumerable<byte[]> GetListMailError(long mailID);

        byte[] SaveMailError(byte[] buffer);

        long InsertMailError(InsertMailErrorCriteria criteria);
    }
}