using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADReplicationAttributeMetadata : ADEntity
	{
		public string AttributeName
		{
			get
			{
				return (string)base.GetValue("AttributeName");
			}
		}

		public object AttributeValue
		{
			get
			{
				return base.GetValue("AttributeValue");
			}
		}

		public DateTime FirstOriginatingCreateTime
		{
			get
			{
				return (DateTime)base.GetValue("FirstOriginatingCreateTime");
			}
		}

		public bool IsLinkValue
		{
			get
			{
				return (bool)base.GetValue("IsLinkValue");
			}
		}

		public string LastOriginatingChangeDirectoryServerIdentity
		{
			get
			{
				return (string)base.GetValue("LastOriginatingChangeDirectoryServerIdentity");
			}
		}

		public Guid? LastOriginatingChangeDirectoryServerInvocationId
		{
			get
			{
				return (Guid?)base.GetValue("LastOriginatingChangeDirectoryServerInvocationId");
			}
		}

		public DateTime LastOriginatingChangeTime
		{
			get
			{
				return (DateTime)base.GetValue("LastOriginatingChangeTime");
			}
		}

		public long LastOriginatingChangeUsn
		{
			get
			{
				return (long)base.GetValue("LastOriginatingChangeUsn");
			}
		}

		public DateTime LastOriginatingDeleteTime
		{
			get
			{
				return (DateTime)base.GetValue("LastOriginatingDeleteTime");
			}
		}

		public long LocalChangeUsn
		{
			get
			{
				return (long)base.GetValue("LocalChangeUsn");
			}
		}

		public string Object
		{
			get
			{
				return (string)base.GetValue("Object");
			}
		}

		public string Server
		{
			get
			{
				return (string)base.GetValue("Server");
			}
		}

		public int Version
		{
			get
			{
				return (int)base.GetValue("Version");
			}
		}

		static ADReplicationAttributeMetadata()
		{
			ADEntity.RegisterMappingTable(typeof(ADReplicationAttributeMetadata), ADReplicationAttributeMetadataFactory<ADReplicationAttributeMetadata>.AttributeTable);
		}

		public ADReplicationAttributeMetadata()
		{
		}
	}
}