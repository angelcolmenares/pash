namespace System.Management.Automation
{
    using System;

    internal class ComMethodInformation : MethodInformation
    {
        internal Type returnType;

        internal ComMethodInformation(bool hasvarargs, bool hasoptional, ParameterInformation[] arguments, Type returnType) : base(hasvarargs, hasoptional, arguments)
        {
            this.returnType = returnType;
        }

        public Type ReturnType
        {
            get
            {
                return this.returnType;
            }
            set
            {
                this.returnType = value;
            }
        }
    }
}

