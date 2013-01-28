namespace Microsoft.PowerShell.Commands
{
    using Microsoft.Win32;
    using System;
    using System.Management.Automation;

    internal static class ImportExportCSVHelper
    {
        internal const char CSVDelimiter = ',';
        internal const string CSVTypePrefix = "CSV:";

        internal static char SetDelimiter(PSCmdlet Cmdlet, string ParameterSetName, char Delimiter, bool UseCulture)
        {
            switch (ParameterSetName)
            {
                case "Delimiter":
                    if (Delimiter == '\0')
                    {
                        Delimiter = ',';
                    }
                    return Delimiter;

                case "UseCulture":
                    if (UseCulture)
                    {
                        string str = Registry.CurrentUser.OpenSubKey(@"Control Panel\International").GetValue("sList").ToString();
                        if (string.IsNullOrEmpty(str))
                        {
                            Delimiter = ',';
                            return Delimiter;
                        }
                        Delimiter = str[0];
                    }
                    return Delimiter;
            }
            Delimiter = ',';
            return Delimiter;
        }
    }
}

