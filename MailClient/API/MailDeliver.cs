namespace Fanex.MailDeliver.MailClient.API
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Fanex.MailDeliver.MailClient.API.WCF;
    using Fanex.MailDeliver.MailClient.Util;
    using Fanex.MailDeliver.Model;

    /// <summary>
    /// Mail deliver.
    /// </summary>
    public class MailDeliver : IMailDeliver, IDisposable
    {
        private EndpointAddress _endpoint;
        private MailServiceClient _mailClient;
        private MailRecord _mailRecord;
        private Binding _binding;

        public MailDeliver()
        {
            // Get mail address service
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("app_mail_address")))
            {
                var appMailAddress = ConfigurationManager.AppSettings["app_mail_address"].ToString();
                _endpoint = new EndpointAddress(appMailAddress);

                // Setting binding
                _binding = GetBinding(_endpoint);

                // Create mail client service
                _mailClient = new MailServiceClient(_binding, _endpoint);

                // Create Mail Record for send Mail
                _mailRecord = new MailRecord();
            }
            else
            {
                throw new Exception("The appsettings key \"app_mail_address\" which didn't support");
            }
        }

        /// <summary>
        /// Get binding for end point.
        /// </summary>
        /// <param name="endpoint">End point for client access.</param>
        /// <returns>Binding configurations.</returns>
        public Binding GetBinding(EndpointAddress endpoint)
        {
            if (endpoint.Uri.Scheme.IndexOf("http") != -1)
            {
                BasicHttpBinding httpBinding = new BasicHttpBinding(BasicHttpSecurityMode.None);
                httpBinding.SendTimeout = new TimeSpan(0, 0, 5);
                httpBinding.OpenTimeout = new TimeSpan(0, 0, 5);
                httpBinding.MaxBufferSize = 2097152; // max 2M
                httpBinding.MaxReceivedMessageSize = 2097152;
                httpBinding.MaxBufferPoolSize = 2097152 * 4; // max 8M
                httpBinding.ReaderQuotas.MaxArrayLength = 2097152;
                httpBinding.ReaderQuotas.MaxDepth = 32;
                httpBinding.ReaderQuotas.MaxBytesPerRead = 4096;
                httpBinding.ReaderQuotas.MaxStringContentLength = 2097152;
                return httpBinding;
            }
            else
            {
                // Setting net.tcp binding
                var tcpBinding = new NetTcpBinding(SecurityMode.None, true);
                tcpBinding.ReliableSession.Enabled = true;
                tcpBinding.ReliableSession.Ordered = true;
                tcpBinding.SendTimeout = new TimeSpan(0, 0, 5);
                tcpBinding.OpenTimeout = new TimeSpan(0, 0, 5);
                tcpBinding.MaxBufferSize = 2097152; // max 2M
                tcpBinding.MaxBufferPoolSize = 2097152 * 4; // max 8M
                tcpBinding.MaxConnections = 10;
                tcpBinding.ReaderQuotas.MaxArrayLength = 2097152;
                tcpBinding.ReaderQuotas.MaxDepth = 32;
                tcpBinding.ReaderQuotas.MaxBytesPerRead = 4096;
                tcpBinding.ReaderQuotas.MaxStringContentLength = 2097152;

                return tcpBinding;
            }
        }

        /// <summary>
        /// Send Mail, we can retry or not after send mail process fail.
        /// </summary>
        /// <param name="mail">Mail need to be sent.</param>
        /// <param name="canRetry">Can resend or not.</param>
        public void SendMail(
            MailRecord mail,
            bool canRetry = true)
        {
            if (mail != null)
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("app_mail_id")) 
                    || (mail.Config != null && mail.Config.AppID > 0))
                {
                    if (mail.Config == null || mail.Config.AppID == 0)
                    {
                        var appid = Convert.ToInt32(ConfigurationManager.AppSettings["app_mail_id"]);
                        mail.Config = new MailConfig
                        {
                            AppID = appid
                        };
                    }

                    Util.AttachmentUtil.Format(ref mail);

                    mail.CanRetry = canRetry;

                    mail.Subject = FilterSpecialCharacters(mail.Subject);
                    mail.Title = FilterSpecialCharacters(mail.Title);
                    mail.MailTo = FilterSpecialCharacterForEmail(mail.MailTo);
                    mail.MailCc = FilterSpecialCharacterForEmail(mail.MailCc);
                    mail.MailBcc = FilterSpecialCharacterForEmail(mail.MailBcc);
                    
                    var stream = MailFormatter.Serialize(mail);
                    var buffer = stream.ToArray();
                    _mailClient.SendMail(buffer);
                }
                else
                {
                    throw new Exception("The appsettings key \"app_mail_id\" which didn't support ");
                }
            }
        }

        private string FilterSpecialCharacters(string text)
        {
            if(string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
            
            var filteredText = text
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Replace("\t", string.Empty)
                .Replace("&nbsp;", " ");

            var specialCharactersConfig = ConfigurationManager.AppSettings["MailService:SpecialCharacters"];
            if(!string.IsNullOrWhiteSpace(specialCharactersConfig))
            {
                var specialCharacters = specialCharactersConfig.Split(',');

                foreach (var specialCharacter in specialCharacters)
                {
                    filteredText = filteredText.Replace(specialCharacter, string.Empty);
                }
            }

            return filteredText;
        }

        private string FilterSpecialCharacterForEmail(string emailRaw)
        {
            if (string.IsNullOrWhiteSpace(emailRaw))
            {
                return emailRaw;
            }
            
            return FilterSpecialCharacters(
                emailRaw
                .Replace("  ", string.Empty)
                .Replace(" ", string.Empty));
        }

        /// <summary>
        /// Get Mail from Mail ID.
        /// </summary>
        /// <param name="mailID">Mail record Id.</param>
        /// <returns>Mail Record id.</returns>
        public MailRecord GetMail(long mailID)
        {
            MailRecord record = null;
            if (mailID > 0)
            {
                byte[] buffer = _mailClient.GetMail(mailID);
                if (buffer != null)
                {
                    record = MailFormatter.Deserialize<MailRecord>(buffer);
                }
            }

            return record;
        }

        /// <summary>
        /// Get List mail by date.
        /// </summary>
        /// <param name="appID">App send mail.</param>
        /// <param name="mailDate">Mail sent date.</param>
        /// <param name="page">Page index.</param>
        /// <param name="counter">Number per page.</param>
        /// <returns>List Mail Record.</returns>
        public IEnumerable<MailRecord> GetListMail(
            int appID,
            DateTime mailDate,
            int page,
            int counter)
        {
            var mailList = new List<MailRecord>();
            if (appID > 0)
            {
                IEnumerable<byte[]> buffers = _mailClient.GetListMail(appID, mailDate, page, counter);
                if (buffers != null)
                {
                    foreach (var buffer in buffers)
                    {
                        var record = MailFormatter.Deserialize<MailRecord>(buffer);
                        if (record != null)
                        {
                            mailList.Add(record);
                        }
                    }
                }
            }

            return mailList;
        }

        /// <summary>
        /// Get List mail from date to date.
        /// </summary>
        /// <param name="appID">App Id, if this equals with 0, will get all mail.</param>
        /// <param name="fromDate">From Date value.</param>
        /// <param name="toDate">To Date value.</param>
        /// <param name="totalRows">Total rows.</param>
        /// <param name="page">Total pages.</param>
        /// <param name="counter">Number Mail Record per page.</param>
        /// <param name="status">Status of mail, all, success or fail.</param>
        /// <returns>List of Mail record.</returns>
        public IEnumerable<MailRecord> GetListMailByDate(
            int appID,
            DateTime fromDate,
            DateTime toDate,
            ref int totalRows,
            int page,
            int counter,
            MailStatus status)
        {
            var mailList = new List<MailRecord>();
            if (appID >= 0)
            {
                IEnumerable<byte[]> buffers = _mailClient.GetListMailByDate(appID, fromDate, toDate, ref totalRows, page, counter, (int)status);
                if (buffers != null)
                {
                    foreach (var buffer in buffers)
                    {
                        var record = MailFormatter.Deserialize<MailRecord>(buffer);
                        if (record != null)
                        {
                            mailList.Add(record);
                        }
                    }
                }
            }

            return mailList;
        }

        /// <summary>
        /// Store config to Database.
        /// </summary>
        /// <param name="config">Config information.</param>
        /// <returns>MailConfig after insert to database.</returns>
        public MailConfig StoreConfig(MailConfig config)
        {
            if (config != null)
            {
                var stream = MailFormatter.Serialize(config);
                return MailFormatter.Deserialize<MailConfig>(_mailClient.StoreConfig(stream.ToArray()));
            }

            return null;
        }

        /// <summary>
        /// Get config from Database for send mail.
        /// </summary>
        /// <param name="appID">Application Id.</param>
        /// <returns>Mail Config.</returns>
        public MailConfig GetConfig(int appID)
        {
            MailConfig config = null;

            if (appID > 0)
            {
                var buffer = _mailClient.GetConfig(appID);
                if (buffer != null)
                {
                    config = MailFormatter.Deserialize<MailConfig>(buffer);
                }
            }

            return config;
        }

        #region Mail Setting

        public MailSetting StoreSetting(MailSetting setting)
        {
            if (setting != null)
            {
                var stream = MailFormatter.Serialize(setting);
                return MailFormatter.Deserialize<MailSetting>(_mailClient.StoreSetting(stream.ToArray()));
            }

            return null;
        }

        /// <summary>
        /// Get Setting from Database by ID.
        /// </summary>
        /// <param name="settingID">Setting ID.</param>
        /// <returns>Setting information.</returns>
        public MailSetting GetSetting(int settingID)
        {
            MailSetting setting = null;

            if (settingID > 0)
            {
                var buffer = _mailClient.GetSetting(settingID);
                if (buffer != null)
                {
                    setting = MailFormatter.Deserialize<MailSetting>(buffer);
                }
            }

            return setting;
        }

        #endregion Mail Setting

        #region resend mail

        public void ReSendMail(long mailID)
        {
            if (mailID > 0)
            {
                _mailClient.ReSendMail(mailID);
            }
        }

        #endregion resend mail

        #region mail function

        /// <summary>
        /// Store function to Database.
        /// </summary>
        /// <param name="func">Mail Function.</param>
        /// <returns>Mail function after inserted successfully from Database.</returns>
        public MailFunction StoreFunction(MailFunction func)
        {
            if (func != null)
            {
                var stream = MailFormatter.Serialize(func);
                return MailFormatter.Deserialize<MailFunction>(_mailClient.StoreFunction(stream.ToArray()));
            }

            return null;
        }

        /// <summary>
        /// Get Function from Function ID.
        /// </summary>
        /// <param name="funcID">Function Id.</param>
        /// <returns>Mail Function.</returns>
        public MailFunction GetFunction(int funcID)
        {
            MailFunction func = null;

            if (funcID > 0)
            {
                var buffer = _mailClient.GetFunction(funcID);
                if (buffer != null)
                {
                    func = MailFormatter.Deserialize<MailFunction>(buffer);
                }
            }

            return func;
        }

        #endregion mail function

        #region get list setting

        /// <summary>
        /// Get List Setting from Database.
        /// </summary>
        /// <returns>List MailSetting.</returns>
        public IEnumerable<MailSetting> GetListSetting()
        {
            var settings = new List<MailSetting>();

            IEnumerable<byte[]> buffers = _mailClient.GetListSetting();
            if (buffers != null)
            {
                foreach (var buffer in buffers)
                {
                    var setting = MailFormatter.Deserialize<MailSetting>(buffer);
                    if (setting != null)
                    {
                        settings.Add(setting);
                    }
                }
            }

            return settings;
        }

        /// <summary>
        /// Get List config from Database, it can be searched with Server IP.
        /// </summary>
        /// <param name="serverIp">Server IP, if this is empty mean get all from database.</param>
        /// <returns>List Mail Config.</returns>
        public IEnumerable<MailConfig> GetListConfig(string serverIp)
        {
            var configs = new List<MailConfig>();
            IEnumerable<byte[]> buffers = _mailClient.GetListConfig(serverIp);
            if (buffers != null)
            {
                foreach (var buffer in buffers)
                {
                    var config = MailFormatter.Deserialize<MailConfig>(buffer);
                    if (config != null)
                    {
                        configs.Add(config);
                    }
                }
            }

            return configs;
        }

        /// <summary>
        /// Get List Function From Database.
        /// </summary>
        /// <returns>List Function.</returns>
        public IEnumerable<MailFunction> GetListFuntion()
        {
            var functions = new List<MailFunction>();
            IEnumerable<byte[]> buffers = _mailClient.GetListFuntion();
            if (buffers != null)
            {
                foreach (var buffer in buffers)
                {
                    var func = MailFormatter.Deserialize<MailFunction>(buffer);
                    if (func != null)
                    {
                        functions.Add(func);
                    }
                }
            }

            return functions;
        }

        #endregion get list setting

        /// <summary>
        /// Get List error of send mail.
        /// </summary>
        /// <param name="mailID">Mail id error.</param>
        /// <returns>List error of send mail.</returns>
        public IEnumerable<MailError> GetListError(long mailID)
        {
            var errors = new List<MailError>();
            IEnumerable<byte[]> buffers = _mailClient.GetListError(mailID);
            if (buffers != null)
            {
                foreach (var buffer in buffers)
                {
                    var error = MailFormatter.Deserialize<MailError>(buffer);
                    if (error != null)
                    {
                        errors.Add(error);
                    }
                }
            }

            return errors;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Fluent Interface
        public IMailDeliver CreateMail(string mailTitle = "")
        {
            _mailRecord.Title = mailTitle;
            return this;
        }

        public IMailDeliver AddSubject(string mailSubject = "")
        {
            _mailRecord.Subject = mailSubject;
            return this;
        }

        public IMailDeliver To(string toMail = "")
        {
            _mailRecord.MailTo = toMail;
            return this;
        }

        public IMailDeliver AddSendMailApplication(int mailApplication = 0)
        {
            _mailRecord.Config.AppID = mailApplication;
            return this;
        }

        public IMailDeliver AddSendMailFunction(int mailFunction = 0)
        {
            _mailRecord.Function.FuncID = mailFunction;
            return this;
        }

        public IMailDeliver AddBody(string mailBody = "")
        {
            _mailRecord.Body = mailBody;
            return this;
        }

        public IMailDeliver ToCcMails(string toCCMails = "")
        {
            _mailRecord.MailCc = toCCMails;
            return this;
        }

        public IMailDeliver ToBccMails(string toBccMails = "")
        {
            _mailRecord.MailBcc = toBccMails;
            return this;
        }

        public IMailDeliver AddTitle(string mailTitle = "")
        {
            _mailRecord.Title = mailTitle;
            return this;
        }

        public IMailDeliver AddAttachments(List<MailAttachment> mailAttachments)
        {
            if (mailAttachments != null)
            {
                _mailRecord.Attachments = mailAttachments;
            }

            return this;
        }

        public IMailDeliver AddAlternativeViews(List<MailLinkedResource> mailAlternativeViews)
        {
            if (mailAlternativeViews != null)
            {
                _mailRecord.AlternateView = mailAlternativeViews;
            }

            return this;
        }

        public IMailDeliver SendMail(bool canRetry = true)
        {
            _mailRecord.CanRetry = canRetry;
            this.SendMail(_mailRecord, _mailRecord.CanRetry);
            return this;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_mailClient != null && _mailClient.State == CommunicationState.Opened)
            {
                _mailClient.Close();
            }

            _mailClient = null;
            _binding = null;
            _endpoint = null;
        }
    }
}