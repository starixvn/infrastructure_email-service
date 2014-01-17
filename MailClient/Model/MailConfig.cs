// --------------------------------------------------------------------------
// <copyright file="MailConfig.cs" company="Nexcel Solutions Vietnam">
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
    /// Mail config information.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MailConfig
    {
        [DataMember]
        public int AppID
        {
            get;
            set;
        }

        [DataMember]
        public string AppName
        {
            get;
            set;
        }

        [DataMember]
        public string Description
        {
            get;
            set;
        }

        [DataMember]
        public MailSetting Setting
        {
            get;
            set;
        }

        [DataMember]
        public int SettingID
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
        public bool IsEncrypt
        {
            get;
            set;
        }
    }
}