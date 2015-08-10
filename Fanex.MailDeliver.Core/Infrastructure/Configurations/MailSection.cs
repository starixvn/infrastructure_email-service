// --------------------------------------------------------------------------
// <copyright file="MailSection.cs" company="Nexcel Solutions Vietnam">
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
    /// Mail configuration section.
    /// </summary>
    public class MailSection : ConfigurationSection
    {
        public enum AttachmentStoreMode
        {
            /// <summary>
            /// Binary type.
            /// </summary>
            Binary,

            /// <summary>
            /// File type.
            /// </summary>
            File
        }

        public static MailSection Instance
        {
            get
            {
                var mailSection = ConfigurationManager.GetSection("MailSection") as MailSection;
                return mailSection;
            }
        }

        [ConfigurationProperty("Attachment", IsRequired = true)]
        public AttachmentElement Attachment
        {
            get
            {
                return (AttachmentElement)this["Attachment"];
            }
        }
    }

    /// <summary>
    /// Mail Server.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public class MailServer : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public new MailServerElement this[string id]
        {
            get
            {
                return (MailServerElement)BaseGet(id);
            }
        }

        public void Add(MailServerElement mailServerElement)
        {
            BaseAdd(mailServerElement);
        }

        public void Remove(MailServerElement mailServerElement)
        {
            if (BaseIndexOf(mailServerElement) >= 0)
            {
                BaseRemove(mailServerElement.Name);
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

        public int IndexOf(MailServerElement mailServerElement)
        {
            return BaseIndexOf(mailServerElement);
        }

        protected override sealed ConfigurationElement CreateNewElement()
        {
            return new MailServerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MailServerElement)element).ID;
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }
    }

    /// <summary>
    /// Mail Server Element.
    /// </summary>
    public class MailServerElement : ConfigurationElement
    {
        [ConfigurationProperty("id", IsRequired = true)]
        public string ID
        {
            get
            {
                return (string)this["id"];
            }
        }

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

        [ConfigurationProperty("auth", IsRequired = false, DefaultValue = true)]
        public bool Auth
        {
            get
            {
                return (bool)this["auth"];
            }
        }
    }

    /// <summary>
    /// Mail attachment Element.
    /// </summary>
    public class AttachmentElement : ConfigurationElement
    {
        [ConfigurationProperty("storeMode", IsRequired = true)]
        public MailSection.AttachmentStoreMode StoreMode
        {
            get
            {
                return (MailSection.AttachmentStoreMode)this["storeMode"];
            }
        }

        [ConfigurationProperty("path", IsRequired = false)]
        public string Path
        {
            get
            {
                return (string)this["path"];
            }
        }

        [ConfigurationProperty("Extensions", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(FileExtension),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public FileExtension Extensions
        {
            get
            {
                var extensions = base["Extensions"] as FileExtension;
                return extensions;
            }
        }
    }

    /// <summary>
    /// File extension information.
    /// </summary>
    public class FileExtension : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public new ExtensionElement this[string name]
        {
            get
            {
                return (ExtensionElement)BaseGet(name);
            }
        }

        public void Add(ExtensionElement extensionElement)
        {
            BaseAdd(extensionElement);
        }

        public void Remove(ExtensionElement extensionElement)
        {
            if (BaseIndexOf(extensionElement) >= 0)
            {
                BaseRemove(extensionElement.Name);
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

        public int IndexOf(ExtensionElement extensionElement)
        {
            return BaseIndexOf(extensionElement);
        }

        protected override sealed ConfigurationElement CreateNewElement()
        {
            return new ExtensionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExtensionElement)element).Name;
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }
    }

    /// <summary>
    /// Extension element.
    /// </summary>
    public class ExtensionElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return (string)this["type"];
            }
        }
    }
}