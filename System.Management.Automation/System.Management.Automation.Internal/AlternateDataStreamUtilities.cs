namespace System.Management.Automation.Internal
{
    using Microsoft.PowerShell.Commands;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    public static class AlternateDataStreamUtilities
    {
        internal static FileStream CreateFileStream(string path, string streamName, FileMode mode, FileAccess access, FileShare share)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (streamName == null)
            {
                throw new ArgumentNullException("streamName");
            }
            string str = streamName.Trim();
            str = ":" + str;
            string lpFileName = path + str;
            if (mode == FileMode.Append)
            {
                mode = FileMode.OpenOrCreate;
            }
            SafeFileHandle handle = NativeMethods.CreateFile(lpFileName, access, share, IntPtr.Zero, mode, 0, IntPtr.Zero);
            if (handle.IsInvalid)
            {
                throw new FileNotFoundException(StringUtil.Format(FileSystemProviderStrings.AlternateDataStreamNotFound, streamName, path), lpFileName);
            }
            return new FileStream(handle, access);
        }

        internal static void DeleteFileStream(string path, string streamName)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (streamName == null)
            {
                throw new ArgumentNullException("streamName");
            }
            string str = streamName.Trim();
            if (str.IndexOf(':') != 0)
            {
                str = ":" + str;
            }
            NativeMethods.DeleteFile(path + str);
        }

        internal static List<AlternateStreamData> GetStreams(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            List<AlternateStreamData> list = new List<AlternateStreamData>();
            AlternateStreamNativeData lpFindStreamData = new AlternateStreamNativeData();
            SafeFindHandle hndFindFile = NativeMethods.FindFirstStreamW(path, NativeMethods.StreamInfoLevels.FindStreamInfoStandard, lpFindStreamData, 0);
            if (hndFindFile.IsInvalid)
            {
                throw new Win32Exception();
            }
            try
            {
                do
                {
                    AlternateStreamData data2;
                    lpFindStreamData.Name = lpFindStreamData.Name.Substring(1);
                    string b = ":$DATA";
                    if (!string.Equals(lpFindStreamData.Name, b, StringComparison.OrdinalIgnoreCase))
                    {
                        lpFindStreamData.Name = lpFindStreamData.Name.Replace(b, "");
                    }
                    data2 = new AlternateStreamData {
                        Stream = lpFindStreamData.Name,
                        Length = lpFindStreamData.Length,
                    };
                    data2.FileName = path.Replace(data2.Stream, "");
                    data2.FileName = data2.FileName.Trim(new char[] { ':' });
                    list.Add(data2);
                    lpFindStreamData = new AlternateStreamNativeData();
                }
                while (NativeMethods.FindNextStreamW(hndFindFile, lpFindStreamData));
                int error = Marshal.GetLastWin32Error();
                if (error != 0x26)
                {
                    throw new Win32Exception(error);
                }
            }
            finally
            {
                hndFindFile.Dispose();
            }
            return list;
        }

        internal static void SetZoneOfOrigin(string path, SecurityZone securityZone)
        {
            using (FileStream stream = CreateFileStream(path, "Zone.Identifier", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (TextWriter writer = new StreamWriter(stream, Encoding.Unicode))
                {
                    writer.WriteLine("[ZoneTransfer]");
                    writer.WriteLine("ZoneId={0}", (int) securityZone);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal class AlternateStreamNativeData
        {
            public long Length;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x128)]
            public string Name;
        }

        private static class NativeMethods
        {
            internal const int ERROR_HANDLE_EOF = 0x26;

            [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
            internal static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
            internal static extern bool DeleteFile(string lpFileName);
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern AlternateDataStreamUtilities.SafeFindHandle FindFirstStreamW(string lpFileName, StreamInfoLevels InfoLevel, [In, Out, MarshalAs(UnmanagedType.LPStruct)] AlternateDataStreamUtilities.AlternateStreamNativeData lpFindStreamData, int dwFlags);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern bool FindNextStreamW(AlternateDataStreamUtilities.SafeFindHandle hndFindFile, [In, Out, MarshalAs(UnmanagedType.LPStruct)] AlternateDataStreamUtilities.AlternateStreamNativeData lpFindStreamData);

            internal enum StreamInfoLevels
            {
                FindStreamInfoStandard
            }
        }

        internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeFindHandle() : base(true)
            {
            }

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32.dll")]
            private static extern bool FindClose(IntPtr handle);
            protected override bool ReleaseHandle()
            {
                return FindClose(base.handle);
            }
        }
    }
}

