using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class ACLConstants
	{
		internal const string SelfSddl = "S-1-5-10";

		internal const string WorldSddl = "S-1-1-0";

		internal readonly static Guid ChangePasswordGuid;

		internal static string DomainAdministratorsGroupSID;

		static ACLConstants()
		{
			ACLConstants.ChangePasswordGuid = new Guid("{ab721a53-1e2f-11d0-9819-00aa0040529b}");
			ACLConstants.DomainAdministratorsGroupSID = "S-1-5-32-544";
		}
	}
}