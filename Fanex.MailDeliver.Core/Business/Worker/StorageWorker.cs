// --------------------------------------------------------------------------
// <copyright file="WStoreMail.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>
namespace Fanex.MailDeliver.Business.Worker
{
    using System;
    using System.Collections.Generic;
    using Fanex.MailDeliver.Business.Implement;
    using Fanex.MailDeliver.Business.Interface;
    using Fanex.MailDeliver.Infrastructure.Logging;
    using Fanex.MailDeliver.MailClient.Util;
    using Fanex.MailDeliver.Model;

    public class StorageWorker
    {
        private IMailDeliverBusiness _mailDeliverBusiness;

        public StorageWorker()
        {
            _mailDeliverBusiness = new MailDeliverBusiness();
        }

        /// <summary>
        /// Store Config information
        /// </summary>
        /// <param name="buffer">buffer contains config information, this param will be parse back to MailConfig Object</param>
        public byte[] StoreConfig(byte[] buffer)
        {
            try
            {
                var config = MailFormatter.Deserialize<MailConfig>(buffer);
                config = _mailDeliverBusiness.SaveConfig(config);
                return MailFormatter.Serialize(config).ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Get Mail from Mail Id
        /// </summary>
        /// <param name="mailID">Mail Id</param>
        /// <returns>Byte Array contains Mail Information</returns>
        public byte[] GetMail(long mailID)
        {
            try
            {
                var record = _mailDeliverBusiness.GetMail(mailID);
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
        /// Get List mail Paged with Data
        /// </summary>
        /// <param name="appID">App Id used to send mail</param>
        /// <param name="mailDate">Mail Date for Get Mail</param>
        /// <param name="page">Page Number</param>
        /// <param name="counter">Num Record Per  Page</param>
        /// <returns>Return list Byte Array contains infor</returns>
        public IEnumerable<byte[]> GetList(int appID, DateTime mailDate, int page, int counter)
        {
            try
            {
                List<byte[]> mailList = new List<byte[]>();
                var records = _mailDeliverBusiness.GetListMail(appID, mailDate, page, counter);
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
        /// Get List mail Paged with Data, and Date Range
        /// </summary>
        /// <param name="appID">App Id used to send mail</param>
        /// <param name="fromDate">From Date</param>
        /// <param name="toDate">To Date</param>
        /// <param name="page">Page Number</param>
        /// <param name="counter">Num Record Per  Page</param>
        /// <returns>Return list Byte Array contains infor</returns>
        public IEnumerable<byte[]> GetListMailByDate(int appID, DateTime fromDate, DateTime toDate, int page, int counter)
        {
            try
            {
                List<byte[]> mailList = new List<byte[]>();
                var records = _mailDeliverBusiness.GetListMailByDate(appID, fromDate, toDate, page, counter);
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
        /// Get Mail Config from App Id
        /// </summary>
        /// <param name="appID">App Id</param>
        /// <returns>Byte Array contains MailConfig information</returns>
        public byte[] GetConfig(int appID)
        {
            try
            {
                var config = _mailDeliverBusiness.GetConfig(appID);
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

        /// <summary>
        /// Store Mail Setting 
        /// </summary>
        /// <param name="buffer">Buffer contains mail setting information</param>
        public byte[] StoreSetting(byte[] buffer)
        {
            try
            {
                var setting = MailFormatter.Deserialize<MailSetting>(buffer);
                setting = _mailDeliverBusiness.SaveSetting(setting);
                return MailFormatter.Serialize(setting).ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Get Mail Setting
        /// </summary>
        /// <param name="settingID">Setting Id</param>
        /// <returns>Return byte Array contains setting information</returns>
        public byte[] GetSetting(int settingID)
        {
            try
            {
                var setting = _mailDeliverBusiness.GetSetting(settingID);
                if (setting != null)
                {
                    var stream = MailFormatter.Serialize(setting);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        #region mail function service

        /// <summary>
        /// Save Function
        /// </summary>
        /// <param name="buffer">Buffer contains information</param>
        public byte[] SaveFunction(byte[] buffer)
        {
            try
            {
                var func = MailFormatter.Deserialize<MailFunction>(buffer);
                func = _mailDeliverBusiness.SaveFunction(func);
                return MailFormatter.Serialize(func).ToArray();
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Get Function from Id
        /// </summary>
        /// <param name="funcID">function ID</param>
        /// <returns>Byte Array contain Function information</returns>
        public byte[] GetFunction(int funcID)
        {
            try
            {
                var func = _mailDeliverBusiness.GetFunction(funcID);
                if (func != null)
                {
                    var stream = MailFormatter.Serialize(func);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        #endregion mail function service

        #region get list

        /// <summary>
        /// Get List Mail Setting
        /// </summary>
        /// <returns>Return list of MailSetting by Byte Array</returns>
        public IEnumerable<byte[]> GetListSetting()
        {
            List<byte[]> buffers = new List<byte[]>();
            var settings = _mailDeliverBusiness.GetListSetting();
            if (settings != null)
            {
                foreach (var setting in settings)
                {
                    var stream = MailFormatter.Serialize(setting);
                    buffers.Add(stream.ToArray());
                }
            }

            return buffers;
        }

        /// <summary>
        /// Get List Mail Config
        /// </summary>
        /// <returns>Return list of MailConfig by Byte Array</returns>
        public IEnumerable<byte[]> GetListConfig()
        {
            List<byte[]> buffers = new List<byte[]>();
            var configs = _mailDeliverBusiness.GetListConfig();
            if (configs != null)
            {
                foreach (var config in configs)
                {
                    var stream = MailFormatter.Serialize(config);
                    buffers.Add(stream.ToArray());
                }
            }

            return buffers;
        }

        /// <summary>
        /// Get List Mail Function
        /// </summary>
        /// <returns>Return list of MailFunction by Byte Array</returns>
        public IEnumerable<byte[]> GetListFuntion()
        {
            List<byte[]> buffers = new List<byte[]>();
            var functions = _mailDeliverBusiness.GetListFuntion();
            if (functions != null)
            {
                foreach (var function in functions)
                {
                    var stream = MailFormatter.Serialize(function);
                    buffers.Add(stream.ToArray());
                }
            }

            return buffers;
        }

        #endregion get list
    }
}