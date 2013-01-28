namespace System.Management.Automation.Language
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    public class ScriptRequirements
    {
        internal static readonly ReadOnlyCollection<string> EmptyAssemblyCollection = new ReadOnlyCollection<string>(new string[0]);
        internal static readonly ReadOnlyCollection<ModuleSpecification> EmptyModuleCollection = new ReadOnlyCollection<ModuleSpecification>(new ModuleSpecification[0]);
        internal static readonly ReadOnlyCollection<PSSnapInSpecification> EmptySnapinCollection = new ReadOnlyCollection<PSSnapInSpecification>(new PSSnapInSpecification[0]);

        public string RequiredApplicationId { get; internal set; }

        public ReadOnlyCollection<string> RequiredAssemblies { get; internal set; }

        public ReadOnlyCollection<ModuleSpecification> RequiredModules { get; internal set; }

        public Version RequiredPSVersion { get; internal set; }

        public ReadOnlyCollection<PSSnapInSpecification> RequiresPSSnapIns { get; internal set; }
    }
}

