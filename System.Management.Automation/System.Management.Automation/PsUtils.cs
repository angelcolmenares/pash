namespace System.Management.Automation
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal static class PsUtils
    {
        internal static string ArmArchitecture = "ARM";

        private static bool FileSystemIsDotNetFrameworkVersionInstalled(Version requiredVersion)
        {
            string str = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "Microsoft.NET");
            try
            {
                string str2 = "Framework";
                bool isRunningOnArm = false;
                if (((GetProcessorArchitecture(out isRunningOnArm) != ProcessorArchitecture.X86) && !isRunningOnArm) && (requiredVersion >= new Version(2, 0)))
                {
                    str2 = "Framework64";
                }
                string[] directories = Directory.GetDirectories(Path.Combine(str, str2), string.Format(null, "v{0}.{1}*", new object[] { requiredVersion.Major, requiredVersion.Minor }));
                if ((directories == null) || (directories.Length == 0))
                {
                    return false;
                }
                if ((requiredVersion.Build != -1) || (requiredVersion.Revision != -1))
                {
                    string path = Path.Combine(directories[0], "mscorlib.dll");
                    if (File.Exists(path))
                    {
                        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
                        Version version = new Version((versionInfo.FileMajorPart < 0) ? 0 : versionInfo.FileMajorPart, (versionInfo.FileMinorPart < 0) ? 0 : versionInfo.FileMinorPart, (versionInfo.FileBuildPart < 0) ? 0 : versionInfo.FileBuildPart, (versionInfo.FilePrivatePart < 0) ? 0 : versionInfo.FilePrivatePart);
                        if (version < requiredVersion)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (PathTooLongException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

        internal static ProcessModule GetMainModule(Process targetProcess)
        {
            int num = 0;
            ProcessModule mainModule = null;
            while (mainModule == null)
            {
                try
                {
                    mainModule = targetProcess.MainModule;
                    continue;
                }
                catch (Win32Exception)
                {
                    num++;
                    Thread.Sleep(100);
                    if (num == 5)
                    {
                        throw;
                    }
                    continue;
                }
            }
            return mainModule;
        }

        internal static Process GetParentProcess(Process current)
        {
            Process process2;
			if (OSHelper.IsUnix) return current; //TODO: IMPLEMENT Managmenet Object
            using (ManagementObject obj2 = new ManagementObject(string.Format(Thread.CurrentThread.CurrentCulture, "win32_process.handle='{0}'", new object[] { current.Id })))
            {
                obj2.Get();
                int processId = Convert.ToInt32(obj2["ParentProcessId"], CultureInfo.CurrentCulture);
                if (processId == 0)
                {
                    process2 = null;
                }
                else
                {
                    try
                    {
                        Process processById = Process.GetProcessById(processId);
                        if (processById.StartTime <= current.StartTime)
                        {
                            return processById;
                        }
                        process2 = null;
                    }
                    catch (ArgumentException)
                    {
                        process2 = null;
                    }
                }
            }
            return process2;
        }

        internal static ProcessorArchitecture GetProcessorArchitecture(out bool isRunningOnArm)
        {
            NativeMethods.SYSTEM_INFO lpSystemInfo = new NativeMethods.SYSTEM_INFO();
            NativeMethods.GetSystemInfo(ref lpSystemInfo);
            isRunningOnArm = false;
            switch (lpSystemInfo.wProcessorArchitecture)
            {
                case 5:
                {
                    ProcessorArchitecture none = ProcessorArchitecture.None;
                    isRunningOnArm = true;
                    return none;
                }
                case 6:
                    return ProcessorArchitecture.IA64;

                case 9:
                    return ProcessorArchitecture.Amd64;

                case 0:
                    return ProcessorArchitecture.X86;
            }
            return ProcessorArchitecture.None;
        }

        internal static bool IsDotNetFrameworkVersionInstalled(Version requiredVersion)
        {
            int num;
            int num2;
            int num3;
            if (FrameworkRegistryInstallation.CanCheckFrameworkInstallation(requiredVersion, out num, out num2, out num3))
            {
                return FrameworkRegistryInstallation.IsFrameworkInstalled(num, num2, num3);
            }
            return FileSystemIsDotNetFrameworkVersionInstalled(requiredVersion);
        }

        internal static bool IsRunningOnProcessorArchitectureARM()
        {
            NativeMethods.SYSTEM_INFO lpSystemInfo = new NativeMethods.SYSTEM_INFO();
            NativeMethods.GetSystemInfo(ref lpSystemInfo);
            return (lpSystemInfo.wProcessorArchitecture == 5);
        }

        internal static class FrameworkRegistryInstallation
        {
            internal static Dictionary<Version, HashSet<Version>> CompatibleNetFrameworkVersions;
            internal static Version KnownHighestNetFrameworkVersion;
            private static Version V1_1 = new Version(1, 1, 0x10e2, 0x23d);
            private static Version V1_1_00 = new Version(1, 1, 0, 0);
            private static Version V1_1sp1 = new Version(1, 1, 0x10e2, 0x7f0);
            private static Version V1_1sp1Server = new Version(1, 1, 0x10e2, 0x8fc);
            private static Version V2_0 = new Version(2, 0, 0xc627, 0x2a);
            private static Version V2_0_00 = new Version(2, 0, 0, 0);
            private static Version V2_0sp1 = new Version(2, 0, 0xc627, 0x599);
            private static Version V2_0sp2 = new Version(2, 0, 0xc627, 0xbed);
            private static Version V3_0 = new Version(3, 0, 0x119a, 30);
            private static Version V3_0_00 = new Version(3, 0, 0, 0);
            private static Version V3_0sp1 = new Version(3, 0, 0x119a, 0x288);
            private static Version V3_0sp2 = new Version(3, 0, 0x119a, 0x868);
            private static Version V3_5 = new Version(3, 5, 0x521e, 8);
            private static Version V3_5_00 = new Version(3, 5, 0, 0);
            private static Version V3_5sp1 = new Version(3, 5, 0x7809, 1);
            private static Version V4_0 = new Version(4, 0, 0x766f, 0);
            private static Version V4_0_00 = new Version(4, 0, 0, 0);
            private static Version V4_5_00 = new Version(4, 5, 0, 0);

            static FrameworkRegistryInstallation()
            {
                Dictionary<Version, HashSet<Version>> dictionary = new Dictionary<Version, HashSet<Version>>();
                dictionary.Add(V1_1_00, new HashSet<Version> { V4_5_00, V4_0_00, V3_5_00, V3_0_00, V2_0_00 });
                dictionary.Add(V2_0_00, new HashSet<Version> { V4_5_00, V4_0_00, V3_5_00, V3_0_00 });
                dictionary.Add(V3_0_00, new HashSet<Version> { V4_5_00, V4_0_00, V3_5_00 });
                dictionary.Add(V3_5_00, new HashSet<Version> { V4_5_00, V4_0_00 });
                dictionary.Add(V4_0_00, new HashSet<Version> { V4_5_00 });
                dictionary.Add(V4_5_00, new HashSet<Version>());
                CompatibleNetFrameworkVersions = dictionary;
                KnownHighestNetFrameworkVersion = new Version(4, 5);
            }

            internal static bool CanCheckFrameworkInstallation(Version version, out int majorVersion, out int minorVersion, out int minimumSpVersion)
            {
                majorVersion = -1;
                minorVersion = -1;
                minimumSpVersion = -1;
                if (version == V4_5_00)
                {
                    majorVersion = 4;
                    minorVersion = 5;
                    minimumSpVersion = 0;
                    return true;
                }
                if ((version == V4_0) || (version == V4_0_00))
                {
                    majorVersion = 4;
                    minorVersion = 0;
                    minimumSpVersion = 0;
                    return true;
                }
                if ((version == V3_5) || (version == V3_5_00))
                {
                    majorVersion = 3;
                    minorVersion = 5;
                    minimumSpVersion = 0;
                    return true;
                }
                if (version == V3_5sp1)
                {
                    majorVersion = 3;
                    minorVersion = 5;
                    minimumSpVersion = 1;
                    return true;
                }
                if ((version == V3_0) || (version == V3_0_00))
                {
                    majorVersion = 3;
                    minorVersion = 0;
                    minimumSpVersion = 0;
                    return true;
                }
                if (version == V3_0sp1)
                {
                    majorVersion = 3;
                    minorVersion = 0;
                    minimumSpVersion = 1;
                    return true;
                }
                if (version == V3_0sp2)
                {
                    majorVersion = 3;
                    minorVersion = 0;
                    minimumSpVersion = 2;
                    return true;
                }
                if ((version == V2_0) || (version == V2_0_00))
                {
                    majorVersion = 2;
                    minorVersion = 0;
                    minimumSpVersion = 0;
                    return true;
                }
                if (version == V2_0sp1)
                {
                    majorVersion = 2;
                    minorVersion = 0;
                    minimumSpVersion = 1;
                    return true;
                }
                if (version == V2_0sp2)
                {
                    majorVersion = 2;
                    minorVersion = 0;
                    minimumSpVersion = 2;
                    return true;
                }
                if ((version == V1_1) || (version == V1_1_00))
                {
                    majorVersion = 1;
                    minorVersion = 1;
                    minimumSpVersion = 0;
                    return true;
                }
                if (!(version == V1_1sp1) && !(version == V1_1sp1Server))
                {
                    return false;
                }
                majorVersion = 1;
                minorVersion = 1;
                minimumSpVersion = 1;
                return true;
            }

            private static RegistryKey GetRegistryKeySubKey(RegistryKey key, string subKeyName)
            {
                try
                {
                    return key.OpenSubKey(subKeyName);
                }
                catch (ObjectDisposedException)
                {
                    return null;
                }
                catch (SecurityException)
                {
                    return null;
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }

            private static int? GetRegistryKeyValueInt(RegistryKey key, string valueName)
            {
                try
                {
                    object obj2 = key.GetValue(valueName);
                    if (obj2 is int)
                    {
                        return new int?((int) obj2);
                    }
                    return null;
                }
                catch (ObjectDisposedException)
                {
                    return null;
                }
                catch (SecurityException)
                {
                    return null;
                }
                catch (IOException)
                {
                    return null;
                }
                catch (UnauthorizedAccessException)
                {
                    return null;
                }
            }

            private static bool GetRegistryNames(int majorVersion, int minorVersion, out string installKeyName, out string installValueName, out string spKeyName, out string spValueName)
            {
                installKeyName = null;
                spKeyName = null;
                installValueName = null;
                spValueName = "SP";
                if ((majorVersion == 1) && (minorVersion == 1))
                {
                    installKeyName = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v1.1.4322";
                    spKeyName = installKeyName;
                    installValueName = "Install";
                    return true;
                }
                if ((majorVersion == 2) && (minorVersion == 0))
                {
                    installKeyName = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v2.0.50727";
                    spKeyName = installKeyName;
                    installValueName = "Install";
                    return true;
                }
                if ((majorVersion == 3) && (minorVersion == 0))
                {
                    installKeyName = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.0\Setup";
                    spKeyName = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.0";
                    installValueName = "InstallSuccess";
                    return true;
                }
                if ((majorVersion == 3) && (minorVersion == 5))
                {
                    installKeyName = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5";
                    spKeyName = installKeyName;
                    installValueName = "Install";
                    return true;
                }
                if ((majorVersion == 4) && (minorVersion == 0))
                {
                    installKeyName = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client";
                    spKeyName = installKeyName;
                    installValueName = "Install";
                    spValueName = "Servicing";
                    return true;
                }
                if ((majorVersion == 4) && (minorVersion == 5))
                {
                    installKeyName = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full";
                    installValueName = "Release";
                    return true;
                }
                return false;
            }

            internal static bool IsFrameworkInstalled(Version version)
            {
                int num;
                int num2;
                int num3;
                if (!CanCheckFrameworkInstallation(version, out num2, out num, out num3))
                {
                    return false;
                }
                return IsFrameworkInstalled(num2, num, num3);
            }

            internal static bool IsFrameworkInstalled(int majorVersion, int minorVersion, int minimumSPVersion)
            {
                string str;
                string str2;
                string str3;
                string str4;
                if (!GetRegistryNames(majorVersion, minorVersion, out str, out str2, out str3, out str4))
                {
                    return false;
                }
                RegistryKey registryKeySubKey = GetRegistryKeySubKey(Registry.LocalMachine, str);
                if (registryKeySubKey == null)
                {
                    return false;
                }
                int? registryKeyValueInt = GetRegistryKeyValueInt(registryKeySubKey, str2);
                if (!registryKeyValueInt.HasValue)
                {
                    return false;
                }
                if (((majorVersion != 4) && (minorVersion != 5)) && (registryKeyValueInt != 1))
                {
                    return false;
                }
                if (minimumSPVersion > 0)
                {
                    RegistryKey key = GetRegistryKeySubKey(Registry.LocalMachine, str3);
                    if (key == null)
                    {
                        return false;
                    }
                    int? nullable2 = GetRegistryKeyValueInt(key, str4);
                    if (!nullable2.HasValue)
                    {
                        return false;
                    }
                    int? nullable4 = nullable2;
                    int num = minimumSPVersion;
                    if ((nullable4.GetValueOrDefault() < num) && nullable4.HasValue)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private static class NativeMethods
        {
            internal const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
            internal const ushort PROCESSOR_ARCHITECTURE_ARM = 5;
            internal const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
            internal const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
            internal const ushort PROCESSOR_ARCHITECTURE_UNKNOWN = 0xffff;

			/*
            [DllImport("kernel32.dll")]
            internal static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);
			*/

			internal static void GetSystemInfo (ref SYSTEM_INFO lpSystemInfo)
			{
				lpSystemInfo.wProcessorArchitecture = (Environment.Is64BitProcess ? PROCESSOR_ARCHITECTURE_AMD64 : PROCESSOR_ARCHITECTURE_INTEL);
				lpSystemInfo.dwProcessorType = (Environment.Is64BitProcess ? 9 : 0);
				lpSystemInfo.dwPageSize = Environment.SystemPageSize;
				lpSystemInfo.dwNumberOfProcessors = Environment.ProcessorCount;
			}


            [StructLayout(LayoutKind.Sequential)]
            internal struct SYSTEM_INFO
            {
                public ushort wProcessorArchitecture;
                public ushort wReserved;
                public int dwPageSize;
                public IntPtr lpMinimumApplicationAddress;
                public IntPtr lpMaximumApplicationAddress;
                public UIntPtr dwActiveProcessorMask;
                public int dwNumberOfProcessors;
                public int dwProcessorType;
                public int dwAllocationGranularity;
                public ushort wProcessorLevel;
                public ushort wProcessorRevision;
            }
        }
    }
}

