// --------------------------------------------------------------------------
// <copyright file="IMailService.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2013.11.11">Created</change>
// </history>

namespace Fanex.MailDeliver.MailClient.API.WCF
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName = "IMailServiceChannel")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName", Justification = "Reviewed.")]
    public interface IMailService
    {
        [System.ServiceModel.OperationContractAttribute(IsOneWay = true, Action = "http://tempuri.org/IMailService/SendMail")]
        void SendMail(byte[] buffer);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/GetMail")]
        byte[] GetMail(long mailID);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/GetListMail")]
        IEnumerable<byte[]> GetListMail(int appID, DateTime mailDate, int page, int counter);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/GetListMailByDate")]
        IEnumerable<byte[]> GetListMailByDate(int appID, DateTime fromDate, DateTime toDate, ref int totalRows, int page, int counter, int status);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/StoreConfig")]
        byte[] StoreConfig(byte[] buffer);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/GetConfig")]
        byte[] GetConfig(int appID);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/StoreSetting")]
        byte[] StoreSetting(byte[] buffer);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/GetSetting")]
        byte[] GetSetting(int settingID);

        [System.ServiceModel.OperationContractAttribute(IsOneWay = true, Action = "http://tempuri.org/IMailService/ReSendMail")]
        void ReSendMail(long mailID);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/StoreFunction")]
        byte[] StoreFunction(byte[] buffer);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/GetFunction")]
        byte[] GetFunction(int funcID);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/GetListSetting")]
        IEnumerable<byte[]> GetListSetting();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/GetListConfig")]
        IEnumerable<byte[]> GetListConfig(string serverIp);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/GetListFuntion")]
        IEnumerable<byte[]> GetListFuntion();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IMailService/GetListError")]
        IEnumerable<byte[]> GetListError(long mailID);
    }
}