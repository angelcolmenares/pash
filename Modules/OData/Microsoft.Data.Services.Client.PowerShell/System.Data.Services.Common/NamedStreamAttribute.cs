namespace System.Data.Services.Common
{
    using System;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    internal sealed class NamedStreamAttribute : Attribute
    {
        public NamedStreamAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}

