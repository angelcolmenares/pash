using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADReplicationUpToDatenessVectorTable : ADEntity
	{
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

		public Guid? PartitionGuid
		{
			get
			{
				return (Guid?)base.GetValue("PartitionGuid");
			}
		}

		public string Partner
		{
			get
			{
				return (string)base.GetValue("Partner");
			}
		}

		public Guid? PartnerInvocationId
		{
			get
			{
				return (Guid?)base.GetValue("PartnerInvocationId");
			}
		}

		public string Server
		{
			get
			{
				return (string)base.GetValue("Server");
			}
		}

		public long UsnFilter
		{
			get
			{
				return (long)base.GetValue("UsnFilter");
			}
		}

		static ADReplicationUpToDatenessVectorTable()
		{
			ADEntity.RegisterMappingTable(typeof(ADReplicationUpToDatenessVectorTable), ADReplicationUpToDatenessVectorTableFactory<ADReplicationUpToDatenessVectorTable>.AttributeTable);
		}

		public ADReplicationUpToDatenessVectorTable()
		{
		}
	}
}