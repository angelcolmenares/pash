namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Net;
    using System.Text;

    internal static class StreamHelper
    {
        private const int activityId = 0xa681412;
        internal const int ChunkSize = 0x2710;
        internal const int DefaultReadBuffer = 0x186a0;

        internal static string DecodeStream(MemoryStream stream, string characterSet)
        {
            Encoding encoding = ContentHelper.GetEncoding(characterSet);
            return DecodeStream(stream, encoding);
        }

        internal static string DecodeStream(MemoryStream stream, Encoding encoding)
        {
            if (encoding == null)
            {
                encoding = ContentHelper.GetDefaultEncoding();
            }
            byte[] bytes = stream.ToArray();
            return encoding.GetString(bytes);
        }

        internal static byte[] EncodeToBytes(string str)
        {
            return EncodeToBytes(str, null);
        }

        internal static byte[] EncodeToBytes(string str, Encoding encoding)
        {
            if (encoding == null)
            {
                encoding = ContentHelper.GetDefaultEncoding();
            }
            return encoding.GetBytes(str);
        }

        internal static Stream GetResponseStream(WebResponse response)
        {
            Stream responseStream = response.GetResponseStream();
            string str = response.Headers["Content-Encoding"];
            if (str != null)
            {
                if (str.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return new GZipStream(responseStream, CompressionMode.Decompress);
                }
                if (str.IndexOf("deflate", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                }
            }
            return responseStream;
        }

        internal static MemoryStream ReadStream(Stream stream, long contentLength, PSCmdlet cmdlet)
        {
            MemoryStream stream3;
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanRead)
            {
                throw new ArgumentOutOfRangeException("stream");
            }
            if (0L >= contentLength)
            {
                contentLength = 0x186a0L;
            }
            int capacity = (int) Math.Min(contentLength, 0x7fffffffL);
            MemoryStream stream2 = new MemoryStream(capacity);
            try
            {
                long o = 0L;
                byte[] buffer = new byte[0x2710];
                int count = 1;
                while (0 < count)
                {
                    if (cmdlet != null)
                    {
                        ProgressRecord progressRecord = new ProgressRecord(0xa681412, WebCmdletStrings.ReadResponseProgressActivity, StringUtil.Format(WebCmdletStrings.ReadResponseProgressStatus, o));
                        cmdlet.WriteProgress(progressRecord);
                    }
                    count = stream.Read(buffer, 0, buffer.Length);
                    if (0 < count)
                    {
                        stream2.Write(buffer, 0, count);
                    }
                    o += count;
                }
                if (cmdlet != null)
                {
                    ProgressRecord record2 = new ProgressRecord(0xa681412, WebCmdletStrings.ReadResponseProgressActivity, StringUtil.Format(WebCmdletStrings.ReadResponseComplete, o)) {
                        RecordType = ProgressRecordType.Completed
                    };
                    cmdlet.WriteProgress(record2);
                }
                stream2.SetLength(o);
                stream3 = stream2;
            }
            catch (Exception)
            {
                stream2.Close();
                throw;
            }
            return stream3;
        }

        internal static void SaveStreamToFile(Stream stream, string filePath, PSCmdlet cmdlet)
        {
            using (FileStream stream2 = System.IO.File.Create(filePath))
            {
                long position = stream.Position;
                stream.Position = 0L;
                WriteToStream(stream, stream2, cmdlet);
                stream.Position = position;
            }
        }

        internal static void WriteToStream(byte[] input, Stream output)
        {
            output.Write(input, 0, input.Length);
            output.Flush();
        }

        internal static void WriteToStream(Stream input, Stream output, PSCmdlet cmdlet)
        {
            byte[] buffer = new byte[0x2710];
            long length = input.Length;
            int count = 1;
            while (0 < count)
            {
                if (cmdlet != null)
                {
                    ProgressRecord progressRecord = new ProgressRecord(0xa681412, WebCmdletStrings.WriteRequestProgressActivity, StringUtil.Format(WebCmdletStrings.WriteRequestProgressStatus, length));
                    cmdlet.WriteProgress(progressRecord);
                }
                int num3 = (int) Math.Min(length, 0x2710L);
                count = input.Read(buffer, 0, num3);
                if (0 < count)
                {
                    output.Write(buffer, 0, count);
                }
                length -= count;
            }
            if (cmdlet != null)
            {
                ProgressRecord record2 = new ProgressRecord(0xa681412, WebCmdletStrings.WriteRequestProgressActivity, StringUtil.Format(WebCmdletStrings.WriteRequestComplete, length)) {
                    RecordType = ProgressRecordType.Completed
                };
                cmdlet.WriteProgress(record2);
            }
            output.Flush();
        }
    }
}

