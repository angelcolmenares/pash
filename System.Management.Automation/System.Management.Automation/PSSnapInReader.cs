namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using Microsoft.PowerShell.Commands;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    internal static class PSSnapInReader
    {
        private static readonly PSSnapInInfo[] _junk = new PSSnapInInfo[] { new PSSnapInInfo("1", false, "3", "4", "5", new Version(), null, null, null, null, null, null) };
        private static PSTraceSource _mshsnapinTracer = PSTraceSource.GetTracer("MshSnapinLoadUnload", "Loading and unloading mshsnapins", false);
        private static DefaultPSSnapInInformation CoreSnapin = new DefaultPSSnapInInformation("Microsoft.PowerShell.Core", "System.Management.Automation", null, "CoreMshSnapInResources,Description", "CoreMshSnapInResources,Vendor");
        private static IList<DefaultPSSnapInInformation> defaultMshSnapins = null;

        internal static string ConvertByteArrayToString(byte[] tokens)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte num in tokens)
            {
                builder.AppendFormat("{0:x2}", num);
            }
            return builder.ToString();
        }

        internal static RegistryKey GetMonadRootKey()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Xamarin\PowerShell");
            if (key == null)
            {
                throw PSTraceSource.NewArgumentException("monad", "MshSnapinInfo", "MonadRootRegistryAccessFailed", new object[0]);
            }
            return key;
        }

        internal static RegistryKey GetMshSnapinKey(string mshSnapInName, string psVersion)
        {
            RegistryKey key3 = GetVersionRootKey(GetMonadRootKey(), psVersion).OpenSubKey("PowerShellSnapIns");
            if (key3 == null)
            {
                throw PSTraceSource.NewArgumentException("psVersion", "MshSnapinInfo", "NoMshSnapinPresentForVersion", new object[] { psVersion });
            }
            return key3.OpenSubKey(mshSnapInName);
        }

        private static RegistryKey GetMshSnapinRootKey(RegistryKey versionRootKey, string psVersion)
        {
            RegistryKey key = versionRootKey.OpenSubKey("PowerShellSnapIns");
            if (key == null)
            {
                throw PSTraceSource.NewArgumentException("psVersion", "MshSnapinInfo", "NoMshSnapinPresentForVersion", new object[] { psVersion });
            }
            return key;
        }

        internal static RegistryKey GetPSEngineKey(string psVersion)
        {
            RegistryKey monadRootKey = GetMonadRootKey();
            GetVersionRootKey(monadRootKey, psVersion);
            RegistryKey key2 = monadRootKey.OpenSubKey(psVersion);
            if (key2 == null)
            {
                throw PSTraceSource.NewArgumentException("monad", "MshSnapinInfo", "MonadEngineRegistryAccessFailed", new object[0]);
            }
            RegistryKey key3 = key2.OpenSubKey("PowerShellEngine");
            if (key3 == null)
            {
                throw PSTraceSource.NewArgumentException("monad", "MshSnapinInfo", "MonadEngineRegistryAccessFailed", new object[0]);
            }
            return key3;
        }

        internal static RegistryKey GetVersionRootKey(RegistryKey rootKey, string psVersion)
        {
            string registerVersionKeyForSnapinDiscovery = PSVersionInfo.GetRegisterVersionKeyForSnapinDiscovery(psVersion);
            RegistryKey key = rootKey.OpenSubKey(registerVersionKeyForSnapinDiscovery);
            if (key == null)
            {
                throw PSTraceSource.NewArgumentException("psVersion", "MshSnapinInfo", "SpecifiedVersionNotFound", new object[] { registerVersionKeyForSnapinDiscovery });
            }
            return key;
        }

        private static bool MeetsVersionFormat(string version)
        {
            bool flag = true;
            try
            {
                LanguagePrimitives.ConvertTo(version, typeof(int), CultureInfo.InvariantCulture);
            }
            catch (PSInvalidCastException)
            {
                flag = false;
            }
            return flag;
        }

        internal static PSSnapInInfo Read (string psVersion, string mshsnapinId)
		{
			if (string.IsNullOrEmpty (psVersion)) {
				throw PSTraceSource.NewArgumentNullException ("psVersion");
			}
			if (string.IsNullOrEmpty (mshsnapinId)) {
				throw PSTraceSource.NewArgumentNullException ("mshsnapinId");
			}
			PSSnapInInfo.VerifyPSSnapInFormatThrowIfError (mshsnapinId);
			if (OSHelper.IsUnix) {
				PSSnapInInfo info = new PSSnapInInfo(mshsnapinId, false, PowerShellConfiguration.PowerShellEngine.ApplicationBase, mshsnapinId, mshsnapinId, PSVersionInfo.PSVersion, PSVersionInfo.PSVersion, null, null, "", "Microsoft", null); 
				return null;
			}
            return ReadOne(GetMshSnapinRootKey(GetVersionRootKey(GetMonadRootKey(), psVersion), psVersion), mshsnapinId);
        }

        internal static Collection<PSSnapInInfo> ReadAll()
        {
            Collection<PSSnapInInfo> collection = new Collection<PSSnapInInfo>();
            RegistryKey monadRootKey = GetMonadRootKey();
            string[] subKeyNames = monadRootKey.GetSubKeyNames();
            if (subKeyNames != null)
            {
                Collection<string> collection2 = new Collection<string>();
                foreach (string str in subKeyNames)
                {
                    string registerVersionKeyForSnapinDiscovery = PSVersionInfo.GetRegisterVersionKeyForSnapinDiscovery(str);
                    if (string.IsNullOrEmpty(registerVersionKeyForSnapinDiscovery))
                    {
                        registerVersionKeyForSnapinDiscovery = str;
                    }
                    if (!collection2.Contains(registerVersionKeyForSnapinDiscovery))
                    {
                        collection2.Add(registerVersionKeyForSnapinDiscovery);
                    }
                }
                foreach (string str3 in collection2)
                {
                    if (!string.IsNullOrEmpty(str3) && MeetsVersionFormat(str3))
                    {
                        Collection<PSSnapInInfo> collection3 = null;
                        try
                        {
                            collection3 = ReadAll(monadRootKey, str3);
                        }
                        catch (SecurityException)
                        {
                        }
                        catch (ArgumentException)
                        {
                        }
                        if (collection3 != null)
                        {
                            foreach (PSSnapInInfo info in collection3)
                            {
                                collection.Add(info);
                            }
                        }
                    }
                }
            }
            return collection;
        }

        internal static Collection<PSSnapInInfo> ReadAll(string psVersion)
        {
            if (string.IsNullOrEmpty(psVersion))
            {
                throw PSTraceSource.NewArgumentNullException("psVersion");
            }
            return ReadAll(GetMonadRootKey(), psVersion);
        }

        private static Collection<PSSnapInInfo> ReadAll(RegistryKey monadRootKey, string psVersion)
        {
            Collection<PSSnapInInfo> collection = new Collection<PSSnapInInfo>();
            RegistryKey mshSnapinRootKey = GetMshSnapinRootKey(GetVersionRootKey(monadRootKey, psVersion), psVersion);
            foreach (string str in mshSnapinRootKey.GetSubKeyNames())
            {
                if (!string.IsNullOrEmpty(str))
                {
                    try
                    {
                        collection.Add(ReadOne(mshSnapinRootKey, str));
                    }
                    catch (SecurityException)
                    {
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            return collection;
        }

        internal static PSSnapInInfo ReadCoreEngineSnapIn()
        {
            Version version;
            Version version2;
            string publicKeyToken = null;
            string culture = null;
            string architecture = null;
            string applicationBase = null;
            ReadRegistryInfo(out version, out publicKeyToken, out culture, out architecture, out applicationBase, out version2);
            Collection<string> formats = new Collection<string>(new string[] { "Certificate.format.ps1xml", "DotNetTypes.format.ps1xml", "FileSystem.format.ps1xml", "Help.format.ps1xml", "HelpV3.format.ps1xml", "PowerShellCore.format.ps1xml", "PowerShellTrace.format.ps1xml", "Registry.format.ps1xml" });
            Collection<string> types = new Collection<string>(new string[] { "types.ps1xml", "typesv3.ps1xml" });
            string assemblyName = string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}, Culture={2}, PublicKeyToken={3}, ProcessorArchitecture={4}", new object[] { CoreSnapin.AssemblyName, version, culture, publicKeyToken, architecture });
            string moduleName = Path.Combine(applicationBase, CoreSnapin.AssemblyName + ".dll");
            PSSnapInInfo psSnapInInfo = new PSSnapInInfo(CoreSnapin.PSSnapInName, true, applicationBase, assemblyName, moduleName, version2, version, types, formats, null, CoreSnapin.Description, CoreSnapin.DescriptionIndirect, null, null, CoreSnapin.VendorIndirect, null);
            SetSnapInLoggingInformation(psSnapInInfo);
            return psSnapInInfo;
        }

        internal static Collection<PSSnapInInfo> ReadEnginePSSnapIns()
        {
            Version version;
            Version version2;
            string publicKeyToken = null;
            string culture = null;
            string architecture = null;
            string applicationBase = null;
            ReadRegistryInfo(out version, out publicKeyToken, out culture, out architecture, out applicationBase, out version2);
            Collection<string> collection = new Collection<string>(new string[] { "Certificate.format.ps1xml", "DotNetTypes.format.ps1xml", "FileSystem.format.ps1xml", "Help.format.ps1xml", "HelpV3.format.ps1xml", "PowerShellCore.format.ps1xml", "PowerShellTrace.format.ps1xml", "Registry.format.ps1xml" });
            Collection<string> collection2 = new Collection<string>(new string[] { "types.ps1xml", "typesv3.ps1xml" });
            Collection<PSSnapInInfo> collection3 = new Collection<PSSnapInInfo>();
            for (int i = 0; i < DefaultMshSnapins.Count; i++)
            {
                DefaultPSSnapInInformation information = DefaultMshSnapins[i];
                string assemblyName = string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}, Culture={2}, PublicKeyToken={3}, ProcessorArchitecture={4}", new object[] { information.AssemblyName, version.ToString(), culture, publicKeyToken, architecture });
                Collection<string> formats = null;
                Collection<string> types = null;
                if (information.AssemblyName.Equals("System.Management.Automation", StringComparison.OrdinalIgnoreCase))
                {
                    formats = collection;
                    types = collection2;
                }
                else if (information.AssemblyName.Equals("Microsoft.PowerShell.Commands.Diagnostics", StringComparison.OrdinalIgnoreCase))
                {
                    types = new Collection<string>(new string[] { "GetEvent.types.ps1xml" });
                    formats = new Collection<string>(new string[] { "Event.Format.ps1xml", "Diagnostics.Format.ps1xml" });
                }
                else if (information.AssemblyName.Equals("Microsoft.WSMan.Management", StringComparison.OrdinalIgnoreCase))
                {
                    formats = new Collection<string>(new string[] { "WSMan.format.ps1xml" });
                }
                string moduleName = Path.Combine(applicationBase, information.AssemblyName + ".dll");
                PSSnapInInfo psSnapInInfo = new PSSnapInInfo(information.PSSnapInName, true, applicationBase, assemblyName, moduleName, version2, version, types, formats, null, information.Description, information.DescriptionIndirect, null, null, information.VendorIndirect, null);
                SetSnapInLoggingInformation(psSnapInInfo);
                collection3.Add(psSnapInInfo);
            }
            return collection3;
        }

        private static Collection<string> ReadMultiStringValue(RegistryKey mshsnapinKey, string name, bool mandatory)
        {
            object obj2 = mshsnapinKey.GetValue(name);
            if (obj2 == null)
            {
                if (mandatory)
                {
                    _mshsnapinTracer.TraceError("Mandatory property {0} not specified for registry key {1}", new object[] { name, mshsnapinKey.Name });
                    throw PSTraceSource.NewArgumentException("name", "MshSnapinInfo", "MandatoryValueNotPresent", new object[] { name, mshsnapinKey.Name });
                }
                return null;
            }
            string[] list = obj2 as string[];
            if (list == null)
            {
                string str = obj2 as string;
                if (str != null)
                {
                    list = new string[] { str };
                }
            }
            if (list == null)
            {
                if (mandatory)
                {
                    _mshsnapinTracer.TraceError("Cannot get string/multi-string value for mandatory property {0} in registry key {1}", new object[] { name, mshsnapinKey.Name });
                    throw PSTraceSource.NewArgumentException("name", "MshSnapinInfo", "MandatoryValueNotInCorrectFormatMultiString", new object[] { name, mshsnapinKey.Name });
                }
                return null;
            }
            _mshsnapinTracer.WriteLine("Successfully read property {0} from {1}", new object[] { name, mshsnapinKey.Name });
            return new Collection<string>(list);
        }

        private static PSSnapInInfo ReadOne(RegistryKey mshSnapInRoot, string mshsnapinId)
        {
            RegistryKey mshsnapinKey = mshSnapInRoot.OpenSubKey(mshsnapinId);
            if (mshsnapinKey == null)
            {
                _mshsnapinTracer.TraceError(@"Error opening registry key {0}\{1}.", new object[] { mshSnapInRoot.Name, mshsnapinId });
                throw PSTraceSource.NewArgumentException("mshsnapinId", "MshSnapinInfo", "MshSnapinDoesNotExist", new object[] { mshsnapinId });
            }
            string applicationBase = ReadStringValue(mshsnapinKey, "ApplicationBase", true);
            string assemblyName = ReadStringValue(mshsnapinKey, "AssemblyName", true);
            string moduleName = ReadStringValue(mshsnapinKey, "ModuleName", true);
            Version psVersion = ReadVersionValue(mshsnapinKey, "PowerShellVersion", true);
            Version version = ReadVersionValue(mshsnapinKey, "Version", false);
            string descriptionFallback = ReadStringValue(mshsnapinKey, "Description", false);
            if (descriptionFallback == null)
            {
                _mshsnapinTracer.WriteLine("No description is specified for mshsnapin {0}. Using empty string for description.", new object[] { mshsnapinId });
                descriptionFallback = string.Empty;
            }
            string vendorFallback = ReadStringValue(mshsnapinKey, "Vendor", false);
            if (vendorFallback == null)
            {
                _mshsnapinTracer.WriteLine("No vendor is specified for mshsnapin {0}. Using empty string for description.", new object[] { mshsnapinId });
                vendorFallback = string.Empty;
            }
            bool flag = false;
            string str6 = ReadStringValue(mshsnapinKey, "LogPipelineExecutionDetails", false);
            if (!string.IsNullOrEmpty(str6) && (string.Compare("1", str6, StringComparison.OrdinalIgnoreCase) == 0))
            {
                flag = true;
            }
            string str7 = ReadStringValue(mshsnapinKey, "CustomPSSnapInType", false);
            if (string.IsNullOrEmpty(str7))
            {
                str7 = null;
            }
            Collection<string> types = ReadMultiStringValue(mshsnapinKey, "Types", false);
            Collection<string> formats = ReadMultiStringValue(mshsnapinKey, "Formats", false);
            _mshsnapinTracer.WriteLine("Successfully read registry values for mshsnapin {0}. Constructing PSSnapInInfo object.", new object[] { mshsnapinId });
            return new PSSnapInInfo(mshsnapinId, false, applicationBase, assemblyName, moduleName, psVersion, version, types, formats, descriptionFallback, vendorFallback, str7) { LogPipelineExecutionDetails = flag };
        }

        private static void ReadRegistryInfo(out Version assemblyVersion, out string publicKeyToken, out string culture, out string architecture, out string applicationBase, out Version psVersion)
        {
			applicationBase = PowerShellConfiguration.PowerShellEngine.ApplicationBase;
            //RegistryKey pSEngineKey = GetPSEngineKey(PSVersionInfo.RegistryVersionKey);
            //applicationBase = ReadStringValue(pSEngineKey, "ApplicationBase", true);
            psVersion = PSVersionInfo.PSVersion;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            assemblyVersion = executingAssembly.GetName().Version;
            byte[] tokens = executingAssembly.GetName().GetPublicKeyToken();
            if (tokens.Length == 0)
            {
                throw PSTraceSource.NewArgumentException("PublicKeyToken", "MshSnapinInfo", "PublicKeyTokenAccessFailed", new object[0]);
            }
            publicKeyToken = ConvertByteArrayToString(tokens);
            culture = "neutral";
            architecture = "MSIL";
        }

        internal static string ReadStringValue(RegistryKey mshsnapinKey, string name, bool mandatory)
        {
            object obj2 = mshsnapinKey.GetValue(name);
            if ((obj2 == null) && mandatory)
            {
                _mshsnapinTracer.TraceError("Mandatory property {0} not specified for registry key {1}", new object[] { name, mshsnapinKey.Name });
                throw PSTraceSource.NewArgumentException("name", "MshSnapinInfo", "MandatoryValueNotPresent", new object[] { name, mshsnapinKey.Name });
            }
            string str = obj2 as string;
            if (string.IsNullOrEmpty(str) && mandatory)
            {
                _mshsnapinTracer.TraceError("Value is null or empty for mandatory property {0} in {1}", new object[] { name, mshsnapinKey.Name });
                throw PSTraceSource.NewArgumentException("name", "MshSnapinInfo", "MandatoryValueNotInCorrectFormat", new object[] { name, mshsnapinKey.Name });
            }
            _mshsnapinTracer.WriteLine("Successfully read value {0} for property {1} from {2}", new object[] { str, name, mshsnapinKey.Name });
            return str;
        }

        internal static Version ReadVersionValue(RegistryKey mshsnapinKey, string name, bool mandatory)
        {
            Version version;
            string str = ReadStringValue(mshsnapinKey, name, mandatory);
            if (str == null)
            {
                _mshsnapinTracer.TraceError("Cannot read value for property {0} in registry key {1}", new object[] { name, mshsnapinKey.ToString() });
                return null;
            }
            try
            {
                version = new Version(str);
            }
            catch (ArgumentOutOfRangeException)
            {
                _mshsnapinTracer.TraceError("Cannot convert value {0} to version format", new object[] { str });
                throw PSTraceSource.NewArgumentException("name", "MshSnapinInfo", "VersionValueInCorrect", new object[] { name, mshsnapinKey.Name });
            }
            catch (ArgumentException)
            {
                _mshsnapinTracer.TraceError("Cannot convert value {0} to version format", new object[] { str });
                throw PSTraceSource.NewArgumentException("name", "MshSnapinInfo", "VersionValueInCorrect", new object[] { name, mshsnapinKey.Name });
            }
            catch (OverflowException)
            {
                _mshsnapinTracer.TraceError("Cannot convert value {0} to version format", new object[] { str });
                throw PSTraceSource.NewArgumentException("name", "MshSnapinInfo", "VersionValueInCorrect", new object[] { name, mshsnapinKey.Name });
            }
            catch (FormatException)
            {
                _mshsnapinTracer.TraceError("Cannot convert value {0} to version format", new object[] { str });
                throw PSTraceSource.NewArgumentException("name", "MshSnapinInfo", "VersionValueInCorrect", new object[] { name, mshsnapinKey.Name });
            }
            _mshsnapinTracer.WriteLine("Successfully converted string {0} to version format.", new object[] { version.ToString() });
            return version;
        }

        private static void SetSnapInLoggingInformation(PSSnapInInfo psSnapInInfo)
        {
            foreach (ExecutionPolicyScope scope in SecuritySupport.ExecutionPolicyScopePreferences)
            {
                IEnumerable<string> enumerable;
                ModuleCmdletBase.ModuleLoggingGroupPolicyStatus moduleLoggingInformation = ModuleCmdletBase.GetModuleLoggingInformation(scope, out enumerable);
                if (moduleLoggingInformation != ModuleCmdletBase.ModuleLoggingGroupPolicyStatus.Undefined)
                {
                    SetSnapInLoggingInformation(psSnapInInfo, moduleLoggingInformation, enumerable);
                    return;
                }
            }
        }

        private static void SetSnapInLoggingInformation(PSSnapInInfo psSnapInInfo, ModuleCmdletBase.ModuleLoggingGroupPolicyStatus status, IEnumerable<string> moduleOrSnapinNames)
        {
            if (((status & ModuleCmdletBase.ModuleLoggingGroupPolicyStatus.Enabled) != ModuleCmdletBase.ModuleLoggingGroupPolicyStatus.Undefined) && (moduleOrSnapinNames != null))
            {
                foreach (string str in moduleOrSnapinNames)
                {
                    if (string.Equals(psSnapInInfo.Name, str, StringComparison.OrdinalIgnoreCase))
                    {
                        psSnapInInfo.LogPipelineExecutionDetails = true;
                    }
                    else if (WildcardPattern.ContainsWildcardCharacters(str))
                    {
                        WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                        if (pattern.IsMatch(psSnapInInfo.Name))
                        {
                            psSnapInInfo.LogPipelineExecutionDetails = true;
                        }
                    }
                }
            }
        }

        private static IList<DefaultPSSnapInInformation> DefaultMshSnapins
        {
            get
            {
                if (defaultMshSnapins == null)
                {
                    defaultMshSnapins = new List<DefaultPSSnapInInformation> { new DefaultPSSnapInInformation("Microsoft.PowerShell.Diagnostics", "Microsoft.PowerShell.Commands.Diagnostics", null, "GetEventResources,Description", "GetEventResources,Vendor"), CoreSnapin, new DefaultPSSnapInInformation("Microsoft.PowerShell.Utility", "Microsoft.PowerShell.Commands.Utility", null, "UtilityMshSnapInResources,Description", "UtilityMshSnapInResources,Vendor"), new DefaultPSSnapInInformation("Microsoft.PowerShell.Host", "Microsoft.PowerShell.ConsoleHost", null, "HostMshSnapInResources,Description", "HostMshSnapInResources,Vendor"), new DefaultPSSnapInInformation("Microsoft.PowerShell.Management", "Microsoft.PowerShell.Commands.Management", null, "ManagementMshSnapInResources,Description", "ManagementMshSnapInResources,Vendor"), new DefaultPSSnapInInformation("Microsoft.PowerShell.Security", "Microsoft.PowerShell.Security", null, "SecurityMshSnapInResources,Description", "SecurityMshSnapInResources,Vendor") };
                    if (!RemotingCommandUtil.IsWinPEHost())
                    {
                        defaultMshSnapins.Add(new DefaultPSSnapInInformation("Microsoft.WSMan.Management", "Microsoft.WSMan.Management", null, "WsManResources,Description", "WsManResources,Vendor"));
                    }
                }
                return defaultMshSnapins;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DefaultPSSnapInInformation
        {
            public string PSSnapInName;
            public string AssemblyName;
            public string Description;
            public string DescriptionIndirect;
            public string VendorIndirect;
            public DefaultPSSnapInInformation(string sName, string sAssemblyName, string sDescription, string sDescriptionIndirect, string sVendorIndirect)
            {
                this.PSSnapInName = sName;
                this.AssemblyName = sAssemblyName;
                this.Description = sDescription;
                this.DescriptionIndirect = sDescriptionIndirect;
                this.VendorIndirect = sVendorIndirect;
            }
        }
    }
}

