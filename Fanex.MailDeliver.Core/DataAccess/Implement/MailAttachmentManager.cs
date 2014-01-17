// --------------------------------------------------------------------------
// <copyright file="MailAttachmentManager.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.DataAccess.Implement
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using Fanex.Common;
    using Fanex.Data;
    using Fanex.MailDeliver.Core.Infrastructure.Configurations;
    using Fanex.MailDeliver.Core.Infrastructure.Logging;
    using Fanex.MailDeliver.Model;

    /// <summary>
    /// Storage Attachments.
    /// </summary>
    public class MailAttachmentManager
    {
        private long _mailID;

        /// <summary>
        /// Save Attachment.
        /// </summary>
        /// <param name="mailID">Mail record id.</param>
        /// <param name="attachments">List attachment files.</param>
        public void StoreMailAttachments(
            long mailID,
            IEnumerable<MailAttachment> attachments)
        {
            try
            {
                _mailID = mailID;
                var mailInfo = MailSection.Instance;
                switch (mailInfo.Attachment.StoreMode)
                {
                    case MailSection.AttachmentStoreMode.Binary:
                        StoreMailAttchmentsToDatabase(mailID, attachments);
                        break;
                    case MailSection.AttachmentStoreMode.File:
                        StoreMailAttachmentsToFiles(mailInfo, attachments);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }
        }

        /// <summary>
        /// Get attachment files.
        /// </summary>
        /// <param name="mailID">Mail record ID.</param>
        /// <param name="attInfo">Attachment Info.</param>
        /// <returns>List attachment files.</returns>
        public IEnumerable<MailAttachment> GetMailAttachments(
            long mailID,
            string attInfo)
        {
            try
            {
                _mailID = mailID;
                var mailInfo = MailSection.Instance;
                switch (mailInfo.Attachment.StoreMode)
                {
                    case MailSection.AttachmentStoreMode.File:
                        return GetMailAttachmentsFromFiles(mailInfo, attInfo);
                    case MailSection.AttachmentStoreMode.Binary:
                        return GetMailAttachmentsFromDatabase(mailInfo, mailID);
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Store attachment to files.
        /// </summary>
        /// <param name="mailSection">Mail contains information.</param>
        /// <param name="attachments">List attachment files.</param>
        private void StoreMailAttachmentsToFiles(
            MailSection mailSection,
            IEnumerable<MailAttachment> attachments)
        {
            if (attachments.Count() > 0)
            {
                foreach (var attachment in attachments)
                {
                    var fileName = string.Format("{0}{1}", mailSection.Attachment.Path, attachment.FileName);
                    var stream = File.OpenWrite(fileName);
                    attachment.Content.WriteTo(stream);
                    stream.Flush();
                    stream.Close();
                }
            }
        }

        /// <summary>
        /// Store attachment to Database.
        /// </summary>
        /// <param name="mailID">Mail record id.</param>
        /// <param name="attachments">List Files.</param>
        private void StoreMailAttchmentsToDatabase(
            long mailID,
            IEnumerable<MailAttachment> attachments)
        {
            if (mailID > 0 && attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    if (attachment.Content != null)
                    {
                        var param = new Hashtable();
                        param.Add("mailid", mailID);
                        param.Add("filename", attachment.FileName);
                        param.Add("contenttype", attachment.ContentType);
                        param.Add("content", attachment.Content.ToArray());
                        using (ObjectDb objDb = new ObjectDb("StoreMailAttachement"))
                        {
                            ////DbHelper.Execute(spElement.Name, param, spElement.Db);                            
                            objDb.ExecuteNonQuery(param);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get Attachments from files.
        /// </summary>
        /// <param name="mailInfo">Mail information.</param>
        /// <param name="info">Information string.</param>
        /// <returns>List attachments.</returns>
        private IEnumerable<MailAttachment> GetMailAttachmentsFromFiles(
            MailSection mailInfo,
            string info)
        {
            var attachments = new List<MailAttachment>();
            string[] fileNames = info.Split(',');
            if (fileNames.Length > 0)
            {
                foreach (var fileName in fileNames)
                {
                    string ext = Path.GetExtension(fileName).Substring(1).ToLower();
                    var extElm = mailInfo.Attachment.Extensions.Cast<ExtensionElement>().Where(s => s.Name.Equals(ext)).SingleOrDefault();
                    var attachment = new MailAttachment
                    {
                        MailID = _mailID,
                        FileName = fileName
                    };
                    string fullPath = string.Format("{0}{1}", mailInfo.Attachment.Path, fileName);
                    if (File.Exists(fullPath))
                    {
                        var fileStream = File.Open(fullPath, FileMode.Open);
                        var buffer = new byte[fileStream.Length];
                        fileStream.Read(buffer, 0, buffer.Length);
                        var stream = new MemoryStream(buffer);
                        attachment.Content = stream;
                    }

                    attachments.Add(attachment);
                }
            }

            return attachments;
        }

        /// <summary>
        /// Get List attachment files from Database.
        /// </summary>
        /// <param name="mailInfo">Mail information.</param>
        /// <param name="mailID">Mail record id.</param>
        /// <returns>List attachment mails.</returns>
        private IEnumerable<MailAttachment> GetMailAttachmentsFromDatabase(
            MailSection mailInfo,
            long mailID)
        {
            var attachments = new List<MailAttachment>();
            if (mailID > 0)
            {
                var param = new Hashtable();
                param.Add("mailid", mailID);
                using (ObjectDb objDb = new ObjectDb("GetMailAttachement"))
                {
                    var result = objDb.ExecuteDataSet(param);
                    if (result.Tables.Count > 0)
                    {
                        var rows = result.Tables[0].Rows;
                        foreach (DataRow row in rows)
                        {
                            var ext = Converter.ToString2(row["contenttype"]);
                            var extElm = mailInfo.Attachment.Extensions.Cast<ExtensionElement>().Where(s => s.Name.Equals(ext)).SingleOrDefault();
                            var item = new MailAttachment
                            {
                                MailID = _mailID,
                                FileName = Converter.ToString2(row["filename"]),
                                ContentType = (FileExtentions)Enum.Parse(typeof(FileExtentions), Converter.ToString2(row["contenttype"]), true)
                            };
                            byte[] binary = row["content"] as byte[];
                            item.Content = new MemoryStream(binary);
                            attachments.Add(item);
                        }
                    }
                }
            }

            return attachments;
        }
    }
}