namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Runspaces;

    public abstract class PSTransportOption : ICloneable
    {
        protected PSTransportOption()
        {
        }

        public object Clone()
        {
            return base.MemberwiseClone();
        }

        internal virtual Hashtable ConstructOptionsAsHashtable()
        {
            throw new NotImplementedException();
        }

        internal virtual string ConstructOptionsAsXmlAttributes()
        {
            throw new NotImplementedException();
        }

        internal virtual string ConstructQuotas()
        {
            throw new NotImplementedException();
        }

        internal virtual Hashtable ConstructQuotasAsHashtable()
        {
            throw new NotImplementedException();
        }

        internal void LoadFromDefaults(PSSessionType sessionType)
        {
            this.LoadFromDefaults(sessionType, false);
        }

        protected internal virtual void LoadFromDefaults(PSSessionType sessionType, bool keepAssigned)
        {
            throw new NotImplementedException();
        }
    }
}

