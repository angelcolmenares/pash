namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class RuntimeDefinedParameterDictionary : Dictionary<string, RuntimeDefinedParameter>
    {
        private string _helpFile;
        internal static RuntimeDefinedParameter[] EmptyParameterArray = new RuntimeDefinedParameter[0];

        public RuntimeDefinedParameterDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
            this._helpFile = string.Empty;
        }

        public object Data { get; set; }

        public string HelpFile
        {
            get
            {
                return this._helpFile;
            }
            set
            {
                this._helpFile = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
    }
}

