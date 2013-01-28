using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADDirectoryServer : ADEntity
	{
		public string IPv4Address
		{
			get
			{
				return (string)base.GetValue("IPv4Address");
			}
			set
			{
				base.SetValue("IPv4Address", value);
			}
		}

		public string IPv6Address
		{
			get
			{
				return (string)base.GetValue("IPv6Address");
			}
			set
			{
				base.SetValue("IPv6Address", value);
			}
		}

		public string Name
		{
			get
			{
				return (string)base.GetValue("Name");
			}
			set
			{
				base.SetValue("Name", value);
			}
		}

		public string Site
		{
			get
			{
				return (string)base.GetValue("Site");
			}
			set
			{
				base.SetValue("Site", value);
			}
		}

		static ADDirectoryServer()
		{
			ADEntity.RegisterMappingTable(typeof(ADDirectoryServer), ADDirectoryServerFactory<ADDirectoryServer>.AttributeTable);
		}

		public ADDirectoryServer()
		{
		}

		public ADDirectoryServer(string identity)
		{
			base.Identity = identity;
		}

		public ADDirectoryServer(Guid guid)
		{
			base.Identity = guid;
		}

		public ADDirectoryServer(ADObject identity)
		{
			if (identity != null)
			{
				base.Identity = identity;
				if (identity.IsSearchResult)
				{
					base.SessionInfo = identity.SessionInfo;
				}
				return;
			}
			else
			{
				throw new ArgumentException("identity");
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