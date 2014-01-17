// --------------------------------------------------------------------------
// <copyright file="MailLinkedResource.cs" company="Nexcel Solutions Vietnam">
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

    /// <summary>
    /// Mail link resource information.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MailLinkedResource
    {
        private string __fileName = string.Empty;
        private FileExtentions __imageType;
        private string __contentID = string.Empty;
        private System.IO.MemoryStream __streamContent;

        public MailLinkedResource()
        {
            ////TODO
        }

        public MailLinkedResource(string fileName, FileExtentions imageType, string contentID)
        {
            ////TODO
            __fileName = fileName;
            __imageType = imageType;
            __contentID = contentID;
        }

        public MailLinkedResource(System.IO.MemoryStream streamContent, FileExtentions imageType, string contentID)
        {
            ////TODO
            __streamContent = streamContent;
            __imageType = imageType;
            __contentID = contentID;
        }

        [DataMember]
        public int LinkedID
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
                return __fileName;
            }

            set
            {
                __fileName = value;
            }
        }

        [DataMember]
        public FileExtentions ImageType
        {
            get
            {
                return __imageType;
            }

            set
            {
                __imageType = value;
            }
        }

        [DataMember]
        public string ContentCID
        {
            get
            {
                return __contentID;
            }

            set
            {
                __contentID = value;
            }
        }

        [DataMember]
        public System.IO.MemoryStream StreamContent
        {
            get
            {
                return __streamContent;
            }

            set
            {
                __streamContent = value;
            }
        }
    }
}