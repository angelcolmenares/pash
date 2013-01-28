namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    internal class RequiredModuleInfo
    {
        internal List<string> CommandsToPostFilter { get; set; }

        internal string Name { get; set; }
    }
}

