using Microsoft.ActiveDirectory.Management.Commands;
using System;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADOrganizationalUnit : ADObject
	{
		public string City
		{
			get
			{
				return (string)base.GetValue("City");
			}
			set
			{
				base.SetValue("City", value);
			}
		}

		public string Country
		{
			get
			{
				return (string)base.GetValue("Country");
			}
			set
			{
				base.SetValue("Country", value);
			}
		}

		public string ManagedBy
		{
			get
			{
				return (string)base.GetValue("ManagedBy");
			}
			set
			{
				base.SetValue("ManagedBy", value);
			}
		}

		public string PostalCode
		{
			get
			{
				return (string)base.GetValue("PostalCode");
			}
			set
			{
				base.SetValue("PostalCode", value);
			}
		}

		public string State
		{
			get
			{
				return (string)base.GetValue("State");
			}
			set
			{
				base.SetValue("State", value);
			}
		}

		public string StreetAddress
		{
			get
			{
				return (string)base.GetValue("StreetAddress");
			}
			set
			{
				base.SetValue("StreetAddress", value);
			}
		}

		static ADOrganizationalUnit()
		{
			ADEntity.RegisterMappingTable(typeof(ADOrganizationalUnit), ADOrganizationalUnitFactory<ADOrganizationalUnit>.AttributeTable);
		}

		public ADOrganizationalUnit()
		{
		}

		public ADOrganizationalUnit(ADObject identity)
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

		public ADOrganizationalUnit(string identity) : base(identity)
		{
		}

		public ADOrganizationalUnit(Guid guid) : base(new Guid?(guid))
		{
		}
	}
}