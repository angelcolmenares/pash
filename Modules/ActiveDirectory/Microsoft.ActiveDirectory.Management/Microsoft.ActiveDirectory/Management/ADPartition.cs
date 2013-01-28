using System;

namespace Microsoft.ActiveDirectory.Management
{
	public abstract class ADPartition : ADObject
	{
		public string DeletedObjectsContainer
		{
			get
			{
				return base.GetValue("DeletedObjectsContainer") as string;
			}
		}

		public string DNSRoot
		{
			get
			{
				return base.GetValue("DNSRoot") as string;
			}
		}

		public string LostAndFoundContainer
		{
			get
			{
				return base.GetValue("LostAndFoundContainer") as string;
			}
		}

		public string QuotasContainer
		{
			get
			{
				return base.GetValue("QuotasContainer") as string;
			}
		}

		public ADPropertyValueCollection ReadOnlyReplicaDirectoryServers
		{
			get
			{
				return base["ReadOnlyReplicaDirectoryServers"];
			}
		}

		public ADPropertyValueCollection ReplicaDirectoryServers
		{
			get
			{
				return base["ReplicaDirectoryServers"];
			}
		}

		public ADPropertyValueCollection SubordinateReferences
		{
			get
			{
				return base["SubordinateReferences"];
			}
		}

		public ADPartition()
		{
		}

		public ADPartition(string identity) : base(identity)
		{
		}

		public ADPartition(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADPartition(ADObject adobject)
		{
			if (adobject != null)
			{
				base.Identity = adobject;
				if (adobject.IsSearchResult)
				{
					base.SessionInfo = adobject.SessionInfo;
				}
				return;
			}
			else
			{
				throw new ArgumentException("adobject");
			}
		}
	}
}