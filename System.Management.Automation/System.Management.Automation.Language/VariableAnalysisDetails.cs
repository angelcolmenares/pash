namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class VariableAnalysisDetails
    {
        internal VariableAnalysisDetails()
        {
            this.AssociatedAsts = new List<Ast>();
        }

        public List<Ast> AssociatedAsts { get; private set; }

        public bool Automatic { get; set; }

        public int BitIndex { get; set; }

        public int LocalTupleIndex { get; set; }

        public string Name { get; set; }

        public bool PreferenceVariable { get; set; }

        public System.Type Type { get; set; }
    }
}

