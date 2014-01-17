// --------------------------------------------------------------------------
// <copyright file="MailError.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2013.7.10">Created</change>
// </history>

namespace Fanex.MailDeliver.Model
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MailError
    {
        [DataMember]
        public long ErrorID { get; set; }

        [DataMember]
        public long MailID { get; set; }

        [DataMember]
        public int AppID { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string StackTrace { get; set; }

        [DataMember]
        public string SettingName { get; set; }

        [DataMember]
        public DateTime ErrorTime { get; set; }
    }
}