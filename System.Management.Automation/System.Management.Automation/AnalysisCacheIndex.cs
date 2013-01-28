namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal class AnalysisCacheIndex
    {
        public Dictionary<string, AnalysisCacheIndexEntry> Entries { get; set; }

        public DateTime LastMaintenance { get; set; }
    }
}

