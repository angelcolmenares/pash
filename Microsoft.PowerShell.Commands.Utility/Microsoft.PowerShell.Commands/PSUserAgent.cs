namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Globalization;
    using System.Management.Automation;

    public static class PSUserAgent
    {
        private static string GetOSName(PlatformID platformId)
        {
            switch (platformId)
            {
                case PlatformID.Win32Windows:
                    return "Windows";

                case PlatformID.Win32NT:
                    return "Windows NT";
            }
            return platformId.ToString();
        }

        internal static string App
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "WindowsPowerShell/{0}", new object[] { PSVersionInfo.PSVersion });
            }
        }

        public static string Chrome
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1}; {2}; {3}) AppleWebKit/534.6 (KHTML, like Gecko) Chrome/7.0.500.0 Safari/534.6", new object[] { Compatibility, Platform, OS, Culture });
            }
        }

        internal static string Compatibility
        {
            get
            {
                return "Mozilla/5.0";
            }
        }

        internal static string Culture
        {
            get
            {
                return CultureInfo.CurrentCulture.Name;
            }
        }

        public static string FireFox
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1}; {2}; {3}) Gecko/20100401 Firefox/4.0", new object[] { Compatibility, Platform, OS, Culture });
            }
        }

        public static string InternetExplorer
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} (compatible; MSIE 9.0; {1}; {2}; {3})", new object[] { Compatibility, Platform, OS, Culture });
            }
        }

        public static string Opera
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "Opera/9.70 ({0}; {1}; {2}) Presto/2.2.1", new object[] { Platform, OS, Culture });
            }
        }

        internal static string OS
        {
            get
            {
                OperatingSystem oSVersion = Environment.OSVersion;
                string oSName = GetOSName(oSVersion.Platform);
                return string.Format(CultureInfo.InvariantCulture, "{0} {1}.{2}", new object[] { oSName, oSVersion.Version.Major, oSVersion.Version.Minor });
            }
        }

        internal static string Platform
        {
            get
            {
                return "Windows NT";
            }
        }

        public static string Safari
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1}; {2}; {3}) AppleWebKit/533.16 (KHTML, like Gecko) Version/5.0 Safari/533.16", new object[] { Compatibility, Platform, OS, Culture });
            }
        }

        internal static string UserAgent
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} ({1}; {2}; {3}) {4}", new object[] { Compatibility, Platform, OS, Culture, App });
            }
        }
    }
}

