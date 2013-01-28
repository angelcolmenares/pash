namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    internal class RuntimeDefinedParameterBinder : ParameterBinderBase
    {
        internal RuntimeDefinedParameterBinder(RuntimeDefinedParameterDictionary target, InternalCommand command, CommandLineParameters commandLineParameters) : base(target, command.MyInvocation, command.Context, command)
        {
            foreach (string str in target.Keys)
            {
                RuntimeDefinedParameter parameter = target[str];
                string parameterName = (parameter == null) ? null : parameter.Name;
                if ((parameter == null) || (str != parameterName))
                {
                    ParameterBindingException exception = new ParameterBindingException(ErrorCategory.InvalidArgument, command.MyInvocation, null, parameterName, null, null, "ParameterBinderStrings", "RuntimeDefinedParameterNameMismatch", new object[] { str });
                    throw exception;
                }
            }
            base.CommandLineParameters = commandLineParameters;
        }

        internal override void BindParameter(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            this.Target[name].Value = value;
            base.CommandLineParameters.Add(name, value);
        }

        internal override object GetDefaultParameterValue(string name)
        {
            object obj2 = null;
            if (this.Target.ContainsKey(name))
            {
                RuntimeDefinedParameter parameter = this.Target[name];
                if (parameter != null)
                {
                    obj2 = parameter.Value;
                }
            }
            return obj2;
        }

        internal RuntimeDefinedParameterDictionary Target
        {
            get
            {
                return (base.Target as RuntimeDefinedParameterDictionary);
            }
            set
            {
                base.Target = value;
            }
        }
    }
}

