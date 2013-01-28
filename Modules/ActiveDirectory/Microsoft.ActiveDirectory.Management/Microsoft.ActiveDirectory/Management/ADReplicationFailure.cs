using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADReplicationFailure : ADEntity
	{
		public int FailureCount
		{
			get
			{
				return (int)base.GetValue("FailureCount");
			}
		}

		public ADReplicationFailureType FailureType
		{
			get
			{
				return (ADReplicationFailureType)base.GetValue("FailureType");
			}
		}

		public DateTime FirstFailureTime
		{
			get
			{
				return (DateTime)base.GetValue("FirstFailureTime");
			}
		}

		public int LastError
		{
			get
			{
				return (int)base.GetValue("LastError");
			}
		}

		public string Partner
		{
			get
			{
				return (string)base.GetValue("Partner");
			}
		}

		public Guid? PartnerGuid
		{
			get
			{
				return (Guid?)base.GetValue("PartnerGuid");
			}
		}

		public string Server
		{
			get
			{
				return (string)base.GetValue("Server");
			}
		}

		static ADReplicationFailure()
		{
			ADEntity.RegisterMappingTable(typeof(ADReplicationFailure), ADReplicationFailureFactory<ADReplicationFailure>.AttributeTable);
		}

		public ADReplicationFailure()
		{
		}
	}
}