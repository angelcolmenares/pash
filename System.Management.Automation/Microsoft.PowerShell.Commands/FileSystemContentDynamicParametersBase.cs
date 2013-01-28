namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public class FileSystemContentDynamicParametersBase
    {
        private FileSystemCmdletProviderEncoding streamType = FileSystemCmdletProviderEncoding.String;

        private static System.Text.Encoding GetEncodingFromEnum(FileSystemCmdletProviderEncoding type)
        {
            System.Text.Encoding unicode = System.Text.Encoding.Unicode;
            switch (type)
            {
                case FileSystemCmdletProviderEncoding.String:
                    return System.Text.Encoding.Unicode;

                case FileSystemCmdletProviderEncoding.Unicode:
                    return System.Text.Encoding.Unicode;

                case FileSystemCmdletProviderEncoding.BigEndianUnicode:
                    return System.Text.Encoding.BigEndianUnicode;

                case FileSystemCmdletProviderEncoding.UTF8:
                    return System.Text.Encoding.UTF8;

                case FileSystemCmdletProviderEncoding.UTF7:
                    return System.Text.Encoding.UTF7;

                case FileSystemCmdletProviderEncoding.UTF32:
                    return System.Text.Encoding.UTF32;

                case FileSystemCmdletProviderEncoding.Ascii:
                    return System.Text.Encoding.ASCII;

                case FileSystemCmdletProviderEncoding.Default:
                    return System.Text.Encoding.Default;

                case FileSystemCmdletProviderEncoding.Oem:
                    return System.Text.Encoding.GetEncoding((int) NativeMethods.GetOEMCP());
            }
            return System.Text.Encoding.Unicode;
        }

        [Parameter]
        public FileSystemCmdletProviderEncoding Encoding
        {
            get
            {
                return this.streamType;
            }
            set
            {
                this.streamType = value;
            }
        }

        public System.Text.Encoding EncodingType
        {
            get
            {
                return GetEncodingFromEnum(this.streamType);
            }
        }

        [Parameter]
        public string Stream { get; set; }

        public bool UsingByteEncoding
        {
            get
            {
                return (this.streamType == FileSystemCmdletProviderEncoding.Byte);
            }
        }

        public bool WasStreamTypeSpecified
        {
            get
            {
                return (this.streamType != FileSystemCmdletProviderEncoding.String);
            }
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
            internal static extern int GetOEMCP();
        }
    }
}

