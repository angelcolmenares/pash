namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using Microsoft.Win32;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Security;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;

    internal static class SecuritySupport
    {
        internal static bool CertHasPrivatekey(X509Certificate2 cert)
        {
            return cert.HasPrivateKey;
        }

        internal static bool CertIsGoodForSigning(X509Certificate2 c)
        {
            if (CertHasPrivatekey(c))
            {
                foreach (string str in GetCertEKU(c))
                {
                    if (str == "1.3.6.1.5.5.7.3.3")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static void CheckIfFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }
        }

        private static void CleanKeyParents(RegistryKey baseKey, string keyPath)
        {
            using (RegistryKey key = baseKey.OpenSubKey(keyPath, true))
            {
                if ((key == null) || ((key.ValueCount == 0) && (key.SubKeyCount == 0)))
                {
                    string[] strArray = keyPath.Split(new char[] { '\\' });
                    if (strArray.Length > 2)
                    {
                        string subkey = strArray[strArray.Length - 1];
                        string name = keyPath.Remove((keyPath.Length - subkey.Length) - 1);
                        if (key != null)
                        {
                            using (RegistryKey key2 = baseKey.OpenSubKey(name, true))
                            {
                                key2.DeleteSubKey(subkey, true);
                            }
                        }
                        CleanKeyParents(baseKey, name);
                    }
                }
            }
        }

        [ArchitectureSensitive]
        internal static Collection<string> GetCertEKU(X509Certificate2 cert)
        {
            Collection<string> collection = new Collection<string>();
            IntPtr handle = cert.Handle;
            int pcbUsage = 0;
            IntPtr zero = IntPtr.Zero;
            if (System.Management.Automation.Security.NativeMethods.CertGetEnhancedKeyUsage(handle, 0, zero, out pcbUsage))
            {
                if (pcbUsage <= 0)
                {
                    return collection;
                }
                IntPtr pUsage = Marshal.AllocHGlobal(pcbUsage);
                try
                {
                    if (!System.Management.Automation.Security.NativeMethods.CertGetEnhancedKeyUsage(handle, 0, pUsage, out pcbUsage))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    System.Management.Automation.Security.NativeMethods.CERT_ENHKEY_USAGE cert_enhkey_usage = (System.Management.Automation.Security.NativeMethods.CERT_ENHKEY_USAGE) Marshal.PtrToStructure(pUsage, typeof(System.Management.Automation.Security.NativeMethods.CERT_ENHKEY_USAGE));
                    IntPtr rgpszUsageIdentifier = cert_enhkey_usage.rgpszUsageIdentifier;
                    for (int i = 0; i < cert_enhkey_usage.cUsageIdentifier; i++)
                    {
                        string item = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(rgpszUsageIdentifier, i * Marshal.SizeOf(rgpszUsageIdentifier)));
                        collection.Add(item);
                    }
                    return collection;
                }
                finally
                {
                    Marshal.FreeHGlobal(pUsage);
                }
            }
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        internal static uint GetDWORDFromInt(int n)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(n), 0);
        }

        internal static string GetExecutionPolicy(ExecutionPolicy policy)
        {
            switch (policy)
            {
                case ExecutionPolicy.Unrestricted:
                    return "Unrestricted";

                case ExecutionPolicy.RemoteSigned:
                    return "RemoteSigned";

                case ExecutionPolicy.AllSigned:
                    return "AllSigned";

                case ExecutionPolicy.Restricted:
                    return "Restricted";

                case ExecutionPolicy.Bypass:
                    return "Bypass";
            }
            return "Restricted";
        }

        internal static ExecutionPolicy GetExecutionPolicy(string shellId)
        {
            foreach (ExecutionPolicyScope scope in ExecutionPolicyScopePreferences)
            {
                ExecutionPolicy executionPolicy = GetExecutionPolicy(shellId, scope);
                if (executionPolicy != ExecutionPolicy.Undefined)
                {
                    return executionPolicy;
                }
            }
            return ExecutionPolicy.Restricted;
        }

        internal static ExecutionPolicy GetExecutionPolicy(string shellId, ExecutionPolicyScope scope)
        {
            string groupPolicyValue;
            bool flag;
            switch (scope)
            {
                case ExecutionPolicyScope.Process:
                {
                    string environmentVariable = Environment.GetEnvironmentVariable("PSExecutionPolicyPreference");
                    if (string.IsNullOrEmpty(environmentVariable))
                    {
                        return ExecutionPolicy.Undefined;
                    }
                    return ParseExecutionPolicy(environmentVariable);
                }
                case ExecutionPolicyScope.CurrentUser:
                case ExecutionPolicyScope.LocalMachine:
                {
                    string localPreferenceValue = GetLocalPreferenceValue(shellId, scope);
                    if (string.IsNullOrEmpty(localPreferenceValue))
                    {
                        return ExecutionPolicy.Undefined;
                    }
                    return ParseExecutionPolicy(localPreferenceValue);
                }
                case ExecutionPolicyScope.UserPolicy:
                case ExecutionPolicyScope.MachinePolicy:
                {
                    groupPolicyValue = GetGroupPolicyValue(shellId, scope);
                    if (string.IsNullOrEmpty(groupPolicyValue))
                    {
                        goto Label_00BA;
                    }
					if (OSHelper.IsUnix) { flag = true;}
					else {
	                    Process currentProcess = Process.GetCurrentProcess();
	                    string a = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "gpscript.exe");
	                    flag = false;
	                    try
	                    {
	                        while (currentProcess != null)
	                        {
	                            if (string.Equals(a, PsUtils.GetMainModule(currentProcess).FileName, StringComparison.OrdinalIgnoreCase))
	                            {
	                                flag = true;
	                                break;
	                            }
	                            currentProcess = PsUtils.GetParentProcess(currentProcess);
	                        }
	                    }
	                    catch (Win32Exception)
	                    {
	                    }
					}
                    break;
                }
                default:
                    return ExecutionPolicy.Restricted;
            }
            if (!flag)
            {
                return ParseExecutionPolicy(groupPolicyValue);
            }
        Label_00BA:
            return ExecutionPolicy.Undefined;
        }

        private static string GetGroupPolicyValue (string shellId, ExecutionPolicyScope scope)
		{
			switch (scope) {
			case ExecutionPolicyScope.UserPolicy:
				if (OSHelper.IsUnix)
				{
					return GetExecutionPolicy (ExecutionPolicy.Unrestricted); //TODO: REVIEW: URGENT:
				}
				else {
						try {
							using (RegistryKey key2 = Registry.CurrentUser.OpenSubKey(@"Software\Policies\Microsoft\Windows\PowerShell")) {
								switch (GetRegistryKeyFromGroupPolicyTest (@"Software\Policies\Microsoft\Windows\PowerShell", "EnableScripts", key2)) {
								case GroupPolicyStatus.Disabled:
									key2.Close ();
									return "Restricted";

								case GroupPolicyStatus.Enabled:
									return (key2.GetValue ("ExecutionPolicy") as string);
								}
							}
						} catch (SecurityException) {
						}
						return null;
					}

                case ExecutionPolicyScope.MachinePolicy:
                    if (OSHelper.IsUnix)
					{
						return GetExecutionPolicy (ExecutionPolicy.Unrestricted); //TODO: REVIEW: URGENT:
					}
					else {
						try
	                    {
	                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Policies\Microsoft\Windows\PowerShell"))
	                        {
	                            if (key != null)
	                            {
	                                switch (GetRegistryKeyFromGroupPolicyTest(@"Software\Policies\Microsoft\Windows\PowerShell", "EnableScripts", key))
	                                {
	                                    case GroupPolicyStatus.Disabled:
	                                        key.Close();
	                                        return "Restricted";

	                                    case GroupPolicyStatus.Enabled:
	                                        return (key.GetValue("ExecutionPolicy") as string);
	                                }
	                            }
	                        }
	                    }
	                    catch (SecurityException)
	                    {
	                    }
	                    return null;
					}
            }
            return null;
        }

        internal static int GetIntFromDWORD(uint n)
        {
            long num = n - 0x100000000L;
            return (int) num;
        }

        private static string GetLocalPreferenceValue(string shellId, ExecutionPolicyScope scope)
        {
            string registryConfigurationPath = Utils.GetRegistryConfigurationPath(shellId);
            switch (scope)
            {
                case ExecutionPolicyScope.CurrentUser:
                {
					if (OSHelper.IsUnix) {
						return "Unrestricted";
					}
					else {
	                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryConfigurationPath))
	                    {
	                        if (key != null)
	                        {
	                            string str2 = key.GetValue("ExecutionPolicy") as string;
	                            key.Close();
	                            return str2;
	                        }
	                        break;
	                    }
					}
                }
                case ExecutionPolicyScope.LocalMachine:
					if (OSHelper.IsUnix) {
						return "Unrestricted";
					}
					else {
	                    using (RegistryKey key2 = Registry.LocalMachine.OpenSubKey(registryConfigurationPath))
	                    {
	                        if (key2 != null)
	                        {
	                            string str3 = key2.GetValue("ExecutionPolicy") as string;
	                            key2.Close();
	                            return str3;
	                        }
	                    }
					}
                    break;
            }
            return null;
        }

        private static GroupPolicyStatus GetRegistryKeyFromGroupPolicyTest(string GroupPolicyKey, string GroupPolicyValue, RegistryKey key)
        {
            if (key != null)
            {
                object obj2 = key.GetValue(GroupPolicyValue);
                if (obj2 != null)
                {
                    if (string.Equals(obj2.ToString(), "0", StringComparison.OrdinalIgnoreCase))
                    {
                        return GroupPolicyStatus.Disabled;
                    }
                    if (string.Equals(obj2.ToString(), "1", StringComparison.OrdinalIgnoreCase))
                    {
                        return GroupPolicyStatus.Enabled;
                    }
                    return GroupPolicyStatus.Undefined;
                }
            }
            return GroupPolicyStatus.Undefined;
        }

        [ArchitectureSensitive]
        internal static SaferPolicy GetSaferPolicy(string path)
        {
            IntPtr ptr;
            SaferPolicy allowed = SaferPolicy.Allowed;
            SAFER_CODE_PROPERTIES pCodeProperties = new SAFER_CODE_PROPERTIES {
                cbSize = (int) Marshal.SizeOf(typeof(SAFER_CODE_PROPERTIES)),
                dwCheckFlags = 13,
                ImagePath = path,
                dwWVTUIChoice = 2
            };
            if (System.Management.Automation.Security.NativeMethods.SaferIdentifyLevel(1, ref pCodeProperties, out ptr, "SCRIPT"))
            {
                IntPtr zero = IntPtr.Zero;
                try
                {
                    if (!System.Management.Automation.Security.NativeMethods.SaferComputeTokenFromLevel(ptr, IntPtr.Zero, ref zero, 1, IntPtr.Zero))
                    {
                        int num = Marshal.GetLastWin32Error();
                        if ((num != 0x4ec) && (num != 0x312))
                        {
                            throw new Win32Exception();
                        }
                        return SaferPolicy.Disallowed;
                    }
                    if (zero == IntPtr.Zero)
                    {
                        return SaferPolicy.Allowed;
                    }
                    allowed = SaferPolicy.Disallowed;
                    System.Management.Automation.Security.NativeMethods.CloseHandle(zero);
                    return allowed;
                }
                finally
                {
                    System.Management.Automation.Security.NativeMethods.SaferCloseLevel(ptr);
                }
            }
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        internal static ExecutionPolicy ParseExecutionPolicy(string policy)
        {
            if (string.Equals(policy, "Bypass", StringComparison.OrdinalIgnoreCase))
            {
                return ExecutionPolicy.Bypass;
            }
            if (string.Equals(policy, "Unrestricted", StringComparison.OrdinalIgnoreCase))
            {
                return ExecutionPolicy.Unrestricted;
            }
            if (string.Equals(policy, "RemoteSigned", StringComparison.OrdinalIgnoreCase))
            {
                return ExecutionPolicy.RemoteSigned;
            }
            if (string.Equals(policy, "AllSigned", StringComparison.OrdinalIgnoreCase))
            {
                return ExecutionPolicy.AllSigned;
            }
            if (string.Equals(policy, "Restricted", StringComparison.OrdinalIgnoreCase))
            {
                return ExecutionPolicy.Restricted;
            }
            return ExecutionPolicy.Restricted;
        }

        internal static void SetExecutionPolicy(ExecutionPolicyScope scope, ExecutionPolicy policy, string shellId)
        {
            string str = "Restricted";
            string registryConfigurationPath = Utils.GetRegistryConfigurationPath(shellId);
            switch (policy)
            {
                case ExecutionPolicy.Unrestricted:
                    str = "Unrestricted";
                    break;

                case ExecutionPolicy.RemoteSigned:
                    str = "RemoteSigned";
                    break;

                case ExecutionPolicy.AllSigned:
                    str = "AllSigned";
                    break;

                case ExecutionPolicy.Restricted:
                    str = "Restricted";
                    break;

                case ExecutionPolicy.Bypass:
                    str = "Bypass";
                    break;
            }
            switch (scope)
            {
                case ExecutionPolicyScope.Process:
                    if (policy == ExecutionPolicy.Undefined)
                    {
                        str = null;
                    }
                    Environment.SetEnvironmentVariable("PSExecutionPolicyPreference", str);
                    return;

                case ExecutionPolicyScope.CurrentUser:
                    if (policy != ExecutionPolicy.Undefined)
                    {
                        using (RegistryKey key2 = Registry.CurrentUser.CreateSubKey(registryConfigurationPath))
                        {
                            key2.SetValue("ExecutionPolicy", str, RegistryValueKind.String);
                            return;
                        }
                    }
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryConfigurationPath, true))
                    {
                        if ((key != null) && (key.GetValue("ExecutionPolicy") != null))
                        {
                            key.DeleteValue("ExecutionPolicy");
                        }
                    }
                    CleanKeyParents(Registry.CurrentUser, registryConfigurationPath);
                    return;

                case ExecutionPolicyScope.LocalMachine:
                    break;

                default:
                    return;
            }
            if (policy == ExecutionPolicy.Undefined)
            {
                using (RegistryKey key3 = Registry.LocalMachine.OpenSubKey(registryConfigurationPath, true))
                {
                    if ((key3 != null) && (key3.GetValue("ExecutionPolicy") != null))
                    {
                        key3.DeleteValue("ExecutionPolicy");
                    }
                }
                CleanKeyParents(Registry.LocalMachine, registryConfigurationPath);
            }
            else
            {
                using (RegistryKey key4 = Registry.LocalMachine.CreateSubKey(registryConfigurationPath))
                {
                    key4.SetValue("ExecutionPolicy", str, RegistryValueKind.String);
                }
            }
        }

        internal static ExecutionPolicyScope[] ExecutionPolicyScopePreferences
        {
            get
            {
                ExecutionPolicyScope[] scopeArray = new ExecutionPolicyScope[5];
                scopeArray[0] = ExecutionPolicyScope.MachinePolicy;
                scopeArray[1] = ExecutionPolicyScope.UserPolicy;
                scopeArray[3] = ExecutionPolicyScope.CurrentUser;
                scopeArray[4] = ExecutionPolicyScope.LocalMachine;
                return scopeArray;
            }
        }

        private enum GroupPolicyStatus
        {
            Enabled,
            Disabled,
            Undefined
        }
    }
}

