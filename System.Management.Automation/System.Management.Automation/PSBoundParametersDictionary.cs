namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class PSBoundParametersDictionary : Dictionary<string, object>
    {
        private static readonly object[] EmptyUsingParameters = new object[0];

        internal PSBoundParametersDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
            this.BoundPositionally = new List<string>();
            this.ImplicitUsingParameters = EmptyUsingParameters;
        }

        public List<string> BoundPositionally { get; private set; }

        internal IList ImplicitUsingParameters { get; set; }
    }
}

