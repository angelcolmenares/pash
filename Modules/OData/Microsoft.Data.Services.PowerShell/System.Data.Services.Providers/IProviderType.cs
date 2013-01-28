namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;

    internal interface IProviderType
    {
        IEnumerable<IProviderMember> Members { get; }

        string Name { get; }
    }
}

