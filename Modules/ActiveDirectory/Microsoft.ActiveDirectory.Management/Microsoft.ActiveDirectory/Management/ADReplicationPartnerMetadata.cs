using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADReplicationPartnerMetadata : ADEntity
	{
		public bool CompressChanges
		{
			get
			{
				return (bool)base.GetValue("CompressChanges");
			}
		}

		public int ConsecutiveReplicationFailures
		{
			get
			{
				return (int)base.GetValue("ConsecutiveReplicationFailures");
			}
		}

		public bool DisableScheduledSync
		{
			get
			{
				return (bool)base.GetValue("DisableScheduledSync");
			}
		}

		public bool IgnoreChangeNotifications
		{
			get
			{
				return (bool)base.GetValue("IgnoreChangeNotifications");
			}
		}

		public string IntersiteTransport
		{
			get
			{
				return (string)base.GetValue("IntersiteTransport");
			}
		}

		public Guid IntersiteTransportGuid
		{
			get
			{
				return (Guid)base.GetValue("IntersiteTransportGuid");
			}
		}

		public ADInterSiteTransportProtocolType IntersiteTransportType
		{
			get
			{
				return (ADInterSiteTransportProtocolType)base.GetValue("IntersiteTransportType");
			}
		}

		public DateTime LastReplicationAttempt
		{
			get
			{
				return (DateTime)base.GetValue("LastReplicationAttempt");
			}
		}

		public int LastReplicationResult
		{
			get
			{
				return (int)base.GetValue("LastReplicationResult");
			}
		}

		public DateTime LastReplicationSuccess
		{
			get
			{
				return (DateTime)base.GetValue("LastReplicationSuccess");
			}
		}

		public string Partition
		{
			get
			{
				return (string)base.GetValue("Partition");
			}
		}

		public Guid PartitionGuid
		{
			get
			{
				return (Guid)base.GetValue("PartitionGuid");
			}
		}

		public string Partner
		{
			get
			{
				return (string)base.GetValue("Partner");
			}
		}

		public string PartnerAddress
		{
			get
			{
				return (string)base.GetValue("PartnerAddress");
			}
		}

		public Guid PartnerGuid
		{
			get
			{
				return (Guid)base.GetValue("PartnerGuid");
			}
		}

		public Guid PartnerInvocationId
		{
			get
			{
				return (Guid)base.GetValue("PartnerInvocationId");
			}
		}

		public ADPartnerType PartnerType
		{
			get
			{
				return (ADPartnerType)base.GetValue("PartnerType");
			}
		}

		public bool ScheduledSync
		{
			get
			{
				return (bool)base.GetValue("ScheduledSync");
			}
		}

		public string Server
		{
			get
			{
				return (string)base.GetValue("Server");
			}
		}

		public bool SyncOnStartup
		{
			get
			{
				return (bool)base.GetValue("SyncOnStartup");
			}
		}

		public bool TwoWaySync
		{
			get
			{
				return (bool)base.GetValue("TwoWaySync");
			}
		}

		public long UsnFilter
		{
			get
			{
				return (long)base.GetValue("UsnFilter");
			}
		}

		public bool Writable
		{
			get
			{
				return (bool)base.GetValue("Writable");
			}
		}

		static ADReplicationPartnerMetadata()
		{
			ADEntity.RegisterMappingTable(typeof(ADReplicationPartnerMetadata), ADReplicationPartnerMetadataFactory<ADReplicationPartnerMetadata>.AttributeTable);
		}

		public ADReplicationPartnerMetadata()
		{
		}
	}
}