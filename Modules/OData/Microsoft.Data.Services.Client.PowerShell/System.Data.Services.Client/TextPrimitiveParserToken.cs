namespace System.Data.Services.Client
{
    using System;
    using System.Runtime.CompilerServices;

    internal class TextPrimitiveParserToken : PrimitiveParserToken
    {
        internal TextPrimitiveParserToken(string text)
        {
            this.Text = text;
        }

        internal override object Materialize(Type clrType)
        {
            return ClientConvert.ChangeType(this.Text, clrType);
        }

        internal string Text { get; private set; }
    }
}

