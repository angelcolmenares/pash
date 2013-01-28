namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;

    public class PSMethod : PSMethodInfo
    {
        private Adapter adapter;
        internal object adapterData;
        internal object baseObject;
        private bool isSpecial;

        internal PSMethod(string name, Adapter adapter, object baseObject, object adapterData)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            this.adapterData = adapterData;
            this.adapter = adapter;
            this.baseObject = baseObject;
        }

        internal PSMethod(string name, Adapter adapter, object baseObject, object adapterData, bool isSpecial) : this(name, adapter, baseObject, adapterData)
        {
            this.isSpecial = isSpecial;
        }

        public override PSMemberInfo Copy()
        {
            PSMethod destiny = new PSMethod(base.name, this.adapter, this.baseObject, this.adapterData, this.isSpecial);
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        public override object Invoke(params object[] arguments)
        {
            return this.Invoke(null, arguments);
        }

        internal object Invoke(PSMethodInvocationConstraints invocationConstraints, params object[] arguments)
        {
            if (arguments == null)
            {
                throw PSTraceSource.NewArgumentNullException("arguments");
            }
            return this.adapter.BaseMethodInvoke(this, invocationConstraints, arguments);
        }

        internal override void ReplicateInstance(object particularInstance)
        {
            base.ReplicateInstance(particularInstance);
            this.baseObject = particularInstance;
        }

        public override string ToString()
        {
            return this.adapter.BaseMethodToString(this);
        }

        internal bool IsSpecial
        {
            get
            {
                return this.isSpecial;
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.Method;
            }
        }

        public override Collection<string> OverloadDefinitions
        {
            get
            {
                return this.adapter.BaseMethodDefinitions(this);
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                return typeof(PSMethod).FullName;
            }
        }
    }
}

