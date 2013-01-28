using System;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal class ADProviderConstants
	{
		public const int DsInstanceTypeIsNCBit = 1;

		public const string RootDSEPath = "";

		public const string GCPortPostfix = ":3268";

		public const string GCSslPortPostfix = ":3269";

		public const string AbsolutePathPrefixToken = "//RootDSE/";

		public const string MamlCommandPrefix = "command";

		public const string MamlCommandNamespace = "http://schemas.microsoft.com/maml/dev/command/2004/10";

		public const string ProviderHelpCommandXPath = "/helpItems/ProviderHelp/CmdletHelpPaths/CmdletHelpPath/command:command";

		public const string CommandDetailsVerbTag = "command:details/command:verb";

		public const string CommandDetailsNounTag = "command:details/command:noun";

		public ADProviderConstants()
		{
		}
	}
}