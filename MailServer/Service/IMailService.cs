// --------------------------------------------------------------------------
// <copyright file="IMailService.cs" company="Nexcel Solutions Vietnam">
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
    using System.ServiceModel;

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IMailService" in both code and config file together.
    [ServiceContract]
    public interface IMailService
    {
        [OperationContract(IsOneWay = true)]
        void SendMail(byte[] buffer);

        [OperationContract]
        byte[] StoreConfig(byte[] buffer);

        [OperationContract]
        byte[] GetMail(long mailID);

        [OperationContract]
        IEnumerable<byte[]> GetListMail(int appID, DateTime mailDate, int page, int counter);

        [OperationContract]
        IEnumerable<byte[]> GetListMailByDate(int appID, DateTime fromDate, DateTime toDate, ref int totalRows, int page, int counter, int status);

        [OperationContract]
        byte[] GetConfig(int appID);

        [OperationContract]
        byte[] StoreSetting(byte[] buffer);

        [OperationContract]
        byte[] GetSetting(int settingID);

        [OperationContract(IsOneWay = true)]
        void ReSendMail(long mailID);

        [OperationContract]
        byte[] StoreFunction(byte[] buffer);

        [OperationContract]
        byte[] GetFunction(int funcID);

        #region get list

        [OperationContract]
        IEnumerable<byte[]> GetListSetting();

        [OperationContract]
        IEnumerable<byte[]> GetListConfig(string serverIp);

        [OperationContract]
        IEnumerable<byte[]> GetListFuntion();

        [OperationContract]
        IEnumerable<byte[]> GetListError(long mailID);

        #endregion get list
    }
}