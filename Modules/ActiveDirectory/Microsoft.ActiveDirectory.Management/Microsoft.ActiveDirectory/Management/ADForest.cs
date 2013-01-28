using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADForest : ADEntity
	{
		public ADPropertyValueCollection ApplicationPartitions
		{
			get
			{
				return base["ApplicationPartitions"];
			}
		}

		public ADPropertyValueCollection CrossForestReferences
		{
			get
			{
				return base["CrossForestReferences"];
			}
		}

		public string DomainNamingMaster
		{
			get
			{
				return base.GetValue("DomainNamingMaster") as string;
			}
		}

		public ADPropertyValueCollection Domains
		{
			get
			{
				return base["Domains"];
			}
		}

		public ADForestMode? ForestMode
		{
			get
			{
				return new ADForestMode?((ADForestMode)((int)base.GetValue("ForestMode")));
			}
			set
			{
				throw new NotSupportedException(StringResources.UseSetADForestMode);
			}
		}

		public ADPropertyValueCollection GlobalCatalogs
		{
			get
			{
				return base["GlobalCatalogs"];
			}
		}

		internal override string IdentifyingString
		{
			get
			{
				return this.PartitionContainerName;
			}
		}

		public string Name
		{
			get
			{
				return base.GetValue("Name") as string;
			}
		}

		internal string PartitionContainerName
		{
			get
			{
				return base.GetValue("PartitionsContainer") as string;
			}
		}

		public string RootDomain
		{
			get
			{
				return base.GetValue("RootDomain") as string;
			}
		}

		public string SchemaMaster
		{
			get
			{
				return base.GetValue("SchemaMaster") as string;
			}
		}

		public ADPropertyValueCollection Sites
		{
			get
			{
				return base["Sites"];
			}
		}

		public ADPropertyValueCollection SPNSuffixes
		{
			get
			{
				return base["SPNSuffixes"];
			}
			set
			{
				base.SetValue("SPNSuffixes", value);
			}
		}

		public ADPropertyValueCollection UPNSuffixes
		{
			get
			{
				return base["UPNSuffixes"];
			}
			set
			{
				base.SetValue("UPNSuffixes", value);
			}
		}

		static ADForest()
		{
			ADEntity.RegisterMappingTable(typeof(ADForest), ADForestFactory<ADForest>.AttributeTable);
		}

		public ADForest()
		{
		}

		public ADForest(string identity)
		{
			base.Identity = identity;
		}

		public ADForest(Guid guid)
		{
			base.Identity = guid;
		}

		public ADForest(SecurityIdentifier sid)
		{
			base.Identity = sid;
		}

		public ADForest(ADObject adobject)
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

		public override string ToString()
		{
			if (!base.IsSearchResult)
			{
				if (this.Identity == null)
				{
					return base.ToString();
				}
				else
				{
					return this.Identity.ToString();
				}
			}
			else
			{
				return this.Name;
			}
		}
	}
}