namespace System.Management.Automation.Security
{
    using Microsoft.Win32;
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public sealed class SystemPolicy
    {
        private static bool hadMissingWldpAssembly = false;
        private static SystemEnforcementMode? systemLockdownPolicy = null;
        private static object systemLockdownPolicyLock = new object();
        private static bool wasSystemPolicyDebugPolicy = false;

        private SystemPolicy()
        {
        }

        internal static string DumpLockdownState(int pdwLockdownState)
        {
            string str = "";
            if ((pdwLockdownState & 0x80000000) == 0x80000000)
            {
                str = str + "WLDP_LOCKDOWN_DEFINED_FLAG\r\n";
            }
            if ((pdwLockdownState & 1) == 1)
            {
                str = str + "WLDP_LOCKDOWN_SECUREBOOT_FLAG\r\n";
            }
            if ((pdwLockdownState & 2) == 2)
            {
                str = str + "WLDP_LOCKDOWN_DEBUGPOLICY_FLAG\r\n";
            }
            if ((pdwLockdownState & 4) == 4)
            {
                str = str + "WLDP_LOCKDOWN_UMCIENFORCE_FLAG\r\n";
            }
            if ((pdwLockdownState & 8) == 8)
            {
                str = str + "WLDP_LOCKDOWN_UMCIAUDIT_FLAG\r\n";
            }
            return str;
        }

        private static SystemEnforcementMode GetDebugLockdownPolicy(string path)
        {
            if (PsUtils.IsRunningOnProcessorArchitectureARM())
            {
                return SystemEnforcementMode.Enforce;
            }
            wasSystemPolicyDebugPolicy = true;
            if (path != null)
            {
                if (path.IndexOf("System32", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return SystemEnforcementMode.None;
                }
                using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default))
                {
                    using (RegistryKey key2 = key.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\CI\TRSData"))
                    {
                        if (key2 != null)
                        {
                            object obj2 = key2.GetValue("TestPath");
                            key2.Close();
                            key.Close();
                            if (obj2 != null)
                            {
                                string[] strArray = (string[]) obj2;
                                foreach (string str in strArray)
                                {
                                    if (path.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        return SystemEnforcementMode.None;
                                    }
                                }
                            }
                        }
                    }
                }
                return GetSystemLockdownPolicy();
            }
            object environmentVariable = Environment.GetEnvironmentVariable("__PSLockdownPolicy", EnvironmentVariableTarget.Machine);
            if (environmentVariable != null)
            {
                return GetLockdownPolicyForResult(LanguagePrimitives.ConvertTo<int>(environmentVariable));
            }
            return SystemEnforcementMode.None;
        }

        public static SystemEnforcementMode GetLockdownPolicy(string path, SafeHandle handle)
        {
            try
            {
                if (hadMissingWldpAssembly)
                {
                    return GetDebugLockdownPolicy(path);
                }
                WLDP_HOST_INFORMATION pHostInformation = new WLDP_HOST_INFORMATION {
                    dwRevision = 1,
                    dwHostId = WLDP_HOST_ID.WLDP_HOST_ID_POWERSHELL
                };
                if (!string.IsNullOrEmpty(path))
                {
                    pHostInformation.szSource = path;
                    if (handle != null)
                    {
                        IntPtr zero = IntPtr.Zero;
                        zero = handle.DangerousGetHandle();
                        pHostInformation.hSource = zero;
                    }
                }
                int pdwLockdownState = 0;
                WldpNativeMethods.WldpGetLockdownPolicy(ref pHostInformation, ref pdwLockdownState, 0);
                return GetLockdownPolicyForResult(pdwLockdownState);
            }
            catch (DllNotFoundException)
            {
                hadMissingWldpAssembly = true;
                return GetDebugLockdownPolicy(path);
            }
        }

        private static SystemEnforcementMode GetLockdownPolicyForResult(int pdwLockdownState)
        {
            if ((pdwLockdownState & 8) == 8)
            {
                return SystemEnforcementMode.Audit;
            }
            if ((pdwLockdownState & 4) == 4)
            {
                return SystemEnforcementMode.Enforce;
            }
            return SystemEnforcementMode.None;
        }

        public static SystemEnforcementMode GetSystemLockdownPolicy()
        {
            if (wasSystemPolicyDebugPolicy || !systemLockdownPolicy.HasValue)
            {
                lock (systemLockdownPolicyLock)
                {
                    if (wasSystemPolicyDebugPolicy || !systemLockdownPolicy.HasValue)
                    {
                        systemLockdownPolicy = new SystemEnforcementMode?(GetLockdownPolicy(null, null));
                    }
                }
            }
            return systemLockdownPolicy.Value;
        }

        internal static bool IsClassInApprovedList(Guid clsid)
        {
            try
            {
                WLDP_HOST_INFORMATION pHostInformation = new WLDP_HOST_INFORMATION {
                    dwRevision = 1,
                    dwHostId = WLDP_HOST_ID.WLDP_HOST_ID_POWERSHELL
                };
                int ptIsApproved = 0;
                WldpNativeMethods.WldpIsClassInApprovedList(ref clsid, ref pHostInformation, ref ptIsApproved, 0);
                return (ptIsApproved == 1);
            }
            catch (DllNotFoundException)
            {
                return string.Equals(clsid.ToString(), "f6d90f11-9c73-11d3-b32e-00c04f990bb4", StringComparison.OrdinalIgnoreCase);
            }
        }

        internal static bool XamlWorkflowSupported
        {
            get;
            set;
        }

        internal enum WLDP_HOST_ID
        {
            WLDP_HOST_ID_UNKNOWN,
            WLDP_HOST_ID_GLOBAL,
            WLDP_HOST_ID_VBA,
            WLDP_HOST_ID_WSH,
            WLDP_HOST_ID_POWERSHELL,
            WLDP_HOST_ID_IE,
            WLDP_HOST_ID_MSI,
            WLDP_HOST_ID_MAX
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WLDP_HOST_INFORMATION
        {
            internal int dwRevision;
            internal SystemPolicy.WLDP_HOST_ID dwHostId;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string szSource;
            internal IntPtr hSource;
        }

        internal class WldpNativeConstants
        {
            internal const int WLDP_HOST_INFORMATION_REVISION = 1;
            internal const int WLDP_LOCKDOWN_DEBUGPOLICY_FLAG = 2;
            internal const uint WLDP_LOCKDOWN_DEFINED_FLAG = 0x80000000;
            internal const int WLDP_LOCKDOWN_SECUREBOOT_FLAG = 1;
            internal const int WLDP_LOCKDOWN_UMCIAUDIT_FLAG = 8;
            internal const int WLDP_LOCKDOWN_UMCIENFORCE_FLAG = 4;
            internal const int WLDP_LOCKDOWN_UNDEFINED = 0;
        }

        internal class WldpNativeMethods
        {
            /*
            [DllImport("wldp.dll")]
            internal static extern int WldpGetLockdownPolicy(ref SystemPolicy.WLDP_HOST_INFORMATION pHostInformation, ref int pdwLockdownState, int dwFlags);
            [DllImport("wldp.dll")]
            internal static extern int WldpIsClassInApprovedList(ref Guid rclsid, ref SystemPolicy.WLDP_HOST_INFORMATION pHostInformation, ref int ptIsApproved, int dwFlags);
             */

            internal static int WldpGetLockdownPolicy (ref SystemPolicy.WLDP_HOST_INFORMATION pHostInformation, ref int pdwLockdownState, int dwFlags)
			{
				if (pHostInformation.szSource != null) {
					var fi = new System.IO.FileInfo (pHostInformation.szSource);
					dwFlags = 1;
					if (fi.Exists) {
						if (fi.Directory.FullName.IndexOf (PowerShellConfiguration.PowerShellEngine.ApplicationBase, StringComparison.OrdinalIgnoreCase) != -1) {
							pdwLockdownState = WldpNativeConstants.WLDP_LOCKDOWN_UNDEFINED;
							return 1;
						}
					}
				}
				pdwLockdownState = WldpNativeConstants.WLDP_LOCKDOWN_UMCIENFORCE_FLAG;
                return 1;
            }

            internal static int WldpIsClassInApprovedList(ref Guid rclsid, ref SystemPolicy.WLDP_HOST_INFORMATION pHostInformation, ref int ptIsApproved, int dwFlags)
            {
                ptIsApproved = 1;
                return 1;
            }
        }
    }
}

