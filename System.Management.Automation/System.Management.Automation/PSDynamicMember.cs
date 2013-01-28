namespace System.Management.Automation
{
    using System;

    public class PSDynamicMember : PSMemberInfo
    {
        internal PSDynamicMember(string name)
        {
            base.name = name;
        }

        public override PSMemberInfo Copy()
        {
            return new PSDynamicMember(base.Name);
        }

        public override string ToString()
        {
            return ("dynamic " + base.Name);
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.Dynamic;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                return "dynamic";
            }
        }

        public override object Value
        {
            get
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            set
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
        }
    }
}

