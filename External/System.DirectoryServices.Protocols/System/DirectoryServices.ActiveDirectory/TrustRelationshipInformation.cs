using System;
using System.Runtime;

namespace System.DirectoryServices.ActiveDirectory
{
	public class TrustRelationshipInformation
	{
		internal string source;

		internal string target;

		internal TrustType type;

		internal TrustDirection direction;

		internal DirectoryContext context;

		public string SourceName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.source;
			}
		}

		public string TargetName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.target;
			}
		}

		public TrustDirection TrustDirection
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.direction;
			}
		}

		public TrustType TrustType
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.type;
			}
		}

		internal TrustRelationshipInformation()
		{
		}

		internal TrustRelationshipInformation(DirectoryContext context, string source, TrustObject obj)
		{
			string netbiosDomainName;
			this.context = context;
			this.source = source;
			TrustRelationshipInformation trustRelationshipInformation = this;
			if (obj.DnsDomainName == null)
			{
				netbiosDomainName = obj.NetbiosDomainName;
			}
			else
			{
				netbiosDomainName = obj.DnsDomainName;
			}
			trustRelationshipInformation.target = netbiosDomainName;
			if ((obj.Flags & 2) == 0 || (obj.Flags & 32) == 0)
			{
				if ((obj.Flags & 2) == 0)
				{
					if ((obj.Flags & 32) != 0)
					{
						this.direction = TrustDirection.Inbound;
					}
				}
				else
				{
					this.direction = TrustDirection.Outbound;
				}
			}
			else
			{
				this.direction = TrustDirection.Bidirectional;
			}
			this.type = obj.TrustType;
		}
	}
}