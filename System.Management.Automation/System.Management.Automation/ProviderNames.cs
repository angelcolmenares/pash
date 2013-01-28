namespace System.Management.Automation
{
    using System;

    internal abstract class ProviderNames
    {
        protected ProviderNames()
        {
        }

        internal abstract string Alias { get; }

        internal abstract string Certificate { get; }

        internal abstract string Environment { get; }

        internal abstract string FileSystem { get; }

        internal abstract string Function { get; }

        internal abstract string Registry { get; }

        internal abstract string Variable { get; }
    }
}

