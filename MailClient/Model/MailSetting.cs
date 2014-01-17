// --------------------------------------------------------------------------
// <copyright file="MailSetting.cs" company="Nexcel Solutions Vietnam">
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
    /// Mail setting information.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MailSetting
    {
        [DataMember]
        public int SettingID
        {
            get;
            set;
        }

        [DataMember]
        public string SettingName
        {
            get;
            set;
        }

        [DataMember]
        public string Server
        {
            get;
            set;
        }

        [DataMember]
        public int Port
        {
            get;
            set;
        }

        [DataMember]
        public string Username
        {
            get;
            set;
        }

        [DataMember]
        public string Password
        {
            get;
            set;
        }

        [DataMember]
        public bool IsAuth
        {
            get;
            set;
        }
    }
}