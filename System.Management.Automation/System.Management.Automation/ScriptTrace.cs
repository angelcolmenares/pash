namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Internal.Host;
    using System.Management.Automation.Runspaces;

    internal static class ScriptTrace
    {
        internal static void Trace(int level, string messageId, string resourceString, params object[] args)
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (executionContextFromTLS != null)
            {
                Trace(executionContextFromTLS, level, messageId, resourceString, args);
            }
        }

        internal static void Trace(ExecutionContext context, int level, string messageId, string resourceString, params object[] args)
        {
            ActionPreference preference = ActionPreference.Continue;
            if (context.PSDebugTraceLevel > level)
            {
                string str;
                if ((args == null) || (args.Length == 0))
                {
                    str = resourceString;
                }
                else
                {
                    str = StringUtil.Format(resourceString, args);
                }
                if (string.IsNullOrEmpty(str))
                {
                    str = "Could not load text for msh script tracing message id '" + messageId + "'";
                }
                ((InternalHostUserInterface) context.EngineHostInterface.UI).WriteDebugLine(str, ref preference);
            }
        }
    }
}

