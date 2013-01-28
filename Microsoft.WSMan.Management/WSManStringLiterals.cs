using System;

namespace Microsoft.WSMan.Management
{
	internal static class WSManStringLiterals
	{
		internal const char DefaultPathSeparator = '\\';

		internal const char AlternatePathSeparator = '/';

		internal const char EnclosingDoubleQuotes = '\"';

		internal const char Equalto = '=';

		internal const char GreaterThan = '>';

		internal const string XmlClosingTag = "/>";

		internal const char SingleWhiteSpace = ' ';

		internal const string ProviderName = "WSMan";

		internal const string WsMan_Schema = "http://schemas.microsoft.com/wbem/wsman/1/config";

		internal const string NS_XSI = "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";

		internal const string ATTR_NIL = "xsi:nil=\"true\"";

		internal const string ATTR_NIL_NAME = "xsi:nil";

		internal const char WinrmPathSeparator = '/';

		internal const string rootpath = "WSMan";

		internal const string ContainerChildValue = "Container";

		internal const string containerPlugin = "Plugin";

		internal const string containerClient = "Client";

		internal const string containerShell = "Shell";

		internal const string containerClientCertificate = "ClientCertificate";

		internal const string containerListener = "Listener";

		internal const string containerService = "Service";

		internal const string containerAuth = "Auth";

		internal const string containerDefaultPorts = "DefaultPorts";

		internal const string containerTrustedHosts = "TrustedHosts";

		internal const string containerSecurity = "Security";

		internal const string containerResources = "Resources";

		internal const string containerSingleResource = "Resource";

		internal const string containerInitParameters = "InitializationParameters";

		internal const string containerQuotasParameters = "Quotas";

		internal const string containerWinrs = "Winrs";

		internal const string containerCertMapping = "Service/certmapping";

		internal const string ConfigRunAsPasswordName = "RunAsPassword";

		internal const string ConfigRunAsUserName = "RunAsUser";

		internal const string ConfigUseSharedProcess = "UseSharedProcess";

		internal const string ConfigAutoRestart = "AutoRestart";

		internal const string ConfigProcessIdleTimeoutSec = "ProcessIdleTimeoutSec";

		internal const string ConfigResourceUriName = "ResourceUri";

		internal const string ConfigInitializeParameterTag = "Param";

		internal const string ConfigInitializeParameterName = "Name";

		internal const string ConfigInitializeParameterValue = "Value";

		internal const string ConfigSecurityUri = "Uri";

		internal const string HiddenSuffixForSourceOfValue = "___Source";

		public const string StartWinrmServiceSBFormat = "\r\nfunction Start-WSManServiceD15A7957836142a18627D7E1D342DD82\r\n{{\r\n[CmdletBinding()]\r\nparam(\r\n    [Parameter()]\r\n    [bool]\r\n    $Force,\r\n   \r\n    [Parameter()]\r\n    [string]\r\n    $captionForStart,\r\n        \r\n    [Parameter()]\r\n    [string]\r\n    $queryForStart)\r\n\r\n    begin\r\n    {{    \r\n        if ($force -or $pscmdlet.ShouldContinue($queryForStart, $captionForStart))\r\n        {{\r\n            Restart-Service WinRM -Force\r\n            return $true\r\n        }}\r\n        return $false    \r\n    }} #end of Begin block\r\n}}\r\n$_ | Start-WSManServiceD15A7957836142a18627D7E1D342DD82 -force $args[0] -captionForStart $args[1] -queryForStart $args[2]\r\n";

		internal readonly static string[] NewItemPluginConfigParams;

		internal readonly static string[] NewItemResourceParams;

		internal readonly static string[] NewItemInitParamsParams;

		internal readonly static string[] NewItemSecurityParams;

		static WSManStringLiterals()
		{
			string[] strArrays = new string[11];
			strArrays[0] = "Name";
			strArrays[1] = "Filename";
			strArrays[2] = "SDKVersion";
			strArrays[3] = "XmlRenderingType";
			strArrays[4] = "Enabled";
			strArrays[5] = "Architecture";
			strArrays[6] = "RunAsPassword";
			strArrays[7] = "RunAsUser";
			strArrays[8] = "AutoRestart";
			strArrays[9] = "ProcessIdleTimeoutSec";
			strArrays[10] = "UseSharedProcess";
			WSManStringLiterals.NewItemPluginConfigParams = strArrays;
			string[] strArrays1 = new string[5];
			strArrays1[0] = "Resource";
			strArrays1[1] = "ResourceUri";
			strArrays1[2] = "Capability";
			strArrays1[3] = "ExactMatch";
			strArrays1[4] = "SupportsOptions";
			WSManStringLiterals.NewItemResourceParams = strArrays1;
			string[] strArrays2 = new string[2];
			strArrays2[0] = "Name";
			strArrays2[1] = "Value";
			WSManStringLiterals.NewItemInitParamsParams = strArrays2;
			string[] strArrays3 = new string[3];
			strArrays3[0] = "Uri";
			strArrays3[1] = "Sddl";
			strArrays3[2] = "ExactMatch";
			WSManStringLiterals.NewItemSecurityParams = strArrays3;
		}
	}
}