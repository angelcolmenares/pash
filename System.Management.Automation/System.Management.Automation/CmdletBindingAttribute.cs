namespace System.Management.Automation
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class CmdletBindingAttribute : CmdletCommonMetadataAttribute
    {
        private bool _positionalBinding = true;

        public bool PositionalBinding
        {
            get
            {
                return this._positionalBinding;
            }
            set
            {
                this._positionalBinding = value;
            }
        }
    }
}

