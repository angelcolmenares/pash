namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class AtomTextConstruct : ODataAnnotatable
    {
        public static implicit operator AtomTextConstruct(string text)
        {
            return ToTextConstruct(text);
        }

        public static AtomTextConstruct ToTextConstruct(string text)
        {
            return new AtomTextConstruct { Text = text };
        }

        public AtomTextConstructKind Kind { get; set; }

        public string Text { get; set; }
    }
}

