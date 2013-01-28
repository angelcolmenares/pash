namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Internal;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public sealed class OutputTypeAttribute : CmdletMetadataAttribute
    {
        private string[] _parameterSetName;
        private string _providerCmdlet;
        private PSTypeName[] _type;

        public OutputTypeAttribute(params string[] type)
        {
            this._parameterSetName = new string[] { "__AllParameterSets" };
            List<PSTypeName> list = new List<PSTypeName>();
            if (type != null)
            {
                foreach (string str in type)
                {
                    list.Add(new PSTypeName(str));
                }
            }
            this._type = list.ToArray();
        }

        public OutputTypeAttribute(params System.Type[] type)
        {
            this._parameterSetName = new string[] { "__AllParameterSets" };
            List<PSTypeName> list = new List<PSTypeName>();
            if (type != null)
            {
                foreach (System.Type type2 in type)
                {
                    list.Add(new PSTypeName(type2));
                }
            }
            this._type = list.ToArray();
        }

        public string[] ParameterSetName
        {
            get
            {
                return this._parameterSetName;
            }
            set
            {
                this._parameterSetName = value;
            }
        }

        public string ProviderCmdlet
        {
            get
            {
                return this._providerCmdlet;
            }
            set
            {
                this._providerCmdlet = value;
            }
        }

        public PSTypeName[] Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

