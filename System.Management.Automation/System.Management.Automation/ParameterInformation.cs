namespace System.Management.Automation
{
    using System;
    using System.Reflection;

    internal class ParameterInformation
    {
        internal object defaultValue;
        internal bool isByRef;
        internal bool isOptional;
        internal bool isParamArray;
        internal Type parameterType;

        internal ParameterInformation(ParameterInfo parameter)
        {
            this.isOptional = parameter.IsOptional;
            this.defaultValue = parameter.DefaultValue;
            this.parameterType = parameter.ParameterType;
            if (this.parameterType.IsByRef)
            {
                this.isByRef = true;
                this.parameterType = this.parameterType.GetElementType();
            }
            else
            {
                this.isByRef = false;
            }
        }

        internal ParameterInformation(Type parameterType, bool isOptional, object defaultValue, bool isByRef)
        {
            this.parameterType = parameterType;
            this.isOptional = isOptional;
            this.defaultValue = defaultValue;
            this.isByRef = isByRef;
        }
    }
}

