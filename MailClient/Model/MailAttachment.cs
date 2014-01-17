// --------------------------------------------------------------------------
// <copyright file="MailAttachment.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.Model
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public enum FileExtentions
    {
        /// <summary>
        /// Excel file.
        /// </summary>
        MS_EXCEL,

        /// <summary>
        /// Excel file.
        /// </summary>
        MS_DOC,

        /// <summary>
        /// Excel file.
        /// </summary>
        IMG_JPG,

        /// <summary>
        /// Excel file.
        /// </summary>
        IMG_GIF,

        /// <summary>
        /// Excel file.
        /// </summary>
        IMG_PNG,

        /// <summary>
        /// Excel file.
        /// </summary>
        PDF,

        /// <summary>
        /// Excel file.
        /// </summary>
        TEXT
    }

    /// <summary>
    /// Mail attachment information.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MailAttachment
    {
        private string _fileName = "noname";
        private FileExtentions __contentType;
        private System.IO.MemoryStream __content;

        public MailAttachment()
        {
            ////TODO
        }

        public MailAttachment(string fileName, FileExtentions contentType)
        {
            _fileName = fileName;
            __contentType = contentType;
        }

        public MailAttachment(System.IO.MemoryStream content, FileExtentions contentType)
        {
            __content = content;
            __contentType = contentType;
        }

        [DataMember]
        public int AttachmentID
        {
            get;
            set;
        }

        [DataMember]
        public long MailID
        {
            get;
            set;
        }

        [DataMember]
        public string FileName
        {
            get
            {
                return _fileName;
            }

            set
            {
                _fileName = value;
            }
        }

        [DataMember]
        public FileExtentions ContentType
        {
            get
            {
                return __contentType;
            }

            set
            {
                __contentType = value;
            }
        }

        [DataMember]
        public System.IO.MemoryStream Content
        {
            get
            {
                return __content;
            }

            set
            {
                __content = value;
            }
        }
    }
}