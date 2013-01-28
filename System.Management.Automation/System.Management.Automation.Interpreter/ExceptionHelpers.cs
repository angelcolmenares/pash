namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal static class ExceptionHelpers
    {
        private const string prevStackTraces = "PreviousStackTraces";

        private static void AssociateStackTraces(Exception e, List<StackTrace> traces)
        {
            e.Data["PreviousStackTraces"] = traces;
        }

        public static IList<StackTrace> GetExceptionStackTraces(Exception rethrow)
        {
            List<StackTrace> list;
            if (!TryGetAssociatedStackTraces(rethrow, out list))
            {
                return null;
            }
            return list;
        }

        private static bool TryGetAssociatedStackTraces(Exception e, out List<StackTrace> traces)
        {
            traces = e.Data["PreviousStackTraces"] as List<StackTrace>;
            return (traces != null);
        }

        public static Exception UpdateForRethrow(Exception rethrow)
        {
            List<StackTrace> list;
            StackTrace item = new StackTrace(rethrow, true);
            if (!TryGetAssociatedStackTraces(rethrow, out list))
            {
                list = new List<StackTrace>();
                AssociateStackTraces(rethrow, list);
            }
            list.Add(item);
            return rethrow;
        }
    }
}

