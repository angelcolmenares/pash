namespace System.Data.Services
{
    using System;

    internal interface IProjectedResult
    {
        object GetProjectedPropertyValue(string propertyName);

        string ResourceTypeName { get; }
    }
}

