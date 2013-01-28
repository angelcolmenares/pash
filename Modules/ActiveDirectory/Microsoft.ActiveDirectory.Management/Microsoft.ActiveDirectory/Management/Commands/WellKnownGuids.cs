using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class WellKnownGuids
	{
		internal readonly static string UsersContainerGuid;

		internal readonly static string ComputersContainerGuid;

		internal readonly static string SystemsContainerGuid;

		internal readonly static string DCContainerGuid;

		internal readonly static string InfrastructureContainerGuid;

		internal readonly static string DeletedObjectsContainerGuid;

		internal readonly static string LostAndFoundContainerGuid;

		internal readonly static string ForeignSecurityPrincipalContainerGuid;

		internal readonly static string ProgramDataContainerGuid;

		internal readonly static string MicrosoftProgramDataContainerGuid;

		internal readonly static string NtdsQuotasContainerGuid;

		internal readonly static string MSAContainerGuid;

		static WellKnownGuids()
		{
			WellKnownGuids.UsersContainerGuid = "a9d1ca15768811d1aded00c04fd8d5cd";
			WellKnownGuids.ComputersContainerGuid = "aa312825768811d1aded00c04fd8d5cd";
			WellKnownGuids.SystemsContainerGuid = "ab1d30f3768811d1aded00c04fd8d5cd";
			WellKnownGuids.DCContainerGuid = "a361b2ffffd211d1aa4b00c04fd7d83a";
			WellKnownGuids.InfrastructureContainerGuid = "2fbac1870ade11d297c400c04fd8d5cd";
			WellKnownGuids.DeletedObjectsContainerGuid = "18e2ea80684f11d2b9aa00c04f79f805";
			WellKnownGuids.LostAndFoundContainerGuid = "ab8153b7768811d1aded00c04fd8d5cd";
			WellKnownGuids.ForeignSecurityPrincipalContainerGuid = "22b70c67d56e4efb91e9300fca3dc1aa";
			WellKnownGuids.ProgramDataContainerGuid = "09460c08ae1e4a4ea0f64aee7daa1e5a";
			WellKnownGuids.MicrosoftProgramDataContainerGuid = "f4be92a4c777485e878e9421d53087db";
			WellKnownGuids.NtdsQuotasContainerGuid = "6227f0af1fc2410d8e3bb10615bb5b0f";
			WellKnownGuids.MSAContainerGuid = "1eb93889e40c45df9f0c64d23bbb6237";
		}
	}
}