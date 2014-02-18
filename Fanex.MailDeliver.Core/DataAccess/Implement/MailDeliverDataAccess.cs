// --------------------------------------------------------------------------
// <copyright file="MailDeliverDataAccess.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
//   <change who="Ricky.Nguyen" date="2012.9.24">Update get MailFunction</change>
//   <change who="Ricky.Nguyen" date="2012.9.24">Update MailUpdatedDate property of MailRecord</change>
// </history>

namespace Fanex.MailDeliver.DataAccess.Implement
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Fanex.Common;
    using Fanex.Data;
    using Fanex.MailDeliver.Core.Criterias;
    using Fanex.MailDeliver.DataAccess.Interface;
    using Fanex.MailDeliver.Model;

    /// <summary>
    /// Mail Deliver Data Access.
    /// </summary>
    public class MailDeliverDataAccess : IMailDeliverDataAccess
    {
        /// <summary>
        /// Insert Mail to Database.
        /// </summary>
        /// <param name="mailRecord">Mail Record contains information.</param>
        /// <returns>Mail after inserted data.</returns>
        public long InsertMailRecord(MailRecord mailRecord)
        {
            long mailID = 0;

            var param = new Hashtable();
            param.Add("mailid", mailRecord.MailID);
            param.Add("appid", mailRecord.Config != null ? mailRecord.Config.AppID : mailRecord.AppID);
            param.Add("funcid", mailRecord.Function != null ? mailRecord.Function.FuncID : mailRecord.FuncID);
            param.Add("mailto", mailRecord.IsEncrypt ? Cryptography.CfsEnCrypt(mailRecord.MailTo ?? string.Empty) : mailRecord.MailTo);
            param.Add("mailcc", mailRecord.IsEncrypt ? Cryptography.CfsEnCrypt(mailRecord.MailCc ?? string.Empty) : mailRecord.MailCc);
            param.Add("mailbcc", mailRecord.IsEncrypt ? Cryptography.CfsEnCrypt(mailRecord.MailBcc ?? string.Empty) : mailRecord.MailBcc);
            param.Add("title", mailRecord.Title);
            param.Add("subject", mailRecord.Subject);
            param.Add("body", mailRecord.IsEncrypt ? Cryptography.CfsEnCrypt(mailRecord.Body ?? string.Empty) : mailRecord.Body);
            param.Add("maildate", DateTime.Now);
            param.Add("attachments", string.Empty);
            param.Add("isencrypt", mailRecord.IsEncrypt);

            // store attachment in local file
            if (mailRecord.Attachments != null)
            {
                var fileNames = mailRecord.Attachments.Select(s => s.FileName).ToArray();
                param["attachments"] = string.Join(",", fileNames);
            }

            using (ObjectDb objDb = new ObjectDb("StoreMail"))
            {
                objDb.ExecuteNonQuery(param);
                mailID = Converter.To<long>(param["mailid"]);
                if (mailID > 0)
                {
                    if (mailRecord.Attachments != null)
                    {
                        (new MailAttachmentManager()).StoreMailAttachments(mailID, mailRecord.Attachments);
                    }

                    if (mailRecord.AlternateView != null)
                    {
                        (new LinkedResourcesManager()).Insert(mailID, mailRecord.AlternateView);
                    }
                }
            }

            return mailID;
        }

        /// <summary>
        /// Get Mail from Mail ID.
        /// </summary>
        /// <param name="mailID">Mail id for get data.</param>
        /// <param name="isSend">Is just send or not.</param>
        /// <returns>Mail Record information.</returns>
        public MailRecord GetMailRecord(
            long mailID,
            bool isSend = false)
        {
            var param = new Hashtable();
            param.Add("mailid", mailID);
            using (ObjectDb objDb = new ObjectDb("GetMail"))
            {
                var result = objDb.ExecuteDataSet(param);
                if (result.Tables.Count > 0)
                {
                    var mailRecordRow = result.Tables[0].Rows[0];
                    var mailItem = GetMailRecordFromDataRow(mailRecordRow, true);
                    if (isSend)
                    {
                        mailItem.IsReady = true;

                        // get attachment file
                        var attachments = (new MailAttachmentManager()).GetMailAttachments(mailID, mailItem.FileAttachments);
                        mailItem.Attachments = attachments == null ? null : attachments.ToList();

                        // get linked resource
                        var resources = (new LinkedResourcesManager()).GetLinkedResources(mailID);
                        mailItem.AlternateView = resources == null ? null : resources.ToList();
                    }

                    return mailItem;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Save Config.
        /// </summary>
        /// <param name="config">Config info.</param>
        /// <returns>Mail Config.</returns>
        public MailConfig SaveMailConfig(MailConfig config)
        {
            var param = new Hashtable();
            param.Add("appid", config.AppID);
            param.Add("appname", config.AppName);
            param.Add("description", config.Description);
            param.Add("settingid", config.Setting != null ? config.Setting.SettingID : config.SettingID);
            param.Add("mailfrom", config.MailFrom ?? string.Empty);
            param.Add("isencrypt", config.IsEncrypt);
            using (ObjectDb objDb = new ObjectDb("StoreConfig"))
            {
                objDb.ExecuteNonQuery(param);
                config.AppID = Converter.ToInt(param["appid"]);
            }

            return config;
        }

        /// <summary>
        /// Get List Mail.
        /// </summary>
        /// <param name="appID">Application id.</param>
        /// <param name="mailDate">Date get Mail.</param>
        /// <param name="page">Number of Page.</param>
        /// <param name="counter">Number per page.</param>
        /// <returns>List Mail Record.</returns>
        public IEnumerable<MailRecord> GetListMailRecordByDate(
            int appID,
            DateTime mailDate,
            int page = 1,
            int counter = 100)
        {
            List<MailRecord> records = new List<MailRecord>();
            var param = new Hashtable();
            param.Add("appid", appID);
            param.Add("maildate", mailDate);
            param.Add("page", page);
            param.Add("counter", counter);
            using (ObjectDb objDb = new ObjectDb("GetListMail"))
            {
                var result = objDb.ExecuteDataSet(param);
                var rows = result.Tables[0].Rows;
                foreach (DataRow rowMail in rows)
                {
                    var mailItem = new MailRecord
                    {
                        AppID = Converter.ToInt(rowMail["appid"]),
                        FuncID = Converter.ToInt(rowMail["funcid"]),
                        MailID = Converter.To<long>(rowMail["mailid"]),
                        Title = Converter.ToString2(rowMail["title"]),
                        Subject = Converter.ToString2(rowMail["subject"]),
                        MailTo = Converter.ToString2(rowMail["mailto"]),
                        MailCc = Converter.ToString2(rowMail["mailcc"]),
                        MailBcc = Converter.ToString2(rowMail["mailbcc"]),
                        MailDate = Converter.ToString2(rowMail["maildate"]),
                        Body = string.Empty,
                        FileAttachments = Converter.ToString2(rowMail["attachments"]),
                        CountSend = Converter.ToInt(rowMail["CountSend"], 0),
                        SentSuccess = Converter.ToInt(rowMail["SentSuccess"], 0),
                        SentFail = Converter.ToInt(rowMail["SentFail"], 0)
                    };
                    records.Add(mailItem);
                }
            }

            return records;
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
        public IEnumerable<MailRecord> GetListMailRecordByDateRange(
            int appID,
            DateTime fromDate,
            DateTime toDate,
            ref int totalRows,
            int page = 1,
            int counter = 100,
            int status = 0)
        {
            List<MailRecord> records = new List<MailRecord>();
            var param = new Hashtable();
            param.Add("appid", appID);
            param.Add("fromdate", fromDate);
            param.Add("todate", toDate);
            param.Add("page", page);
            param.Add("counter", counter);
            param.Add("status", status);
            param.Add("totalrows", 0);
            using (ObjectDb objDb = new ObjectDb("GetListMailByDate"))
            {
                var result = objDb.ExecuteDataSet(param);
                var rows = result.Tables[0].Rows;
                foreach (DataRow mailRecordRow in rows)
                {
                    records.Add(GetMailRecordFromDataRow(mailRecordRow));
                }
            }

            totalRows = Convert.ToInt32(param["totalrows"]);

            return records;
        }

        /// <summary>
        /// Get Config for send mail.
        /// </summary>
        /// <param name="appID">Application Id.</param>
        /// <returns>Mail Config.</returns>
        public MailConfig GetMailConfig(int appID)
        {
            if (appID <= 0)
            {
                return null;
            }

            Hashtable param = new Hashtable();
            param.Add("appid", appID);
            using (ObjectDb objDb = new ObjectDb("GetConfig"))
            {
                var resultDataset = objDb.ExecuteDataSet(param);
                if (resultDataset.Tables.Count > 0
                    && resultDataset.Tables[0].Rows.Count > 0)
                {
                    return GetMailConfigFromDataRow(resultDataset.Tables[0].Rows[0]);
                }

                return new MailConfig();
            }
        }

        /// <summary>
        /// Save Mail Setting.
        /// </summary>
        /// <param name="setting">Mail setting information.</param>
        /// <returns>Mail setting after save success.</returns>
        public MailSetting SaveMailSetting(MailSetting setting)
        {
            var param = new Hashtable();
            param.Add("settingid", setting.SettingID);
            param.Add("settingname", setting.SettingName);
            param.Add("isauth", setting.IsAuth);
            param.Add("username", setting.Username);
            param.Add("password", setting.Password);
            param.Add("server", setting.Server);
            param.Add("port", setting.Port);
            using (ObjectDb objDb = new ObjectDb("StoreSetting"))
            {
                objDb.ExecuteNonQuery(param);
                setting.SettingID = Converter.ToInt(param["settingid"]);
            }

            return setting;
        }

        /// <summary>
        /// Get Setting from Setting ID.
        /// </summary>
        /// <param name="settingID">Setting ID.</param>
        /// <returns>Mail Setting.</returns>
        public MailSetting GetMailSetting(int settingID)
        {
            using (ObjectDb objDb = new ObjectDb("GetSetting"))
            {
                return objDb.QueryEntity<MailSetting>(new { SettingID = settingID });
            }
        }

        /// <summary>
        /// Save setting.
        /// </summary>
        /// <param name="func">Mail function.</param>
        /// <returns>Mail function data.</returns>
        public MailFunction SaveMailFunction(MailFunction func)
        {
            var param = new Hashtable();
            param.Add("funcid", func.FuncID);
            param.Add("appid", func.Config != null ? func.Config.AppID : func.AppID);
            param.Add("funcname", func.FuncName);
            param.Add("settingid", func.Setting != null ? func.Setting.SettingID : func.SettingID);
            using (ObjectDb objDb = new ObjectDb("StoreFunction"))
            {
                objDb.ExecuteNonQuery(param);
                var result = objDb.GetOuputParams();
                if (Converter.ToInt(result["errcode"]) == 0)
                {
                    func.FuncID = Converter.ToInt(result["funcid"]);
                    return func;
                }
                else
                {
                    throw new Exception(result["errmsg"].ToString());
                }
            }
        }

        /// <summary>
        /// Get Function from Function ID.
        /// </summary>
        /// <param name="funcID">Function ID.</param>
        /// <returns>Mail Function.</returns>
        public MailFunction GetMailFunction(int funcID)
        {
            MailFunction mailFunction = new MailFunction();
            var inputParams = new Hashtable();
            inputParams.Add("funcid", funcID);
            using (ObjectDb objDb = new ObjectDb("GetFunction"))
            {
                var resultDataset = objDb.ExecuteDataSet(inputParams);
                if (resultDataset.Tables.Count > 0
                   && resultDataset.Tables[0].Rows.Count > 0)
                {
                    var functionRow = resultDataset.Tables[0].Rows[0];
                    mailFunction = GetMailFunctionFromDataRow(functionRow);
                }

                return mailFunction;
            }
        }

        /// <summary>
        /// Update Mail status after send mail.
        /// </summary>
        /// <param name="mailID">Mail ID need to update.</param>
        /// <param name="status">Send mail status.</param>
        public void UpdateMailStatus(
            long mailID,
            bool status)
        {
            if (mailID > 0)
            {
                var param = new Hashtable();
                param.Add("mailid", mailID);
                param.Add("status", status);
                param.Add("mailupdateddate", DateTime.Now);
                using (ObjectDb objDb = new ObjectDb("UpdateMailStatus"))
                {
                    objDb.ExecuteNonQuery(param);
                }
            }
        }

        /// <summary>
        /// Get List Setting for send mail.
        /// </summary>
        /// <returns>Return List Mail Setting.</returns>
        public List<MailSetting> GetListMailSetting()
        {
            using (ObjectDb objDb = new ObjectDb("GetListSetting"))
            {
                return objDb.Query<MailSetting>().ToList();
            }
        }

        /// <summary>
        /// Get List Config for Send Mail From DB.
        /// </summary>
        /// <param name="serverIp">Server address.</param>
        /// <returns>List Mail Config.</returns>
        public List<MailConfig> GetListMailConfig(string serverIp = "")
        {
            List<MailConfig> mailConfigs = new List<MailConfig>();
            using (ObjectDb objDb = new ObjectDb("GetListConfig"))
            {
                var resultDataset = objDb.ExecuteDataSet(new { Ip = serverIp });
                if (resultDataset.Tables.Count > 0
                    && resultDataset.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow configRow in resultDataset.Tables[0].Rows)
                    {
                        mailConfigs.Add(GetMailConfigFromDataRow(configRow));
                    }
                }

                return mailConfigs;
            }
        }

        /// <summary>
        /// Get List Function from DB.
        /// </summary>
        /// <returns>Return list of Function.</returns>
        public List<MailFunction> GetListMailFuntion()
        {
            List<MailFunction> mailFunctions = new List<MailFunction>();
            using (ObjectDb objDb = new ObjectDb("GetListFunction"))
            {
                var resultDataset = objDb.ExecuteDataSet();
                if (resultDataset.Tables.Count > 0
                    && resultDataset.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow functionRow in resultDataset.Tables[0].Rows)
                    {
                        mailFunctions.Add(GetMailFunctionFromDataRow(functionRow));
                    }
                }

                return mailFunctions;
            }
        }

        /// <summary>
        /// Insert mail error when send mail was error.
        /// </summary>
        /// <param name="criteria">Mail error information.</param>
        /// <returns>Inserted error id.</returns>
        public long InsertMailError(InsertMailErrorCriteria criteria)
        {
            long errorID = 0;

            using (ObjectDb objDb = new ObjectDb(criteria.GetSettingKey()))
            {
                var parameters = criteria.GetSpParams();
                objDb.ExecuteNonQuery(parameters);
                errorID = Converter.To<long>(parameters["errorid"]);
            }

            return errorID;
        }

        /// <summary>
        /// Get Mail Function from DataRow.
        /// </summary>
        /// <param name="mailFunctionDataRow">Mail Function Data row.</param>
        /// <returns>Mail Function.</returns>
        private MailFunction GetMailFunctionFromDataRow(DataRow mailFunctionDataRow)
        {
            return new MailFunction
            {
                FuncID = Converter.ToInt(mailFunctionDataRow["funcid"].ToString()),
                FuncName = Converter.ToString2(mailFunctionDataRow["funcname"].ToString()),
                AppID = Converter.ToInt(mailFunctionDataRow["appid"].ToString()),
                Config = new MailConfig
                {
                    AppID = Converter.ToInt(mailFunctionDataRow["appid"].ToString()),
                    AppName = Converter.ToString2(mailFunctionDataRow["appname"].ToString()),
                    Description = Converter.ToString2(mailFunctionDataRow["description"].ToString()),
                    Setting = new MailSetting
                    {
                        SettingID = Converter.ToInt(mailFunctionDataRow["appsettingid"].ToString()),
                        SettingName = Converter.ToString2(mailFunctionDataRow["appsettingname"].ToString()),
                        Server = Converter.ToString2(mailFunctionDataRow["appsettingserver"].ToString()),
                        Port = Converter.ToInt(mailFunctionDataRow["appsettingport"].ToString())
                    }
                },
                SettingID = Converter.ToInt(mailFunctionDataRow["settingid"].ToString()),
                Setting = new MailSetting
                {
                    SettingID = Converter.ToInt(mailFunctionDataRow["settingid"].ToString()),
                    SettingName = Converter.ToString2(mailFunctionDataRow["settingname"].ToString()),
                    Server = Converter.ToString2(mailFunctionDataRow["server"].ToString()),
                    Port = Converter.ToInt(mailFunctionDataRow["port"].ToString()),
                    IsAuth = Converter.ToBool(mailFunctionDataRow["isauth"].ToString()),
                    Password = Converter.ToString2(mailFunctionDataRow["password"].ToString()),
                    Username = Converter.ToString2(mailFunctionDataRow["username"].ToString())
                }
            };
        }

        /// <summary>
        /// Get Mail Record from Data row after query from Database.
        /// </summary>
        /// <param name="mailRecordDataRow">Mail Record row.</param>
        /// <param name="isGetMailBody">Is Get Mail body or not.</param>
        /// <returns>Mail Record.</returns>
        private MailRecord GetMailRecordFromDataRow(DataRow mailRecordDataRow, bool isGetMailBody = false)
        {
            return new MailRecord
                    {
                        AppID = Converter.ToInt(mailRecordDataRow["appid"]),
                        Config = new MailConfig
                        {
                            AppID = Converter.ToInt(mailRecordDataRow["appid"]),
                            AppName = Converter.ToString2(mailRecordDataRow["appname"]),
                            Setting = new MailSetting
                            {
                                SettingID = Converter.ToInt(mailRecordDataRow["settingid"]),
                                SettingName = Converter.ToString2(mailRecordDataRow["settingname"]),
                                Username = Converter.ToString2(mailRecordDataRow["username"]),
                                Password = Converter.ToString2(mailRecordDataRow["password"]),
                                IsAuth = Converter.ToBool(mailRecordDataRow["isauth"]),
                                Server = Converter.ToString2(mailRecordDataRow["server"]),
                                Port = Converter.ToInt(mailRecordDataRow["port"])
                            }
                        },
                        FuncID = Converter.ToInt(mailRecordDataRow["funcid"]),
                        Function = new MailFunction
                        {
                            FuncID = Converter.ToInt(mailRecordDataRow["funcid"]),
                            FuncName = Converter.ToString2(mailRecordDataRow["funcname"])
                        },
                        MailID = Converter.To<long>(mailRecordDataRow["mailid"]),
                        Title = Converter.ToString2(mailRecordDataRow["title"]),
                        Subject = Converter.ToString2(mailRecordDataRow["subject"]),
                        MailTo = Converter.ToString2(mailRecordDataRow["mailto"]),
                        MailCc = Converter.ToString2(mailRecordDataRow["mailcc"]),
                        MailBcc = Converter.ToString2(mailRecordDataRow["mailbcc"]),
                        MailDate = Converter.ToString2(mailRecordDataRow["maildate"]),
                        MailUpdatedDate = Converter.ToString2(mailRecordDataRow["mailupdateddate"]),
                        Body = isGetMailBody ? Converter.ToString2(mailRecordDataRow["body"]) : string.Empty,
                        FileAttachments = Converter.ToString2(mailRecordDataRow["attachments"]),
                        CountSend = Converter.ToInt(mailRecordDataRow["CountSend"], 0),
                        SentSuccess = Converter.ToInt(mailRecordDataRow["SentSuccess"], 0),
                        SentFail = Converter.ToInt(mailRecordDataRow["SentFail"], 0),
                        IsEncrypt = Converter.ToBool(mailRecordDataRow["IsEncrypt"], false)
                    };
        }

        /// <summary>
        /// Get Mail Config from Data row.
        /// </summary>
        /// <param name="mailConfigDataRow">Mail config data row.</param>
        /// <returns>Mail config data.</returns>
        private MailConfig GetMailConfigFromDataRow(DataRow mailConfigDataRow)
        {
            return new MailConfig
            {
                AppID = Converter.ToInt(mailConfigDataRow["appid"]),
                AppName = Converter.ToString2(mailConfigDataRow["appname"]),
                Description = Converter.ToString2(mailConfigDataRow["description"]),
                SettingID = Converter.ToInt(mailConfigDataRow["settingid"]),
                MailFrom = Converter.ToString2(mailConfigDataRow["mailfrom"]),
                IsEncrypt = Converter.ToBool(mailConfigDataRow["isencrypt"]),
                Setting = new MailSetting
                            {
                                SettingID = Converter.ToInt(mailConfigDataRow["settingid"]),
                                SettingName = Converter.ToString2(mailConfigDataRow["settingname"]),
                                Username = Converter.ToString2(mailConfigDataRow["username"]),
                                Password = Converter.ToString2(mailConfigDataRow["password"]),
                                IsAuth = Converter.ToBool(mailConfigDataRow["isauth"]),
                                Server = Converter.ToString2(mailConfigDataRow["server"]),
                                Port = Converter.ToInt(mailConfigDataRow["port"])
                            }
            };
        }
    }
}