namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;

    public abstract class PSMethodInfo : PSMemberInfo
    {
        protected PSMethodInfo()
        {
        }

        public abstract object Invoke(params object[] arguments);

        public abstract Collection<string> OverloadDefinitions { get; }

        public sealed override object Value
        {
            get
            {
                return this;
            }
            set
            {
                throw new ExtendedTypeSystemException("CannotChangePSMethodInfoValue", null, ExtendedTypeSystem.CannotSetValueForMemberType, new object[] { base.GetType().FullName });
            }
        }
    }
}

