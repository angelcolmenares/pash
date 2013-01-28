namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;

    public class PSListModifier<T> : PSListModifier
    {
        public PSListModifier()
        {
        }

        public PSListModifier(Hashtable hash) : base(hash)
        {
        }

        public PSListModifier(object replacementItems) : base(replacementItems)
        {
        }

        public PSListModifier(Collection<object> removeItems, Collection<object> addItems) : base(removeItems, addItems)
        {
        }
    }
}

