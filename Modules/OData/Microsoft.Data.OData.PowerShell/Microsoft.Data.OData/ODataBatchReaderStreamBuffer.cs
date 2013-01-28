namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ODataBatchReaderStreamBuffer
    {
        internal const int BufferLength = 0x1f40;
        private readonly byte[] bytes = new byte[0x1f40];
        private int currentReadPosition;
        private const int MaxLineFeedLength = 2;
        private int numberOfBytesInBuffer;
        private const int TwoDashesLength = 2;

        private bool MatchBoundary(string boundary, int startIx, int matchLength, out bool isEndBoundary)
        {
            isEndBoundary = false;
            if (matchLength != 0)
            {
                int num = 0;
                int index = startIx;
                for (int i = -2; i < (matchLength - 2); i++)
                {
                    if (i < 0)
                    {
                        if (this.bytes[index] != 0x2d)
                        {
                            return false;
                        }
                    }
                    else if (i < boundary.Length)
                    {
                        if (this.bytes[index] != boundary[i])
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (this.bytes[index] != 0x2d)
                        {
                            return true;
                        }
                        num++;
                    }
                    index++;
                }
                isEndBoundary = num == 2;
            }
            return true;
        }

        private ODataBatchReaderStreamScanResult MatchBoundary(int lineEndStartPosition, int boundaryDelimiterStartPosition, string boundary, out int boundaryStartPosition, out int boundaryEndPosition, out bool isEndBoundary)
        {
            bool flag;
            int num3;
            boundaryStartPosition = -1;
            boundaryEndPosition = -1;
            int num = (this.currentReadPosition + this.numberOfBytesInBuffer) - 1;
            int num2 = (((boundaryDelimiterStartPosition + 2) + boundary.Length) + 2) - 1;
            if (num < (num2 + 2))
            {
                flag = true;
                num3 = (Math.Min(num, num2) - boundaryDelimiterStartPosition) + 1;
            }
            else
            {
                flag = false;
                num3 = (num2 - boundaryDelimiterStartPosition) + 1;
            }
            if (this.MatchBoundary(boundary, boundaryDelimiterStartPosition, num3, out isEndBoundary))
            {
                int num4;
                int num5;
                bool flag2;
                boundaryStartPosition = (lineEndStartPosition < 0) ? boundaryDelimiterStartPosition : lineEndStartPosition;
                if (flag)
                {
                    isEndBoundary = false;
                    return ODataBatchReaderStreamScanResult.PartialMatch;
                }
                boundaryEndPosition = ((boundaryDelimiterStartPosition + 2) + boundary.Length) - 1;
                if (isEndBoundary)
                {
                    boundaryEndPosition += 2;
                }
                switch (this.ScanForLineEnd(boundaryEndPosition + 1, 0x7fffffff, true, out num4, out num5, out flag2))
                {
                    case ODataBatchReaderStreamScanResult.NoMatch:
                        if (flag2)
                        {
                            if (boundaryStartPosition == 0)
                            {
                                throw new ODataException(Strings.ODataBatchReaderStreamBuffer_BoundaryLineSecurityLimitReached(0x1f40));
                            }
                            isEndBoundary = false;
                            return ODataBatchReaderStreamScanResult.PartialMatch;
                        }
                        break;

                    case ODataBatchReaderStreamScanResult.PartialMatch:
                        if (boundaryStartPosition == 0)
                        {
                            throw new ODataException(Strings.ODataBatchReaderStreamBuffer_BoundaryLineSecurityLimitReached(0x1f40));
                        }
                        isEndBoundary = false;
                        return ODataBatchReaderStreamScanResult.PartialMatch;

                    case ODataBatchReaderStreamScanResult.Match:
                        boundaryEndPosition = num5;
                        return ODataBatchReaderStreamScanResult.Match;

                    default:
                        throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStreamBuffer_ScanForBoundary));
                }
            }
            return ODataBatchReaderStreamScanResult.NoMatch;
        }

        internal bool RefillFrom(Stream stream, int preserveFrom)
        {
            this.ShiftToBeginning(preserveFrom);
            int count = 0x1f40 - this.numberOfBytesInBuffer;
            int num2 = stream.Read(this.bytes, this.numberOfBytesInBuffer, count);
            this.numberOfBytesInBuffer += num2;
            return (num2 == 0);
        }

        internal ODataBatchReaderStreamScanResult ScanForBoundary(IEnumerable<string> boundaries, int maxDataBytesToScan, out int boundaryStartPosition, out int boundaryEndPosition, out bool isEndBoundary, out bool isParentBoundary)
        {
            int num2;
            int num3;
            boundaryStartPosition = -1;
            boundaryEndPosition = -1;
            isEndBoundary = false;
            isParentBoundary = false;
            int currentReadPosition = this.currentReadPosition;
        Label_0016:
            switch (this.ScanForBoundaryStart(currentReadPosition, maxDataBytesToScan, out num2, out num3))
            {
                case ODataBatchReaderStreamScanResult.NoMatch:
                    return ODataBatchReaderStreamScanResult.NoMatch;

                case ODataBatchReaderStreamScanResult.PartialMatch:
                    boundaryStartPosition = (num2 < 0) ? num3 : num2;
                    return ODataBatchReaderStreamScanResult.PartialMatch;

                case ODataBatchReaderStreamScanResult.Match:
                    isParentBoundary = false;
                    foreach (string str in boundaries)
                    {
                        switch (this.MatchBoundary(num2, num3, str, out boundaryStartPosition, out boundaryEndPosition, out isEndBoundary))
                        {
                            case ODataBatchReaderStreamScanResult.NoMatch:
                                boundaryStartPosition = -1;
                                boundaryEndPosition = -1;
                                isEndBoundary = false;
                                isParentBoundary = true;
                                break;

                            case ODataBatchReaderStreamScanResult.PartialMatch:
                                boundaryEndPosition = -1;
                                isEndBoundary = false;
                                return ODataBatchReaderStreamScanResult.PartialMatch;

                            case ODataBatchReaderStreamScanResult.Match:
                                return ODataBatchReaderStreamScanResult.Match;

                            default:
                                throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStreamBuffer_ScanForBoundary));
                        }
                    }
                    currentReadPosition = (currentReadPosition == num3) ? (num3 + 1) : num3;
                    goto Label_0016;
            }
            throw new ODataException(Strings.General_InternalError(InternalErrorCodes.ODataBatchReaderStreamBuffer_ScanForBoundary));
        }

        private ODataBatchReaderStreamScanResult ScanForBoundaryStart(int scanStartIx, int maxDataBytesToScan, out int lineEndStartPosition, out int boundaryDelimiterStartPosition)
        {
            int num = (this.currentReadPosition + Math.Min(maxDataBytesToScan, this.numberOfBytesInBuffer)) - 1;
            for (int i = scanStartIx; i <= num; i++)
            {
                char ch = (char) this.bytes[i];
                switch (ch)
                {
                    case '\r':
                    case '\n':
                        lineEndStartPosition = i;
                        if (((ch == '\r') && (i == num)) && (maxDataBytesToScan >= this.numberOfBytesInBuffer))
                        {
                            boundaryDelimiterStartPosition = i;
                            return ODataBatchReaderStreamScanResult.PartialMatch;
                        }
                        boundaryDelimiterStartPosition = ((ch == '\r') && (this.bytes[i + 1] == 10)) ? (i + 2) : (i + 1);
                        return ODataBatchReaderStreamScanResult.Match;
                }
                if (ch == '-')
                {
                    lineEndStartPosition = -1;
                    if ((i == num) && (maxDataBytesToScan >= this.numberOfBytesInBuffer))
                    {
                        boundaryDelimiterStartPosition = i;
                        return ODataBatchReaderStreamScanResult.PartialMatch;
                    }
                    if (this.bytes[i + 1] == 0x2d)
                    {
                        boundaryDelimiterStartPosition = i;
                        return ODataBatchReaderStreamScanResult.Match;
                    }
                }
            }
            lineEndStartPosition = -1;
            boundaryDelimiterStartPosition = -1;
            return ODataBatchReaderStreamScanResult.NoMatch;
        }

        internal ODataBatchReaderStreamScanResult ScanForLineEnd(out int lineEndStartPosition, out int lineEndEndPosition)
        {
            bool flag;
            return this.ScanForLineEnd(this.currentReadPosition, 0x1f40, false, out lineEndStartPosition, out lineEndEndPosition, out flag);
        }

        private ODataBatchReaderStreamScanResult ScanForLineEnd(int scanStartIx, int maxDataBytesToScan, bool allowLeadingWhitespaceOnly, out int lineEndStartPosition, out int lineEndEndPosition, out bool endOfBufferReached)
        {
            endOfBufferReached = false;
            int num = (this.currentReadPosition + Math.Min(maxDataBytesToScan, this.numberOfBytesInBuffer)) - 1;
            for (int i = scanStartIx; i <= num; i++)
            {
                char c = (char) this.bytes[i];
                switch (c)
                {
                    case '\r':
                    case '\n':
                        lineEndStartPosition = i;
                        if (((c == '\r') && (i == num)) && (maxDataBytesToScan >= this.numberOfBytesInBuffer))
                        {
                            lineEndEndPosition = -1;
                            return ODataBatchReaderStreamScanResult.PartialMatch;
                        }
                        lineEndEndPosition = lineEndStartPosition;
                        if ((c == '\r') && (this.bytes[i + 1] == 10))
                        {
                            lineEndEndPosition++;
                        }
                        return ODataBatchReaderStreamScanResult.Match;
                }
                if (allowLeadingWhitespaceOnly && !char.IsWhiteSpace(c))
                {
                    lineEndStartPosition = -1;
                    lineEndEndPosition = -1;
                    return ODataBatchReaderStreamScanResult.NoMatch;
                }
            }
            endOfBufferReached = true;
            lineEndStartPosition = -1;
            lineEndEndPosition = -1;
            return ODataBatchReaderStreamScanResult.NoMatch;
        }

        private void ShiftToBeginning(int startIndex)
        {
            int count = (this.currentReadPosition + this.numberOfBytesInBuffer) - startIndex;
            this.currentReadPosition = 0;
            if (count <= 0)
            {
                this.numberOfBytesInBuffer = 0;
            }
            else
            {
                this.numberOfBytesInBuffer = count;
                Buffer.BlockCopy(this.bytes, startIndex, this.bytes, 0, count);
            }
        }

        internal void SkipTo(int newPosition)
        {
            int num = newPosition - this.currentReadPosition;
            this.currentReadPosition = newPosition;
            this.numberOfBytesInBuffer -= num;
        }

        internal byte[] Bytes
        {
            get
            {
                return this.bytes;
            }
        }

        internal int CurrentReadPosition
        {
            get
            {
                return this.currentReadPosition;
            }
        }

        internal byte this[int index]
        {
            get
            {
                return this.bytes[index];
            }
        }

        internal int NumberOfBytesInBuffer
        {
            get
            {
                return this.numberOfBytesInBuffer;
            }
        }
    }
}

