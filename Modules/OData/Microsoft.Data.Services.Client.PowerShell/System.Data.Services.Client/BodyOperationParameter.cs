namespace System.Data.Services.Client
{
    using System;

    internal sealed class BodyOperationParameter : OperationParameter
    {
        public BodyOperationParameter(string name, object value) : base(name, value)
        {
        }
    }
}

