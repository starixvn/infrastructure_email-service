// --------------------------------------------------------------------------
// <copyright file="StorageAdapter.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Ricky.Nguyen" date="2012.9.20">Created</change>
// </history>
namespace Fanex.MailDeliver.Core.Infrastructure.Configurations
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Storage adapter section.
    /// </summary>
    public class StorageAdapter : ConfigurationSection
    {
        public enum StorageType
        {
            /// <summary>
            /// SQL server type.
            /// </summary>
            SqlServer
        }

        public static StorageAdapter Instance
        {
            get
            {
                return ConfigurationManager.GetSection("StorageAdapter") as StorageAdapter;
            }
        }

        [ConfigurationProperty("Server", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(StorageServer),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public StorageServer Server
        {
            get
            {
                var server = base["Server"] as StorageServer;
                return server;
            }
        }

        [ConfigurationProperty("active", IsRequired = true)]
        public StorageType Active
        {
            get
            {
                return (StorageType)base["active"];
            }
        }
    }

    /// <summary>
    /// Storage Server.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public class StorageServer : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public new StorageElement this[string name]
        {
            get
            {
                return (StorageElement)BaseGet(name);
            }
        }

        public void Add(StorageElement storageElement)
        {
            BaseAdd(storageElement);
        }

        public void Remove(StorageElement storageElement)
        {
            if (BaseIndexOf(storageElement) >= 0)
            {
                BaseRemove(storageElement.Name);
            }
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string alias)
        {
            BaseRemove(alias);
        }

        public void Clear()
        {
            BaseClear();
        }

        public int IndexOf(StorageElement storageElement)
        {
            return BaseIndexOf(storageElement);
        }

        protected override sealed ConfigurationElement CreateNewElement()
        {
            return new StorageElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((StorageElement)element).Name;
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }
    }

    /// <summary>
    /// Storage Element.
    /// </summary>
    public class StorageElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
        }

        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get
            {
                return (string)this["host"];
            }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
        }

        [ConfigurationProperty("username", IsRequired = false)]
        public string Username
        {
            get
            {
                return (string)this["username"];
            }
        }

        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get
            {
                return (string)this["password"];
            }
        }
    }
}