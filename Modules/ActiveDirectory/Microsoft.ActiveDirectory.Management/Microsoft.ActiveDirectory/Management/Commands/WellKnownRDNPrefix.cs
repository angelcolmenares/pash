using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class WellKnownRDNPrefix
	{
		internal const string NTDSSettingsRDNPrefix = "CN=NTDS Settings,";

		internal const string ServerRDNPrefix = "CN=Servers,";

		internal const string SitesRDNPrefix = "CN=Sites,";

		internal const string fgppContainerRDNPrefix = "CN=Password Settings Container,";

		internal const string NTDSSiteSettingsRDNPrefix = "CN=NTDS Site Settings,";

		internal const string SubnetsRDNPrefix = "CN=Subnets,CN=Sites,";

		internal const string SiteLinkRDNPrefix = "CN=Inter-Site Transports,CN=Sites,";

		internal const string SiteLinkIPRDNPrefix = "CN=IP,";

		internal const string SiteLinkSMTPRDNPrefix = "CN=SMTP,";

		internal const string SiteLinkIPPrefix = "CN=IP";

		internal const string SiteLinkSMTPPrefix = "CN=SMTP";

		internal const string ClaimTypesBaseRDNPrefix = "CN=Claims Configuration,CN=Services,";

		internal const string ResourcePropertyRDNPrefix = "CN=Resource Properties,";

		internal const string ClaimTypesRDNPrefix = "CN=Claim Types,";

		internal const string CentralAccessPolicyRDNPrefix = "CN=Central Access Policies,CN=Claims Configuration,CN=Services,";

		internal const string CentralAccessRuleRDNPrefix = "CN=Central Access Rules,CN=Claims Configuration,CN=Services,";

		internal const string ResourcePropertyListRDNPrefix = "CN=Resource Property Lists,CN=Claims Configuration,CN=Services,";

		internal const string ResourcePropertyValueTypeRDNPrefix = "CN=Value Types,CN=Claims Configuration,CN=Services,";

		internal const string ClaimTransformPolicyRDNPrefix = "CN=Claims Transformation Policies,CN=Claims Configuration,CN=Services,";

	}
}