namespace Microsoft.Data.OData
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal sealed class ODataBatchReaderStream
    {
        private readonly string batchBoundary;
        private readonly ODataBatchReaderStreamBuffer batchBuffer;
        private Encoding batchEncoding;
        private string changesetBoundary;
        private Encoding changesetEncoding;
        private readonly ODataRawInputContext inputContext;
        private readonly byte[] lineBuffer;
        private const int LineBufferLength = 0x7d0;
        private bool underlyingStreamExhausted;

        internal ODataBatchReaderStream(ODataRawInputContext inputContext, string batchBoundary, Encoding batchEncoding)
        {
            this.inputContext = inputContext;
            this.batchBoundary = batchBoundary;
            this.batchEncoding = batchEncoding;
            this.batchBuffer = new ODataBatchReaderStreamBuffer();
            this.lineBuffer = new byte[0x7d0];
        }

        private Encoding DetectEncoding()
        {
            while (!this.underlyingStreamExhausted && (this.batchBuffer.NumberOfBytesInBuffer < 4))
            {
                this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, this.batchBuffer.CurrentReadPosition);
            }
            int numberOfBytesInBuffer = this.batchBuffer.NumberOfBytesInBuffer;
            if (numberOfBytesInBuffer >= 2)
            {
                if ((this.batchBuffer[this.batchBuffer.CurrentReadPosition] == 0xfe) && (this.batchBuffer[this.batchBuffer.CurrentReadPosition + 1] == 0xff))
                {
                    return new UnicodeEncoding(true, true);
                }
                if ((this.batchBuffer[this.batchBuffer.CurrentReadPosition] == 0xff) && (this.batchBuffer[this.batchBuffer.CurrentReadPosition + 1] == 0xfe))
                {
                    if (((numberOfBytesInBuffer >= 4) && (this.batchBuffer[this.batchBuffer.CurrentReadPosition + 2] == 0)) && (this.batchBuffer[this.batchBuffer.CurrentReadPosition + 3] == 0))
                    {
                        return new UTF32Encoding(false, true);
                    }
                    return new UnicodeEncoding(false, true);
                }
                if (((numberOfBytesInBuffer >= 3) && (this.batchBuffer[this.batchBuffer.CurrentReadPosition] == 0xef)) && ((this.batchBuffer[this.batchBuffer.CurrentReadPosition + 1] == 0xbb) && (this.batchBuffer[this.batchBuffer.CurrentReadPosition + 2] == 0xbf)))
                {
                    return Encoding.UTF8;
                }
                if ((((numberOfBytesInBuffer >= 4) && (this.batchBuffer[this.batchBuffer.CurrentReadPosition] == 0)) && ((this.batchBuffer[this.batchBuffer.CurrentReadPosition + 1] == 0) && (this.batchBuffer[this.batchBuffer.CurrentReadPosition + 2] == 0xfe))) && (this.batchBuffer[this.batchBuffer.CurrentReadPosition + 3] == 0xff))
                {
                    return new UTF32Encoding(true, true);
                }
            }
            return Encoding.ASCII;
        }

        private void DetermineChangesetBoundaryAndEncoding(string contentType)
        {
            MediaType type;
            ODataPayloadKind kind;
            MediaTypeUtils.GetFormatFromContentType(contentType, new ODataPayloadKind[] { ODataPayloadKind.Batch }, MediaTypeResolver.DefaultMediaTypeResolver, out type, out this.changesetEncoding, out kind, out this.changesetBoundary);
        }

        private void EnsureBatchEncoding()
        {
            if (this.batchEncoding == null)
            {
                this.batchEncoding = this.DetectEncoding();
            }
            ReaderValidationUtils.ValidateEncodingSupportedInBatch(this.batchEncoding);
        }

        internal bool ProcessPartHeader()
        {
            bool flag;
            ODataBatchOperationHeaders headers = this.ReadPartHeaders(out flag);
            if (flag)
            {
                this.DetermineChangesetBoundaryAndEncoding(headers["Content-Type"]);
                if (this.changesetEncoding == null)
                {
                    this.changesetEncoding = this.DetectEncoding();
                }
                ReaderValidationUtils.ValidateEncodingSupportedInBatch(this.changesetEncoding);
            }
            return flag;
        }

        internal ODataBatchOperationHeaders ReadHeaders()
        {
            ODataBatchOperationHeaders headers = new ODataBatchOperationHeaders();
            for (string str = this.ReadLine(); (str != null) && (str.Length > 0); str = this.ReadLine())
            {
                string str2;
                string str3;
                ValidateHeaderLine(str, out str2, out str3);
                if (headers.ContainsKeyOrdinal(str2))
                {
                    throw new ODataException(Strings.ODataBatchReaderStream_DuplicateHeaderFound(str2));
                }
                headers.Add(str2, str3);
            }
            return headers;
        }

        internal string ReadLine()
        {
            int numberOfBytesInBuffer = 0;
            byte[] lineBuffer = this.lineBuffer;
            ODataBatchReaderStreamScanResult noMatch = ODataBatchReaderStreamScanResult.NoMatch;
            while (noMatch != ODataBatchReaderStreamScanResult.Match)
            {
                int num2;
                int num3;
                int num4;
                noMatch = this.batchBuffer.ScanForLineEnd(out num3, out num4);
                switch (noMatch)
                {
                    case ODataBatchReaderStreamScanResult.NoMatch:
                    {
                        num2 = this.batchBuffer.NumberOfBytesInBuffer;
                        if (num2 > 0)
                        {
                            ODataBatchUtils.EnsureArraySize(ref lineBuffer, numberOfBytesInBuffer, num2);
                            Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, lineBuffer, numberOfBytesInBuffer, num2);
                            numberOfBytesInBuffer += num2;
                        }
                        if (this.underlyingStreamExhausted)
                        {
                            noMatch = ODataBatchReaderStreamScanResult.Match;
                            this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + num2);
                        }
                        else
                        {
                            this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, 0x1f40);
                        }
                        continue;
                    }
                    case ODataBatchReaderStreamScanResult.PartialMatch:
                    {
                        num2 = num3 - this.batchBuffer.CurrentReadPosition;
                        if (num2 > 0)
                        {
                            ODataBatchUtils.EnsureArraySize(ref lineBuffer, numberOfBytesInBuffer, num2);
                            Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, lineBuffer, numberOfBytesInBuffer, num2);
                            numberOfBytesInBuffer += num2;
                        }
                        if (this.underlyingStreamExhausted)
                        {
                            noMatch = ODataBatchReaderStreamScanResult.Match;
                            this.batchBuffer.SkipTo(num3 + 1);
                        }
                        else
                        {
                            this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, num3);
                        }
                        continue;
                    }
                    case ODataBatchReaderStreamScanResult.Match:
                    {
                        num2 = num3 - this.batchBuffer.CurrentReadPosition;
                        if (num2 > 0)
                        {
                            ODataBatchUtils.EnsureArraySize(ref lineBuffer, numberOfBytesInBuffer, num2);
                            Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, lineBuffer, numberOfBytesInBuffer, num2);
                            numberOfBytesInBuffer += num2;
                        }
                        this.batchBuffer.SkipTo(num4 + 1);
                        continue;
                    }
                }
                throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStream_ReadLine));
            }
            if (lineBuffer == null)
            {
                return string.Empty;
            }
            return this.CurrentEncoding.GetString(lineBuffer, 0, numberOfBytesInBuffer);
        }

        private ODataBatchOperationHeaders ReadPartHeaders(out bool isChangeSetPart)
        {
            ODataBatchOperationHeaders headers = this.ReadHeaders();
            return this.ValidatePartHeaders(headers, out isChangeSetPart);
        }

        internal int ReadWithDelimiter(byte[] userBuffer, int userBufferOffset, int count)
        {
            if (count == 0)
            {
                return 0;
            }
            int maxDataBytesToScan = count;
            ODataBatchReaderStreamScanResult noMatch = ODataBatchReaderStreamScanResult.NoMatch;
            while ((maxDataBytesToScan > 0) && (noMatch != ODataBatchReaderStreamScanResult.Match))
            {
                int num2;
                int num3;
                bool flag;
                bool flag2;
                int num4;
                switch (this.batchBuffer.ScanForBoundary(this.CurrentBoundaries, maxDataBytesToScan, out num2, out num3, out flag, out flag2))
                {
                    case ODataBatchReaderStreamScanResult.NoMatch:
                        if (this.batchBuffer.NumberOfBytesInBuffer < maxDataBytesToScan)
                        {
                            break;
                        }
                        Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, maxDataBytesToScan);
                        this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + maxDataBytesToScan);
                        return count;

                    case ODataBatchReaderStreamScanResult.PartialMatch:
                    {
                        if (!this.underlyingStreamExhausted)
                        {
                            goto Label_0168;
                        }
                        int num6 = Math.Min(this.batchBuffer.NumberOfBytesInBuffer, maxDataBytesToScan);
                        Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, num6);
                        this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + num6);
                        maxDataBytesToScan -= num6;
                        return (count - maxDataBytesToScan);
                    }
                    case ODataBatchReaderStreamScanResult.Match:
                        num4 = num2 - this.batchBuffer.CurrentReadPosition;
                        Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, num4);
                        maxDataBytesToScan -= num4;
                        userBufferOffset += num4;
                        this.batchBuffer.SkipTo(num2);
                        return (count - maxDataBytesToScan);

                    default:
                    {
                        continue;
                    }
                }
                int numberOfBytesInBuffer = this.batchBuffer.NumberOfBytesInBuffer;
                Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, numberOfBytesInBuffer);
                maxDataBytesToScan -= numberOfBytesInBuffer;
                userBufferOffset += numberOfBytesInBuffer;
                if (this.underlyingStreamExhausted)
                {
                    this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + numberOfBytesInBuffer);
                    return (count - maxDataBytesToScan);
                }
                this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, 0x1f40);
                continue;
            Label_0168:
                num4 = num2 - this.batchBuffer.CurrentReadPosition;
                Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, num4);
                maxDataBytesToScan -= num4;
                userBufferOffset += num4;
                this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, num2);
            }
            throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStream_ReadWithDelimiter));
        }

        internal int ReadWithLength(byte[] userBuffer, int userBufferOffset, int count)
        {
            int num = count;
            while (num > 0)
            {
                if (this.batchBuffer.NumberOfBytesInBuffer >= num)
                {
                    Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, num);
                    this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + num);
                    num = 0;
                }
                else
                {
                    int numberOfBytesInBuffer = this.batchBuffer.NumberOfBytesInBuffer;
                    Buffer.BlockCopy(this.batchBuffer.Bytes, this.batchBuffer.CurrentReadPosition, userBuffer, userBufferOffset, numberOfBytesInBuffer);
                    num -= numberOfBytesInBuffer;
                    userBufferOffset += numberOfBytesInBuffer;
                    if (this.underlyingStreamExhausted)
                    {
                        throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStreamBuffer_ReadWithLength));
                    }
                    this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, 0x1f40);
                }
            }
            return (count - num);
        }

        internal void ResetChangeSetBoundary()
        {
            this.changesetBoundary = null;
            this.changesetEncoding = null;
        }

        internal bool SkipToBoundary(out bool isEndBoundary, out bool isParentBoundary)
        {
            this.EnsureBatchEncoding();
            ODataBatchReaderStreamScanResult noMatch = ODataBatchReaderStreamScanResult.NoMatch;
            while (noMatch != ODataBatchReaderStreamScanResult.Match)
            {
                int num;
                int num2;
                switch (this.batchBuffer.ScanForBoundary(this.CurrentBoundaries, 0x7fffffff, out num, out num2, out isEndBoundary, out isParentBoundary))
                {
                    case ODataBatchReaderStreamScanResult.NoMatch:
                        if (!this.underlyingStreamExhausted)
                        {
                            break;
                        }
                        this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + this.batchBuffer.NumberOfBytesInBuffer);
                        return false;

                    case ODataBatchReaderStreamScanResult.PartialMatch:
                        if (!this.underlyingStreamExhausted)
                        {
                            goto Label_00BE;
                        }
                        this.batchBuffer.SkipTo(this.batchBuffer.CurrentReadPosition + this.batchBuffer.NumberOfBytesInBuffer);
                        return false;

                    case ODataBatchReaderStreamScanResult.Match:
                        this.batchBuffer.SkipTo(isParentBoundary ? num : (num2 + 1));
                        return true;

                    default:
                        throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStream_SkipToBoundary));
                }
                this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, 0x1f40);
                continue;
            Label_00BE:
                this.underlyingStreamExhausted = this.batchBuffer.RefillFrom(this.inputContext.Stream, num);
            }
            throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStream_SkipToBoundary));
        }

        private static void ValidateHeaderLine(string headerLine, out string headerName, out string headerValue)
        {
            int index = headerLine.IndexOf(':');
            if (index <= 0)
            {
                throw new ODataException(Strings.ODataBatchReaderStream_InvalidHeaderSpecified(headerLine));
            }
            headerName = headerLine.Substring(0, index).Trim();
            headerValue = headerLine.Substring(index + 1).Trim();
        }

        private ODataBatchOperationHeaders ValidatePartHeaders(ODataBatchOperationHeaders headers, out bool isChangeSetPart)
        {
            string str;
            if (!headers.TryGetValue("Content-Type", out str))
            {
                throw new ODataException(Strings.ODataBatchReaderStream_MissingContentTypeHeader);
            }
            if (MediaTypeUtils.MediaTypeAndSubtypeAreEqual(str, "application/http"))
            {
                string str2;
                isChangeSetPart = false;
                if (!headers.TryGetValue("Content-Transfer-Encoding", out str2) || (string.Compare(str2, "binary", StringComparison.OrdinalIgnoreCase) != 0))
                {
                    throw new ODataException(Strings.ODataBatchReaderStream_MissingOrInvalidContentEncodingHeader("Content-Transfer-Encoding", "binary"));
                }
                return headers;
            }
            if (!MediaTypeUtils.MediaTypeStartsWithTypeAndSubtype(str, "multipart/mixed"))
            {
                throw new ODataException(Strings.ODataBatchReaderStream_InvalidContentTypeSpecified("Content-Type", str, "multipart/mixed", "application/http"));
            }
            isChangeSetPart = true;
            if (this.changesetBoundary != null)
            {
                throw new ODataException(Strings.ODataBatchReaderStream_NestedChangesetsAreNotSupported);
            }
            return headers;
        }

        internal string BatchBoundary
        {
            get
            {
                return this.batchBoundary;
            }
        }

        internal string ChangeSetBoundary
        {
            get
            {
                return this.changesetBoundary;
            }
        }

        private IEnumerable<string> CurrentBoundaries
        {
            get
            {
                if (this.changesetBoundary != null)
                {
                    yield return this.changesetBoundary;
                }
                yield return this.batchBoundary;
            }
        }

        private Encoding CurrentEncoding
        {
            get
            {
                return (this.changesetEncoding ?? this.batchEncoding);
            }
        }

    }
}

