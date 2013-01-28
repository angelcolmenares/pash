namespace Microsoft.Powershell.Commands.GetCounter.PdhNative
{
    using System;

    internal enum PdhLogFileType
    {
        PDH_LOG_TYPE_BINARY = 8,
        PDH_LOG_TYPE_CSV = 1,
        PDH_LOG_TYPE_PERFMON = 6,
        PDH_LOG_TYPE_SQL = 7,
        PDH_LOG_TYPE_TRACE_GENERIC = 5,
        PDH_LOG_TYPE_TRACE_KERNEL = 4,
        PDH_LOG_TYPE_TSV = 2,
        PDH_LOG_TYPE_UNDEFINED = 0
    }
}

