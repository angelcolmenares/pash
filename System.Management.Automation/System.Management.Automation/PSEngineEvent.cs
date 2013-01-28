namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    public sealed class PSEngineEvent
    {
        internal static readonly HashSet<string> EngineEvents = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PowerShell.Exiting", "PowerShell.OnIdle", "PowerShell.OnScriptBlockInvoke" };
        public const string Exiting = "PowerShell.Exiting";
        public const string OnIdle = "PowerShell.OnIdle";
        internal const string OnScriptBlockInvoke = "PowerShell.OnScriptBlockInvoke";

        private PSEngineEvent()
        {
        }
    }
}

