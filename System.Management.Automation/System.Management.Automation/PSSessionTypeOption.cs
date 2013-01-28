namespace System.Management.Automation
{
    using System;

    public abstract class PSSessionTypeOption
    {
        protected PSSessionTypeOption()
        {
        }

        protected internal virtual PSSessionTypeOption ConstructObjectFromPrivateData(string privateData)
        {
            throw new NotImplementedException();
        }

        protected internal virtual string ConstructPrivateData()
        {
            throw new NotImplementedException();
        }

        protected internal virtual void CopyUpdatedValuesFrom(PSSessionTypeOption updated)
        {
            throw new NotImplementedException();
        }
    }
}

