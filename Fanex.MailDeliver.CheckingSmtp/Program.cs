// --------------------------------------------------------------------------
// <copyright file="Program.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Edward.Nguyen" date="2013.8.7">Created</change>
// </history>

namespace Fanex.MailDeliver.CheckingSmtp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using Fanex.Logging;
    using Fanex.MailDeliver.DataAccess.Implement;
    using Fanex.MailDeliver.DataAccess.Interface;
    using Fanex.MailDeliver.Model;

    /// <summary>
    /// First running program.
    /// </summary>
    public class Program
    {
        private static IMailDeliverDataAccess _mailDatabaseAccess = new MailDeliverDataAccess();

        /// <summary>
        /// Main function run first.
        /// </summary>
        /// <param name="args">Function arguments.</param>
        public static void Main(string[] args)
        {
            try
            {
                Fanex.Data.Configuration.DbSettingProviderManager.Current.Start();

                var settings = new List<MailSetting>();

                settings = _mailDatabaseAccess.GetListMailSetting();

                string message = string.Empty;

                foreach (var item in settings)
                {
                    message = CheckingSmtp(item);
                    Logger.Log.Info(item.Server + " : " + message);
                    Console.WriteLine(item.Server + " : " + message);
                    Console.WriteLine("----");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Checking SMTP still alive or not.
        /// </summary>
        /// <param name="mailSetting">Mail setting information.</param>
        /// <returns>If empty, then still alive, if have string, then it have error.</returns>
        private static string CheckingSmtp(MailSetting mailSetting)
        {
            string message = string.Empty;

            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(mailSetting.Server, mailSetting.Port);

                    // As GMail requires SSL we should use SslStream
                    // If your SMTP server doesn't support SSL you can
                    // work directly with the underlying stream
                    using (var stream = client.GetStream())
                    {
                        using (var writer = new StreamWriter(stream))
                        using (var reader = new StreamReader(stream))
                        {
                            writer.WriteLine(string.Format("HELO {0}", mailSetting.Server));
                            writer.Flush();
                            message = reader.ReadLine();
                            //// GMail responds with: 220 mx.google.com ESMTP
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                Logger.Log.Error(ex.Message);
            }

            return message;
        }
    }
}