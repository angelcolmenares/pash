namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;

    internal class ScriptParameterBinderController : ParameterBinderController
    {
        internal const string NotePropertyNameForSplattingParametersInArgs = "<CommandParameterName>";

        internal ScriptParameterBinderController(ScriptBlock script, InvocationInfo invocationInfo, ExecutionContext context, InternalCommand command, SessionStateScope localScope) : base(invocationInfo, context, new ScriptParameterBinder(script, invocationInfo, context, command, localScope))
        {
            this.DollarArgs = new List<object>();
            if (script.HasDynamicParameters)
            {
                base.UnboundParameters = base.BindableParameters.ReplaceMetadata(script.ParameterMetadata);
            }
            else
            {
                base._bindableParameters = script.ParameterMetadata;
                base.UnboundParameters = new List<MergedCompiledCommandParameter>(base._bindableParameters.BindableParameters.Values);
            }
        }

        internal void BindCommandLineParameters(Collection<CommandParameterInternal> arguments)
        {
            ParameterBindingException exception;
            foreach (CommandParameterInternal internal2 in arguments)
            {
                base.UnboundArguments.Add(internal2);
            }
            base.ReparseUnboundArguments();
            base.UnboundArguments = this.BindParameters(base.UnboundArguments);
            base.UnboundArguments = base.BindPositionalParameters(base.UnboundArguments, int.MaxValue, int.MaxValue, out exception);
            try
            {
                base.DefaultParameterBinder.RecordBoundParameters = false;
                base.BindUnboundScriptParameters();
                this.HandleRemainingArguments(base.UnboundArguments);
            }
            finally
            {
                base.DefaultParameterBinder.RecordBoundParameters = true;
            }
        }

        internal override bool BindParameter(CommandParameterInternal argument, ParameterBindingFlags flags)
        {
            base.DefaultParameterBinder.BindParameter(argument.ParameterName, argument.ArgumentValue);
            return true;
        }

        internal override Collection<CommandParameterInternal> BindParameters(Collection<CommandParameterInternal> arguments)
        {
            Collection<CommandParameterInternal> collection = new Collection<CommandParameterInternal>();
            foreach (CommandParameterInternal internal2 in arguments)
            {
                if (!internal2.ParameterNameSpecified)
                {
                    collection.Add(internal2);
                }
                else
                {
                    MergedCompiledCommandParameter parameter = base.BindableParameters.GetMatchingParameter(internal2.ParameterName, false, true, new InvocationInfo(base.InvocationInfo.MyCommand, internal2.ParameterExtent));
                    if (parameter != null)
                    {
                        if (base.BoundParameters.ContainsKey(parameter.Parameter.Name))
                        {
                            ParameterBindingException exception = new ParameterBindingException(ErrorCategory.InvalidArgument, base.InvocationInfo, base.GetParameterErrorExtent(internal2), internal2.ParameterName, null, null, "ParameterBinderStrings", "ParameterAlreadyBound", new object[0]);
                            throw exception;
                        }
                        this.BindParameter(int.MaxValue, internal2, parameter, ParameterBindingFlags.ShouldCoerceType);
                    }
                    else if (internal2.ParameterName.Equals("-%", StringComparison.Ordinal))
                    {
                        base.DefaultParameterBinder.CommandLineParameters.SetImplicitUsingParameters(internal2.ArgumentValue);
                    }
                    else
                    {
                        collection.Add(internal2);
                    }
                }
            }
            return collection;
        }

        private void HandleRemainingArguments(Collection<CommandParameterInternal> arguments)
        {
            ArrayList list = new ArrayList();
            foreach (CommandParameterInternal internal2 in arguments)
            {
                object obj2 = internal2.ArgumentSpecified ? internal2.ArgumentValue : null;
                if (internal2.ParameterAndArgumentSpecified && internal2.ParameterName.Equals("$args", StringComparison.OrdinalIgnoreCase))
                {
                    if (obj2 is object[])
                    {
                        list.AddRange(obj2 as object[]);
                    }
                    else
                    {
                        list.Add(obj2);
                    }
                }
                else
                {
                    if (internal2.ParameterNameSpecified)
                    {
                        PSObject obj3 = new PSObject(string.Copy(internal2.ParameterText));
                        if (obj3.Properties["<CommandParameterName>"] == null)
                        {
                            PSNoteProperty member = new PSNoteProperty("<CommandParameterName>", internal2.ParameterName) {
                                isHidden = true
                            };
                            obj3.Properties.Add(member);
                        }
                        list.Add(obj3);
                    }
                    if (internal2.ArgumentSpecified)
                    {
                        list.Add(obj2);
                    }
                }
            }
            object[] objArray = list.ToArray();
            base.DefaultParameterBinder.BindParameter("args", objArray);
            this.DollarArgs.AddRange(objArray);
        }

        internal List<object> DollarArgs { get; private set; }
    }
}

