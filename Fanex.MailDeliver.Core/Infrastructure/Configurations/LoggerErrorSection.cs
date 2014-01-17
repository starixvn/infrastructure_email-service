// --------------------------------------------------------------------------
// <copyright file="LoggerErrorSection.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2013.7.1">Created</change>
// </history>

namespace Fanex.MailDeliver.Core.Infrastructure.Configurations
{
    using System.Configuration;
    using Fanex.Common;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class LoggerErrorSection : ConfigurationSection
    {
        public static LoggerErrorSection Instance
        {
            get
            {
                var loggerErrorSection = ConfigurationManager.GetSection("LoggerErrorSection") as LoggerErrorSection;
                return loggerErrorSection;
            }
        }

        [ConfigurationProperty("appSendMailId", IsRequired = true)]
        public int AppSendMailId
        {
            get
            {
                return Converter.ToInt(this["appSendMailId"]);
            }
        }

        [ConfigurationProperty("toMails", IsRequired = true)]
        public string ToMails
        {
            get
            {
                return (string)this["toMails"];
            }
        }
    }
}