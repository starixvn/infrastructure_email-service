// --------------------------------------------------------------------------
// <copyright file="MailExtension.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.Business
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Messaging;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Text;
    using EASendMail;
    using Fanex.MailDeliver.Business.Implement;
    using Fanex.MailDeliver.Core.Business.Worker;
    using Fanex.MailDeliver.Core.Criterias;
    using Fanex.MailDeliver.Core.Infrastructure;
    using Fanex.MailDeliver.Core.Infrastructure.Configurations;
    using Fanex.MailDeliver.Core.Infrastructure.ExtensionMethod;
    using Fanex.MailDeliver.Core.Infrastructure.Logging;
    using Fanex.MailDeliver.DataAccess.Implement;
    using Fanex.MailDeliver.Model;

    /// <summary>
    /// Mail extension.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed.")]
    public static class MailExtension
    {
        private static object _sendMailLockObject = new object();

        /// <summary>
        /// Send mail to User.
        /// </summary>
        /// <param name="mailSend">Mail contains all information need be sent.</param>
        /// <param name="message">String message contains error message if send mail process throw error.</param>
        [SuppressMessage("StyleCop.CSharp.StylecopPlus.Rules", "SP2101:MethodMustNotContainMoreLinesThan", Justification = "Reviewed. Suppression is OK here.")]
        public static void SendMail(MailRecord mailSend, out string message)
        {
            message = string.Empty;
            try
            {
                int mailConfigId = mailSend.Config != null ? mailSend.Config.AppID : mailSend.AppID;
                var mailSetting = GetMailSettingForSendingMail(
                    mailConfigId,
                    mailSend.Function != null ? mailSend.Function.FuncID : mailSend.FuncID);

                if (mailSetting == null)
                {
                    LogExtension.LogError(new Exception(string.Format("Application's setting \"{0}\" which was not supported.", mailSend.Config.AppID)));
                }

                if (mailSetting.Port == Constant.Port465)
                {
                    SendMailThroughImplicitSSL(mailConfigId, mailSetting, mailSend);
                }
                else
                {
                    var msg = new MailMessage();
                    var mailFrom = GetMailFrom(mailConfigId, mailSetting, mailSend);

                    msg.From = new System.Net.Mail.MailAddress(mailFrom, mailSend.Title, Encoding.UTF8);
                    if (!string.IsNullOrEmpty(mailSend.MailTo))
                    {
                        mailSend.MailTo = RemoveDoubleComma(mailSend.MailTo);
                        mailSend.MailTo.Split(';').ForEach<string>(toMail =>
                        {
                            if (!string.IsNullOrEmpty(toMail))
                            {
                                msg.To.Add(toMail);
                            }
                        });
                    }
                    else
                    {
                        message = LogMailError(mailSend, new Exception("Mail To cannot be null or empty."));
                        return;
                    }

                    if (!string.IsNullOrEmpty(mailSend.MailCc))
                    {
                        mailSend.MailCc = RemoveDoubleComma(mailSend.MailCc);
                        mailSend.MailCc.Split(';').ForEach<string>(ccMail =>
                        {
                            if (!string.IsNullOrEmpty(ccMail))
                            {
                                msg.CC.Add(ccMail);
                            }
                        });
                    }

                    if (!string.IsNullOrEmpty(mailSend.MailBcc))
                    {
                        mailSend.MailBcc = RemoveDoubleComma(mailSend.MailBcc);
                        mailSend.MailBcc.Split(';').ForEach<string>(bccMail =>
                            {
                                if (!string.IsNullOrEmpty(bccMail))
                                {
                                    msg.Bcc.Add(bccMail);
                                }
                            });
                    }

                    msg.Subject = mailSend.Subject;
                    msg.SubjectEncoding = Encoding.UTF8;
                    msg.IsBodyHtml = true;
                    msg.Body = mailSend.Body;
                    msg.BodyEncoding = Encoding.UTF8;
                    msg.Priority = System.Net.Mail.MailPriority.Normal;

                    //// http://support.microsoft.com/kb/2183292
                    var attInfo = MailSection.Instance.Attachment;
                    var attachments = mailSend.Attachments;
                    if (attachments != null)
                    {
                        foreach (var attachFile in attachments)
                        {
                            var contType = attInfo.Extensions[attachFile.ContentType.ToString()].Type;
                            attachFile.Content.Seek(0, SeekOrigin.Begin);
                            var attachment = new System.Net.Mail.Attachment(attachFile.Content, attachFile.FileName, contType);
                            msg.Attachments.Add(attachment);
                        }
                    }

                    var alternateView = mailSend.AlternateView;
                    if (alternateView != null)
                    {
                        AlternateView altView = AlternateView.CreateAlternateViewFromString(msg.Body, Encoding.UTF8, MediaTypeNames.Text.Html);
                        foreach (var resoure in alternateView)
                        {
                            var imageType = attInfo.Extensions[resoure.ImageType.ToString()].Type;
                            LinkedResource linkedImage = new LinkedResource(resoure.StreamContent, imageType);
                            linkedImage.ContentId = resoure.ContentCID;
                            linkedImage.ContentStream.Seek(0, SeekOrigin.Begin);
                            altView.LinkedResources.Add(linkedImage);
                        }

                        msg.AlternateViews.Add(altView);
                    }

                    var client = new System.Net.Mail.SmtpClient
                    {
                        Port = mailSetting.Port,
                        Host = mailSetting.Server
                    };
                    if (mailSetting.IsAuth)
                    {
                        client.Credentials = new System.Net.NetworkCredential(mailSetting.Username, mailSetting.Password);
                        client.EnableSsl = true;
                    }

                    System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;

                    SafeSmtpClientSendMessage(client, msg, mailSend);
                }
            }
            catch (Exception ex)
            {
                message = LogMailError(mailSend, ex);
            }
        }

        private static void SafeSmtpClientSendMessage(System.Net.Mail.SmtpClient smtpClient, MailMessage msg, MailRecord mailRecord)
        {
            try
            {
                smtpClient.Send(msg);
                UpdateMailStatus(mailRecord, true, null);
            }
            catch (Exception e)
            {
                LogExtension.LogError(e);
                UpdateMailStatus(mailRecord, false, e);
            }
        }

        /// <summary>
        /// Add mail to queue for another service read and send mail.
        /// </summary>
        /// <param name="mailRecord">Mail record information.</param>
        public static void AddMailToQueueForSendingMail(object mailRecord)
        {
            var record = mailRecord as MailRecord;
            if (record != null)
            {
                IMessageFormatter msgFormat;
                try
                {
                    LogExtension.LogMessage(string.Format("\r\nRecieved Email from WCF: {0}", record.Subject));
                    msgFormat = new BinaryMessageFormatter();

                    // Set the formatter to binary
                    var message = new Message(record, msgFormat);
                    message.Priority = MessagePriority.High;
                    message.Label = string.Format("MailRecord_{0}", record.Config != null ? record.Config.AppID : record.AppID);

                    using (var msmq = new MessageQueue(AppSettings.QueuePath, QueueAccessMode.Send))
                    {
                        // Set the formatter to binary
                        msmq.Formatter = msgFormat;
                        msmq.Send(message);
                    }
                }
                catch (Exception ex)
                {
                    Fanex.Logging.Logger.Log.Info(ex.Message + "\nMail Subject: " + record.Subject);
                    //// Send without queue
                    record.MailID = (int)(new MailDeliverBusiness()).InsertMailRecord(record);
                    record.IsReady = true;
                    var outputMessage = string.Empty;
                    SendMail(record, out outputMessage);
                }
            }
        }

        private static string LogMailError(MailRecord mail, Exception ex)
        {
            LogExtension.LogError(ex);
            UpdateMailStatus(mail, false, ex);
            return ex.Message;
        }

        /// <summary>
        /// Get Setting information for send mail, from App Id and Function Id.
        /// </summary>
        /// <param name="appID">Application Id.</param>
        /// <param name="funcID">Function Id.</param>
        /// <returns>Mail Setting contain information of Mail Server.</returns>
        private static MailSetting GetMailSettingForSendingMail(int appID, int funcID)
        {
            if (appID > 0)
            {
                int settingID = 0;
                var mailManager = new MailDeliverDataAccess();
                //// Check mail function
                if (funcID > 0)
                {
                    var funcInfo = mailManager.GetMailFunction(funcID);
                    if (funcInfo != null && funcInfo.Config.AppID == appID)
                    {
                        settingID = funcInfo.Setting.SettingID;
                    }
                }
                //// Check app mail
                if (settingID == 0)
                {
                    var appConfig = mailManager.GetMailConfig(appID);
                    if (appConfig != null)
                    {
                        settingID = appConfig.Setting.SettingID;
                    }
                }
                //// Get mai setting
                if (settingID > 0)
                {
                    var mailSetting = mailManager.GetMailSetting(settingID);
                    return mailSetting;
                }
            }

            return null;
        }

        /// <summary>
        /// Update mail status after sent mail, if fail, a thread will add mail record to queue for resend mail.
        /// </summary>
        /// <param name="mailRecord">Mail information.</param>
        /// <param name="status">Status of send mail process.</param>
        /// <param name="error">Error information.</param>
        private static void UpdateMailStatus(MailRecord mailRecord, bool status, Exception error)
        {
            if (mailRecord.MailID > 0)
            {
                try
                {
                    var mailManager = new MailDeliverBusiness();
                    mailManager.UpdateMailStatus(mailRecord.MailID, status);

                    //// Resend mail if retry = true and resentcount > 0
                    if (status == false
                       ////&& mailRecord.CanRetry
                       && mailRecord.ResentCount > 0)
                    {
                        ////mailRecord.ResentCount--;
                        ////ThreadPool.QueueUserWorkItem(new WaitCallback(AddMailToQueueForSendingMail), (object)mailRecord);
                        lock (ResendMailWorker.ListMailLockObject)
                        {
                            ResendMailWorker.ListMailFailed.Add(mailRecord);
                        }
                    }

                    if (error != null)
                    {
                        var currentAppId = mailRecord.AppID > 0 ? mailRecord.AppID : (mailRecord.Config != null ? mailRecord.Config.AppID : 0);
                        //// Log Error to DB
                        var mailDeliverBusiness = new MailDeliverBusiness();
                        mailDeliverBusiness.InsertMailError(new InsertMailErrorCriteria
                        {
                            AppID = currentAppId,
                            ErrorTime = DateTime.Now,
                            MailID = mailRecord.MailID,
                            Message = error.Message,
                            StackTrace = error.ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogExtension.LogError(ex);
                }
            }
        }

        /// <summary>
        /// Remove double comma in email address.
        /// </summary>
        /// <param name="mails">Mail list string.</param>
        /// <returns>Mail list with only one comma.</returns>
        private static string RemoveDoubleComma(string mails)
        {
            while (mails.Contains(",,"))
            {
                mails = mails.Replace(",,", ",");
            }

            while (mails.Contains(";;"))
            {
                mails = mails.Replace(";;", ";");
            }

            return mails.TrimEnd(new char[] { ',', ';' });
        }

        /// <summary>
        /// Send mail through implicit SSL.
        /// </summary>
        /// <param name="mailConfigId">Mail config.</param>
        /// <param name="mailSetting">Mail setting.</param>
        /// <param name="mailRecord">Mail information.</param>
        [SuppressMessage("StyleCop.CSharp.StylecopPlus.Rules", "SP2101:MethodMustNotContainMoreLinesThan", Justification = "Reviewed. Suppression is OK here.")]
        private static void SendMailThroughImplicitSSL(int mailConfigId, MailSetting mailSetting, MailRecord mailRecord)
        {
            SmtpMail oMail = new SmtpMail("TryIt");
            EASendMail.SmtpClient oSmtp = new EASendMail.SmtpClient();

            // Set sender email address, please change it to yours
            oMail.From = GetMailFrom(mailConfigId, mailSetting, mailRecord);

            // Set recipient email address, please change it to yours
            oMail.To = mailRecord.MailTo;

            if (!string.IsNullOrEmpty(mailRecord.MailCc))
            {
                mailRecord.MailCc = RemoveDoubleComma(mailRecord.MailCc);
                mailRecord.MailCc.Split(';').ForEach<string>(ccMail =>
                {
                    if (!string.IsNullOrEmpty(ccMail))
                    {
                        oMail.Cc.Add(ccMail);
                    }
                });
            }

            if (!string.IsNullOrEmpty(mailRecord.MailBcc))
            {
                mailRecord.MailBcc = RemoveDoubleComma(mailRecord.MailBcc);
                mailRecord.MailBcc.Split(';').ForEach<string>(bccMail =>
                {
                    if (!string.IsNullOrEmpty(bccMail))
                    {
                        oMail.Bcc.Add(bccMail);
                    }
                });
            }

            // Set email subject
            oMail.Subject = mailRecord.Subject;

            // Set email body
            oMail.HtmlBody = mailRecord.Body;

            // Your SMTP server address
            SmtpServer oServer = new SmtpServer(mailSetting.Server);

            // User and password for ESMTP authentication, if your server doesn't require
            // User authentication, please remove the following codes.
            oServer.User = mailSetting.Username;
            oServer.Password = mailSetting.Password;

            // Set 465 SMTP port
            oServer.Port = mailSetting.Port;

            // Enable SSL connection
            oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

            try
            {
                oSmtp.SendMail(oServer, oMail);
                UpdateMailStatus(mailRecord, true, null);
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
                UpdateMailStatus(mailRecord, false, ex);
            }
        }

        /// <summary>
        /// Get mail from mail config.
        /// </summary>
        /// <param name="mailConfigId">Mail config id.</param>
        /// <param name="mailSetting">Mail setting.</param>
        /// <param name="mailSend">Mail information for send.</param>
        /// <returns>Mail from information.</returns>
        private static string GetMailFrom(int mailConfigId, MailSetting mailSetting, MailRecord mailSend)
        {
            var mailFrom = mailSetting.Username;
            if (!string.IsNullOrEmpty(mailSend.MailFrom))
            {
                mailFrom = mailSend.MailFrom;
            }

            if (mailConfigId > 0)
            {
                var mailManager = new MailDeliverDataAccess();
                var appConfig = mailManager.GetMailConfig(mailConfigId);
                if (appConfig != null && !string.IsNullOrEmpty(appConfig.MailFrom))
                {
                    mailFrom = appConfig.MailFrom;
                }
            }

            return mailFrom;
        }
    }
}