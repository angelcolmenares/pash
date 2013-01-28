namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal class AnalysisCacheIndexEntry
    {
        public DateTime LastWriteTime { get; set; }

        public string Path { get; set; }
    }
}

