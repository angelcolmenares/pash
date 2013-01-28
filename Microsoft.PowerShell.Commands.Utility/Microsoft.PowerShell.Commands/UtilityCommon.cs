namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Text;

    internal static class UtilityCommon
    {
        private static bool alreadyFailing;
        private static bool designForTestability_SkipFailFast;

        internal static void CheckForSevereException(PSCmdlet cmdlet, Exception e)
        {
            if ((e is AccessViolationException) || (e is StackOverflowException))
            {
                try
                {
                    if (!alreadyFailing)
                    {
                        alreadyFailing = true;
                        ExecutionContext executionContext = (cmdlet != null) ? cmdlet.Context : LocalPipeline.GetExecutionContextFromTLS();
                        MshLog.LogCommandHealthEvent(executionContext, e, Severity.Critical);
                    }
                }
                finally
                {
                    if (!designForTestability_SkipFailFast)
                    {
                        WindowsErrorReporting.FailFast(e);
                    }
                }
            }
        }

        internal static Encoding GetEncodingFromEnum(TextEncodingType type)
        {
            Encoding aSCII = Encoding.ASCII;
            switch (type)
            {
                case TextEncodingType.String:
                    return Encoding.Unicode;

                case TextEncodingType.Unicode:
                    return Encoding.Unicode;

                case TextEncodingType.BigEndianUnicode:
                    return Encoding.BigEndianUnicode;

                case TextEncodingType.Utf8:
                    return Encoding.UTF8;

                case TextEncodingType.Utf7:
                    return Encoding.UTF7;

                case TextEncodingType.Ascii:
                    return Encoding.ASCII;
            }
            return Encoding.ASCII;
        }
    }
}

