// --------------------------------------------------------------------------
// <copyright file="MailRecord.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
//   <change who="Ricky.Nguyen" date="2012.9.24">Update MailStatus Enum for getting list MailRecord</change>
// </history>

namespace Fanex.MailDeliver.Model
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Runtime.Serialization;

    public enum MailStatus : int
    {
        /// <summary>
        /// All Mail Type.
        /// </summary>
        All = 0,

        /// <summary>
        /// Success Mail Type.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Fail Mail Type.
        /// </summary>
        Fail = -1
    }

    /// <summary>
    /// All information about mail.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MailRecord
    {
        private bool _isReady = false;

        public MailRecord()
        {
            MailID = 0;
            Config = new MailConfig();
            Function = new MailFunction();
            MailTo = string.Empty;
            Title = string.Empty;
            Body = string.Empty;
            Subject = string.Empty;
            MailTo = string.Empty;
            MailCc = string.Empty;
            MailBcc = string.Empty;
            Attachments = new List<MailAttachment>();
            AlternateView = new List<MailLinkedResource>();
            SentFail = 0;
            SentSuccess = 0;
            CanRetry = true;
            MailFrom = string.Empty;
            IsEncrypt = false;
        }

        [DataMember]
        public long MailID
        {
            get;
            set;
        }

        [DataMember]
        public MailConfig Config
        {
            get;
            set;
        }

        [DataMember]
        public int AppID
        {
            get;
            set;
        }

        [DataMember]
        public int FuncID
        {
            get;
            set;
        }

        [DataMember]
        public MailFunction Function
        {
            get;
            set;
        }

        [DataMember]
        public string MailTo
        {
            get;
            set;
        }

        [DataMember]
        public string Title
        {
            get;
            set;
        }

        [DataMember]
        public string Subject
        {
            get;
            set;
        }

        [DataMember]
        public string Body
        {
            get;
            set;
        }

        [DataMember]
        public string MailCc
        {
            get;
            set;
        }

        [DataMember]
        public string MailFrom
        {
            get;
            set;
        }

        [DataMember]
        public string MailBcc
        {
            get;
            set;
        }

        [DataMember]
        public string FileAttachments
        {
            get;
            set;
        }

        [DataMember]
        public List<MailAttachment> Attachments
        {
            get;
            set;
        }

        [DataMember]
        public string MailDate
        {
            get;
            set;
        }

        [DataMember]
        public string MailUpdatedDate
        {
            get;
            set;
        }

        [DataMember]
        public List<MailLinkedResource> AlternateView
        {
            get;
            set;
        }

        public bool IsReady
        {
            get
            {
                return _isReady;
            }

            set
            {
                _isReady = value;
            }
        }

        [DataMember]
        public int CountSend
        {
            get;
            set;
        }

        [DataMember]
        public int SentSuccess { get; set; }

        [DataMember]
        public int SentFail { get; set; }

        [DataMember]
        public bool CanRetry { get; set; }

        [DataMember]
        public int ResentCount { get; set; }

        [DataMember]
        public bool IsEncrypt
        {
            get;
            set;
        }

        [DataMember]
        public MailPriority Priority
        {
            get;
            set;
        }
    }
}