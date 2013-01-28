using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADReplicationQueueOperation : ADEntity
	{
		public DateTime EnqueueTime
		{
			get
			{
				return (DateTime)base.GetValue("EnqueueTime");
			}
		}

		public int OperationID
		{
			get
			{
				return (int)base.GetValue("OperationID");
			}
		}

		public ADReplicationOperationType OperationType
		{
			get
			{
				return (ADReplicationOperationType)((int)base.GetValue("OperationType"));
			}
		}

		public int Options
		{
			get
			{
				return (int)base.GetValue("Options");
			}
		}

		public string Partition
		{
			get
			{
				return (string)base.GetValue("Partition");
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

		public int Priority
		{
			get
			{
				return (int)base.GetValue("Priority");
			}
		}

		public string Server
		{
			get
			{
				return (string)base.GetValue("Server");
			}
		}

		static ADReplicationQueueOperation()
		{
			ADEntity.RegisterMappingTable(typeof(ADReplicationQueueOperation), ADReplicationQueueOperationFactory<ADReplicationQueueOperation>.AttributeTable);
		}

		public ADReplicationQueueOperation()
		{
		}
	}
}