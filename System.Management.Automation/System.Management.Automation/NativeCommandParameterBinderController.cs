namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Internal;
    using System.Text;

    internal class NativeCommandParameterBinderController : ParameterBinderController
    {
        internal NativeCommandParameterBinderController(NativeCommand command) : base(command.MyInvocation, command.Context, new NativeCommandParameterBinder(command))
        {
        }

        internal override bool BindParameter(CommandParameterInternal argument, ParameterBindingFlags flags)
        {
            base.DefaultParameterBinder.BindParameter(argument.ParameterName, argument.ArgumentValue);
            return true;
        }

        internal override Collection<CommandParameterInternal> BindParameters(Collection<CommandParameterInternal> parameters)
        {
            ArrayList list = new ArrayList();
            foreach (CommandParameterInternal internal2 in parameters)
            {
                if (internal2.ParameterNameSpecified)
                {
                    StringBuilder builder = new StringBuilder();
                    bool spaceAfterParameter = internal2.SpaceAfterParameter;
                    builder.Append(internal2.ParameterText);
                    object argumentValue = internal2.ArgumentValue;
                    if ((argumentValue != AutomationNull.Value) && (argumentValue != UnboundParameter.Value))
                    {
                        if (spaceAfterParameter)
                        {
                            list.Add(builder);
                            builder = new StringBuilder();
                        }
                        NativeCommandParameterBinder.appendOneNativeArgument(base.Context, builder, false, argumentValue);
                    }
                    list.Add(builder);
                }
                else
                {
                    list.Add(internal2.ArgumentValue);
                }
            }
            base.DefaultParameterBinder.BindParameter(null, list);
            return new Collection<CommandParameterInternal>();
        }

        internal string Arguments
        {
            get
            {
                return ((NativeCommandParameterBinder) base.DefaultParameterBinder).Arguments;
            }
        }
    }
}

