namespace System.Data.Services
{
    using System;

    internal interface IExpandedResult
    {
        object GetExpandedPropertyValue(string name);

        object ExpandedElement { get; }
    }
}

