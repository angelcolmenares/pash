namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class EncodingConversion
    {
        internal const string Ascii = "ascii";
        internal const string BigEndianUnicode = "bigendianunicode";
        internal const string Default = "default";
        internal const string OEM = "oem";
        internal const string String = "string";
        internal const string Unicode = "unicode";
        internal const string Unknown = "unknown";
        internal const string Utf32 = "utf32";
        internal const string Utf7 = "utf7";
        internal const string Utf8 = "utf8";

        internal static Encoding Convert(Cmdlet cmdlet, string encoding)
        {
            if ((encoding == null) || (encoding.Length == 0))
            {
                return Encoding.Unicode;
            }
            if (string.Equals(encoding, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.Unicode;
            }
            if (string.Equals(encoding, "string", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.Unicode;
            }
            if (string.Equals(encoding, "unicode", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.Unicode;
            }
            if (string.Equals(encoding, "bigendianunicode", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.BigEndianUnicode;
            }
            if (string.Equals(encoding, "ascii", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.ASCII;
            }
            if (string.Equals(encoding, "utf8", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.UTF8;
            }
            if (string.Equals(encoding, "utf7", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.UTF7;
            }
            if (string.Equals(encoding, "utf32", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.UTF32;
            }
            if (string.Equals(encoding, "default", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.Default;
            }
            if (string.Equals(encoding, "oem", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.GetEncoding((int) NativeMethods.GetOEMCP());
            }
            string str = string.Join(", ", new string[] { "unknown", "string", "unicode", "bigendianunicode", "ascii", "utf8", "utf7", "utf32", "default", "oem" });
            string message = StringUtil.Format(PathUtilsStrings.OutFile_WriteToFileEncodingUnknown, encoding, str);
            ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewArgumentException("Encoding"), "WriteToFileEncodingUnknown", ErrorCategory.InvalidArgument, null) {
                ErrorDetails = new ErrorDetails(message)
            };
            cmdlet.ThrowTerminatingError(errorRecord);
            return null;
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
            internal static extern int GetOEMCP();
        }
    }
}

