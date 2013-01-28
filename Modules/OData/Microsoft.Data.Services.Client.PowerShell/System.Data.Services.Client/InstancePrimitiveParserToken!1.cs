namespace System.Data.Services.Client
{
    using System;
    using System.Runtime.CompilerServices;

    internal class InstancePrimitiveParserToken<T> : PrimitiveParserToken
    {
        internal InstancePrimitiveParserToken(T instance)
        {
            this.Instance = instance;
        }

        internal override object Materialize(Type clrType)
        {
            return this.Instance;
        }

        internal T Instance { get; private set; }
    }
}

