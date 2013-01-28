using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DSROLE_PRIMARY_DOMAIN_INFO_BASIC
	{
		public DSROLE_MACHINE_ROLE MachineRole;

		public uint Flags;

		public string DomainNameFlat;

		public string DomainNameDns;

		public string DomainForestName;

		public Guid DomainGuid;

		public DSROLE_PRIMARY_DOMAIN_INFO_BASIC()
		{
			this.DomainGuid = new Guid();
		}
	}
}