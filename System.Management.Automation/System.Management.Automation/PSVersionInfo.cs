namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation.Remoting.Client;
    using System.Reflection;

    internal class PSVersionInfo
    {
        private static Version _psV1Version = new Version(1, 0);
        private static Version _psV2Version = new Version(2, 0);
        private static Version _psV3Version = new Version(3, 0);
        private static Hashtable _psVersionTable = null;
        private static object lockObject = new object();
        internal const string PSRemotingProtocolVersionName = "PSRemotingProtocolVersion";
        internal const string PSVersionName = "PSVersion";
        internal const string PSVersionTableName = "PSVersionTable";
        internal const string SerializationVersionName = "SerializationVersion";
        internal const string WSManStackVersionName = "WSManStackVersion";

        internal static Hashtable GetPSVersionTable()
        {
            if (_psVersionTable == null)
            {
                lock (lockObject)
                {
                    if (_psVersionTable == null)
                    {
                        Hashtable listToInitialize = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        listToInitialize.Add("PSVersion",_psV3Version);
                        listToInitialize.Add("CLRVersion", Environment.Version);
                        string fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
						listToInitialize.Add("BuildVersion", new Version(fileVersion));
                        listToInitialize.Add("PSCompatibleVersions", new Version[] { _psV1Version, _psV2Version, _psV3Version });
                        listToInitialize.Add("SerializationVersion", new Version("1.1.0.1"));
                        listToInitialize.Add("PSRemotingProtocolVersion", RemotingConstants.ProtocolVersion);
                        listToInitialize.Add("WSManStackVersion", WSManNativeApi.WSMAN_STACK_VERSION);
                        _psVersionTable = new PSVersionHashTable(listToInitialize);
                    }
                }
            }
            return _psVersionTable;
        }

        internal static string GetRegisterVersionKeyForSnapinDiscovery(string majorVersion)
        {
            string str;
            if (((str = majorVersion) == null) || ((!(str == "1") && !(str == "2")) && !(str == "3")))
            {
                return null;
            }
            return "1";
        }

        internal static bool IsValidPSVersion(Version version)
        {
            if (version.Major == _psV1Version.Major)
            {
                return (version.Minor == _psV1Version.Minor);
            }
            if (version.Major == _psV2Version.Major)
            {
                return (version.Minor == _psV2Version.Minor);
            }
            return ((version.Major == _psV3Version.Major) && (version.Minor == _psV3Version.Minor));
        }

        internal static Version BuildVersion
        {
            get
            {
                return (Version) GetPSVersionTable()["BuildVersion"];
            }
        }

        internal static Version CLRVersion
        {
            get
            {
                return (Version) GetPSVersionTable()["CLRVersion"];
            }
        }

        internal static string FeatureVersionString
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { PSVersion.Major, PSVersion.Minor });
            }
        }

        internal static Version[] PSCompatibleVersions
        {
            get
            {
                return (Version[]) GetPSVersionTable()["PSCompatibleVersions"];
            }
        }

        internal static Version PSVersion
        {
            get
            {
				return (Version)GetPSVersionTable()["PSVersion"];
            }
        }

        internal static string RegistryVersion1Key
        {
            get
            {
                return "1";
            }
        }

        internal static string RegistryVersionKey
        {
            get
            {
                return "3";
            }
        }

        internal static Version SerializationVersion
        {
            get
            {
                return (Version) GetPSVersionTable()["SerializationVersion"];
            }
        }
    }
}

