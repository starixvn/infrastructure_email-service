// --------------------------------------------------------------------------
// <copyright file="MailFunction.cs" company="Nexcel Solutions Vietnam">
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
    /// Mail function information.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MailFunction
    {
        [DataMember]
        public int FuncID
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
        public MailConfig Config
        {
            get;
            set;
        }

        [DataMember]
        public string FuncName
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
    }
}