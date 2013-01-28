using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADPrincipal : ADObject
	{
		public string SamAccountName
		{
			get
			{
				return (string)base.GetValue("SamAccountName");
			}
			set
			{
				if (!base.IsSearchResult)
				{
					base.SetValue("SamAccountName", value);
					return;
				}
				else
				{
					throw new NotSupportedException();
				}
			}
		}

		public SecurityIdentifier SID
		{
			get
			{
				var obj = base.GetValue("SID");
				if (obj is SecurityIdentifier) return (SecurityIdentifier)obj;
				return new SecurityIdentifier((byte[])obj, 0);
			}
			set
			{
				if (!base.IsSearchResult)
				{
					base.SetValue("SID", value);
					return;
				}
				else
				{
					throw new NotSupportedException();
				}
			}
		}

		static ADPrincipal()
		{
			ADEntity.RegisterMappingTable(typeof(ADPrincipal), ADPrincipalFactory<ADPrincipal>.AttributeTable);
		}

		public ADPrincipal()
		{
		}

		public ADPrincipal(string identity) : base(identity)
		{
		}

		public ADPrincipal(Guid guid) : base(new Guid?(guid))
		{
		}

		public ADPrincipal(SecurityIdentifier sid)
		{
			base.Identity = sid;
		}

		public ADPrincipal(ADObject adobject)
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