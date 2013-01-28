namespace Microsoft.PowerShell
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class NativeCultureResolver
    {
        private static CultureInfo m_Culture = null;
        private static object m_syncObject = new object();
        private static CultureInfo m_uiCulture = null;
        private static int MUI_CONSOLE_FILTER = 0x100;
        private static int MUI_LANGUAGE_NAME = 8;
        private static int MUI_MERGE_SYSTEM_FALLBACK = 0x10;
        private static int MUI_MERGE_USER_FALLBACK = 0x20;

        private static CultureInfo EmulateDownLevel()
        {
            CultureInfo info = new CultureInfo(GetUserDefaultUILanguage());
            return info.GetConsoleFallbackUICulture();
        }

        internal static CultureInfo GetCulture()
        {
            return GetCulture(true);
        }

        internal static CultureInfo GetCulture(bool filterOutNonConsoleCultures)
        {
            CultureInfo currentCulture;
            try
            {
                if (!IsVistaAndLater())
                {
                    currentCulture = new CultureInfo(GetUserDefaultLCID());
                }
                else
                {
                    StringBuilder lpLocaleName = new StringBuilder(0x10);
                    if (GetUserDefaultLocaleName(lpLocaleName, 0x10) == 0)
                    {
                        currentCulture = CultureInfo.CurrentCulture;
                    }
                    else
                    {
                        currentCulture = new CultureInfo(lpLocaleName.ToString().Trim());
                    }
                }
                if (filterOutNonConsoleCultures)
                {
                    currentCulture = CultureInfo.CreateSpecificCulture(currentCulture.GetConsoleFallbackUICulture().Name);
                }
            }
            catch (ArgumentException)
            {
                currentCulture = CultureInfo.CurrentCulture;
            }
            return currentCulture;
        }
		/*
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        private static extern bool GetThreadPreferredUILanguages(int dwFlags, out long pulNumLanguages, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] pwszLanguagesBuffer, out int pcchLanguagesBuffer);
		*/

		private static bool GetThreadPreferredUILanguages(int dwFlags, out long pulNumLanguages, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] pwszLanguagesBuffer, out int pcchLanguagesBuffer)
		{
			pulNumLanguages = 0L;
			pcchLanguagesBuffer = 0;
			return true;
		}


		internal static CultureInfo GetUICulture()
        {
            return GetUICulture(true);
        }

        internal static CultureInfo GetUICulture(bool filterOutNonConsoleCultures)
        {
            if (!IsVistaAndLater())
            {
                m_uiCulture = EmulateDownLevel();
                return m_uiCulture;
            }
            string userPreferredUILangs = GetUserPreferredUILangs(filterOutNonConsoleCultures);
            if (!string.IsNullOrEmpty(userPreferredUILangs))
            {
                try
                {
                    char[] separator = new char[1];
                    string[] sourceArray = userPreferredUILangs.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    string name = sourceArray[0];
                    string[] destinationArray = null;
                    if (sourceArray.Length > 1)
                    {
                        destinationArray = new string[sourceArray.Length - 1];
                        Array.Copy(sourceArray, 1, destinationArray, 0, sourceArray.Length - 1);
                    }
                    m_uiCulture = new VistaCultureInfo(name, destinationArray);
                    return m_uiCulture;
                }
                catch (ArgumentException)
                {
                }
            }
            m_uiCulture = EmulateDownLevel();
            return m_uiCulture;
        }

		/*
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        private static extern int GetUserDefaultLCID();
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        private static extern int GetUserDefaultLocaleName([MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpLocaleName, int cchLocaleName);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern ushort GetUserDefaultUILanguage();
		*/

		private static int GetUserDefaultLCID()
		{
			return 1033;
		}

		private static int GetUserDefaultLocaleName([MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpLocaleName, int cchLocaleName)
		{
			return 1033;
		}

		internal static ushort GetUserDefaultUILanguage()
		{
			return 1033;
		}

        private static string GetUserPreferredUILangs(bool filterOutNonConsoleCultures)
        {
            long pulNumLanguages = 0L;
            int pcchLanguagesBuffer = 0;
            string str = "";
            if (!filterOutNonConsoleCultures || SetThreadPreferredUILanguages(MUI_CONSOLE_FILTER, null, IntPtr.Zero))
            {
                if (!GetThreadPreferredUILanguages((MUI_LANGUAGE_NAME | MUI_MERGE_SYSTEM_FALLBACK) | MUI_MERGE_USER_FALLBACK, out pulNumLanguages, null, out pcchLanguagesBuffer))
                {
                    return str;
                }
                byte[] pwszLanguagesBuffer = new byte[pcchLanguagesBuffer * 2];
                if (!GetThreadPreferredUILanguages((MUI_LANGUAGE_NAME | MUI_MERGE_SYSTEM_FALLBACK) | MUI_MERGE_USER_FALLBACK, out pulNumLanguages, pwszLanguagesBuffer, out pcchLanguagesBuffer))
                {
                    return str;
                }
                try
                {
                    return Encoding.Unicode.GetString(pwszLanguagesBuffer).Trim().ToLowerInvariant();
                }
                catch (ArgumentNullException)
                {
                }
                catch (DecoderFallbackException)
                {
                }
            }
            return str;
        }

        private static bool IsVistaAndLater()
        {
            return (Environment.OSVersion.Version.Major >= 6);
        }

		/*
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        private static extern bool SetThreadPreferredUILanguages(int dwFlags, StringBuilder pwszLanguagesBuffer, IntPtr pulNumLanguages);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern short SetThreadUILanguage(short langId);
		*/

		private static bool SetThreadPreferredUILanguages (int dwFlags, StringBuilder pwszLanguagesBuffer, IntPtr pulNumLanguages)
		{
			return true;
		}

		internal static short SetThreadUILanguage (short langId)
		{
			return langId;
		}

        internal static CultureInfo Culture
        {
            get
            {
                if (m_Culture == null)
                {
                    lock (m_syncObject)
                    {
                        if (m_Culture == null)
                        {
                            m_Culture = GetCulture();
                        }
                    }
                }
                return m_Culture;
            }
        }

        internal static CultureInfo UICulture
        {
            get
            {
                if (m_uiCulture == null)
                {
                    lock (m_syncObject)
                    {
                        if (m_uiCulture == null)
                        {
                            m_uiCulture = GetUICulture();
                        }
                    }
                }
                return (CultureInfo) m_uiCulture.Clone();
            }
        }
    }
}

