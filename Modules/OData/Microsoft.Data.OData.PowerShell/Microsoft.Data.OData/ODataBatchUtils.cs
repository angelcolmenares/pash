namespace Microsoft.Data.OData
{
    using System;
    using System.Globalization;
    using System.IO;

    internal static class ODataBatchUtils
    {
        internal static ODataBatchOperationReadStream CreateBatchOperationReadStream(ODataBatchReaderStream batchReaderStream, ODataBatchOperationHeaders headers, IODataBatchOperationListener operationListener)
        {
            string str;
            if (!headers.TryGetValue("Content-Length", out str))
            {
                return ODataBatchOperationReadStream.Create(batchReaderStream, operationListener);
            }
            int length = int.Parse(str, CultureInfo.InvariantCulture);
            if (length < 0)
            {
                throw new ODataException(Strings.ODataBatchReaderStream_InvalidContentLengthSpecified(str));
            }
            return ODataBatchOperationReadStream.Create(batchReaderStream, operationListener, length);
        }

        internal static ODataBatchOperationWriteStream CreateBatchOperationWriteStream(Stream outputStream, IODataBatchOperationListener operationListener)
        {
            return new ODataBatchOperationWriteStream(outputStream, operationListener);
        }

        internal static Uri CreateOperationRequestUri(Uri uri, Uri baseUri, IODataUrlResolver urlResolver)
        {
            if (urlResolver != null)
            {
                Uri uri2 = urlResolver.ResolveUrl(baseUri, uri);
                if (uri2 != null)
                {
                    return uri2;
                }
            }
            if (uri.IsAbsoluteUri)
            {
                return uri;
            }
            if (baseUri == null)
            {
                string message = uri.OriginalString.StartsWith("$", StringComparison.Ordinal) ? Strings.ODataBatchUtils_RelativeUriStartingWithDollarUsedWithoutBaseUriSpecified(UriUtilsCommon.UriToString(uri)) : Strings.ODataBatchUtils_RelativeUriUsedWithoutBaseUriSpecified(UriUtilsCommon.UriToString(uri));
                throw new ODataException(message);
            }
            return UriUtils.UriToAbsoluteUri(baseUri, uri);
        }

        internal static void EnsureArraySize(ref byte[] buffer, int numberOfBytesInBuffer, int requiredByteCount)
        {
            int num = buffer.Length - numberOfBytesInBuffer;
            if (requiredByteCount > num)
            {
                int num2 = requiredByteCount - num;
                byte[] src = buffer;
                buffer = new byte[buffer.Length + num2];
                Buffer.BlockCopy(src, 0, buffer, 0, numberOfBytesInBuffer);
            }
        }
    }
}

