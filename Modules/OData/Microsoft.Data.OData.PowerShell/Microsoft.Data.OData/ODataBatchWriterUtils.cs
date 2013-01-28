namespace Microsoft.Data.OData
{
    using System;
    using System.Globalization;
    using System.IO;

    internal static class ODataBatchWriterUtils
    {
        internal static string CreateBatchBoundary(bool isResponse)
        {
            string format = isResponse ? "batchresponse_{0}" : "batch_{0}";
            return string.Format(CultureInfo.InvariantCulture, format, new object[] { Guid.NewGuid().ToString() });
        }

        internal static string CreateChangeSetBoundary(bool isResponse)
        {
            string format = isResponse ? "changesetresponse_{0}" : "changeset_{0}";
            return string.Format(CultureInfo.InvariantCulture, format, new object[] { Guid.NewGuid().ToString() });
        }

        internal static string CreateMultipartMixedContentType(string boundary)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}; {1}={2}", new object[] { "multipart/mixed", "boundary", boundary });
        }

        internal static void WriteChangeSetPreamble(TextWriter writer, string changeSetBoundary)
        {
            string str = CreateMultipartMixedContentType(changeSetBoundary);
            writer.WriteLine("{0}: {1}", "Content-Type", str);
            writer.WriteLine();
        }

        internal static void WriteEndBoundary(TextWriter writer, string boundary, bool missingStartBoundary)
        {
            if (!missingStartBoundary)
            {
                writer.WriteLine();
            }
            writer.Write("--{0}--", boundary);
        }

        internal static void WriteRequestPreamble(TextWriter writer, string httpMethod, Uri uri)
        {
            writer.WriteLine("{0}: {1}", "Content-Type", "application/http");
            writer.WriteLine("{0}: {1}", "Content-Transfer-Encoding", "binary");
            writer.WriteLine();
            writer.WriteLine("{0} {1} {2}", httpMethod, UriUtilsCommon.UriToString(uri), "HTTP/1.1");
        }

        internal static void WriteResponsePreamble(TextWriter writer)
        {
            writer.WriteLine("{0}: {1}", "Content-Type", "application/http");
            writer.WriteLine("{0}: {1}", "Content-Transfer-Encoding", "binary");
            writer.WriteLine();
        }

        internal static void WriteStartBoundary(TextWriter writer, string boundary, bool firstBoundary)
        {
            if (!firstBoundary)
            {
                writer.WriteLine();
            }
            writer.WriteLine("--{0}", boundary);
        }
    }
}

