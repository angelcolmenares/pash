namespace System.Data.Services.Client
{
    using System;

    internal abstract class PrimitiveParserToken
    {
        protected PrimitiveParserToken()
        {
        }

        internal abstract object Materialize(Type clrType);
    }
}

