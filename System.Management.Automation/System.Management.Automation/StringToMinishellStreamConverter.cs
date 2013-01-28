namespace System.Management.Automation
{
    using System;

    internal static class StringToMinishellStreamConverter
    {
        internal const string DebugStream = "debug";
        internal const string ErrorStream = "error";
        internal const string OutputStream = "output";
        internal const string ProgressStream = "progress";
        internal const string VerboseStream = "verbose";
        internal const string WarningStream = "warning";

        internal static MinishellStream ToMinishellStream(string stream)
        {
            MinishellStream unknown = MinishellStream.Unknown;
            if ("output".Equals(stream, StringComparison.OrdinalIgnoreCase))
            {
                return MinishellStream.Output;
            }
            if ("error".Equals(stream, StringComparison.OrdinalIgnoreCase))
            {
                return MinishellStream.Error;
            }
            if ("debug".Equals(stream, StringComparison.OrdinalIgnoreCase))
            {
                return MinishellStream.Debug;
            }
            if ("verbose".Equals(stream, StringComparison.OrdinalIgnoreCase))
            {
                return MinishellStream.Verbose;
            }
            if ("warning".Equals(stream, StringComparison.OrdinalIgnoreCase))
            {
                return MinishellStream.Warning;
            }
            if ("progress".Equals(stream, StringComparison.OrdinalIgnoreCase))
            {
                unknown = MinishellStream.Progress;
            }
            return unknown;
        }
    }
}

