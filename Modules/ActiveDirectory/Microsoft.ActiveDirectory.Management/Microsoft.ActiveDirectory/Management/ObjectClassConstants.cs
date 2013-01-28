using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ObjectClassConstants
	{
		public const string SecurityPrincipal = "securityPrincipal";

		public const string ForeignSecurityPrincipal = "foreignSecurityPrincipal";

		public const string Account = "account";

		public const string User = "user";

		public const string Computer = "computer";

		public const string Group = "group";

		public const string Domain = "domain";

		public const string OrganizationalUnit = "organizationalUnit";

		public const string Site = "site";

		public const string SiteLink = "siteLink";

		public const string SiteLinkBridge = "siteLinkBridge";

		public const string DynamicObject = "dynamicObject";

		public const string ClassSchema = "classSchema";

		public const string AttributeSchema = "attributeSchema";

		public const string CrossRef = "crossRef";

		public const string Server = "server";

		private ObjectClassConstants()
		{
		}
	}
}