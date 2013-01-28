namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    internal abstract class MemberRedirectionAdapter : Adapter
    {
        protected MemberRedirectionAdapter()
        {
        }

        protected override Collection<string> MethodDefinitions(PSMethod method)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected override object MethodInvoke(PSMethod method, object[] arguments)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected override AttributeCollection PropertyAttributes(PSProperty property)
        {
            return new AttributeCollection(new Attribute[0]);
        }

        protected override object PropertyGet(PSProperty property)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected override bool PropertyIsGettable(PSProperty property)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected override bool PropertyIsSettable(PSProperty property)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected override void PropertySet(PSProperty property, object setValue, bool convertIfPossible)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected override string PropertyToString(PSProperty property)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected override string PropertyType(PSProperty property, bool forDisplay)
        {
            throw PSTraceSource.NewNotSupportedException();
        }
    }
}

