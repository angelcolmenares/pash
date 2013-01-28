using System;
using System.Collections.Generic;

namespace Microsoft.Management.PowerShellWebAccess
{
	public interface IActiveDirectoryHelper
	{
		bool CheckComputerTypeMatch(bool isLocal, string sid, PswaDestinationType type, string domain, out string errorMessage);

		bool CheckUserTypeMatch(bool isLocal, string sid, PswaUserType type, string domain, out string errorMessage);

		string ConvertAccountNameToStringSid(string accountName, out bool isAccountLocal, out string domain);

		string ConvertComputerName(string computerName, bool enforceFqdn);

		string ConvertStringSidToAccountName(string sid, out string domain);

		List<string> GetAccountDomainGroupSid(string accountSid);

		string GetFqdn(string destination);

		bool IsAccountInGroup(string groupSid, List<string> accountDomainGroup, string accountSid, Dictionary<string, string> checkedSid);

		bool IsCurrentComputerDomainJoined();
	}
}