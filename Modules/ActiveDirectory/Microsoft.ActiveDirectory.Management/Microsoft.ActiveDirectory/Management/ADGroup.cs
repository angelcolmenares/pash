using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADGroup : ADPrincipal
	{
		public ADGroupCategory? GroupCategory
		{
			get
			{
				object value = base.GetValue("GroupCategory");
				if (value != null)
				{
					return new ADGroupCategory?((ADGroupCategory)value);
				}
				else
				{
					ADGroupCategory? nullable = null;
					return nullable;
				}
			}
			set
			{
				base.SetValue("GroupCategory", value);
			}
		}

		public ADGroupScope? GroupScope
		{
			get
			{
				object value = base.GetValue("GroupScope");
				if (value != null)
				{
					return new ADGroupScope?((ADGroupScope)value);
				}
				else
				{
					ADGroupScope? nullable = null;
					return nullable;
				}
			}
			set
			{
				base.SetValue("GroupScope", value);
			}
		}

		static ADGroup()
		{
			ADEntity.RegisterMappingTable(typeof(ADGroup), ADGroupFactory<ADGroup>.AttributeTable);
		}

		public ADGroup()
		{
		}

		public ADGroup(string identity) : base(identity)
		{
		}

		public ADGroup(Guid guid) : base(guid)
		{
		}

		public ADGroup(SecurityIdentifier sid) : base(sid)
		{
		}

		public ADGroup(ADObject identity) : base(identity)
		{
		}
	}
}