// --------------------------------------------------------------------------
// <copyright file="MailFormatter.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>

namespace Fanex.MailDeliver.MailClient.Util
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Mail formatter information.
    /// </summary>
    public class MailFormatter
    {
        private static BinaryFormatter _formatter = new BinaryFormatter();

        public static MemoryStream Serialize(object obj)
        {
            ////byte[] buffer = new byte[2097152]; // Max 2M
            var stream = new MemoryStream();
            _formatter.Serialize(stream, obj);
            return stream;
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            var stream = new MemoryStream(buffer);
            return (T)_formatter.Deserialize(stream);
        }
    }
}