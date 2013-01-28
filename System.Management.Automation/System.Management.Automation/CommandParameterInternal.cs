namespace System.Management.Automation
{
    using System;
    using System.Diagnostics;
    using System.Management.Automation.Language;
    using System.Runtime.InteropServices;

    [DebuggerDisplay("{ParameterName}")]
    internal sealed class CommandParameterInternal
    {
        private Argument _argument;
        private Parameter _parameter;
        private bool _spaceAfterParameter;

        internal static CommandParameterInternal CreateArgument(IScriptExtent extent, object value, bool splatted = false)
        {
            CommandParameterInternal internal2 = new CommandParameterInternal();
            Argument argument = new Argument {
                extent = extent,
                value = value,
                splatted = splatted
            };
            internal2._argument = argument;
            return internal2;
        }

        internal static CommandParameterInternal CreateParameter(IScriptExtent extent, string parameterName, string parameterText)
        {
            CommandParameterInternal internal2 = new CommandParameterInternal();
            Parameter parameter = new Parameter {
                extent = extent,
                parameterName = parameterName,
                parameterText = parameterText
            };
            internal2._parameter = parameter;
            return internal2;
        }

        internal static CommandParameterInternal CreateParameterWithArgument(IScriptExtent parameterExtent, string parameterName, string parameterText, IScriptExtent argumentExtent, object value, bool spaceAfterParameter)
        {
            CommandParameterInternal internal2 = new CommandParameterInternal();
            Parameter parameter = new Parameter {
                extent = parameterExtent,
                parameterName = parameterName,
                parameterText = parameterText
            };
            internal2._parameter = parameter;
            Argument argument = new Argument {
                extent = argumentExtent,
                value = value
            };
            internal2._argument = argument;
            internal2._spaceAfterParameter = spaceAfterParameter;
            return internal2;
        }

        internal bool IsDashQuestion()
        {
            return (this.ParameterNameSpecified && this.ParameterName.Equals("?", StringComparison.OrdinalIgnoreCase));
        }

        internal void SetArgumentValue(IScriptExtent extent, object value)
        {
            if (this._argument == null)
            {
                this._argument = new Argument();
            }
            this._argument.value = value;
            this._argument.extent = extent;
        }

        internal IScriptExtent ArgumentExtent
        {
            get
            {
                if (this._argument == null)
                {
                    return PositionUtilities.EmptyExtent;
                }
                return this._argument.extent;
            }
        }

        internal bool ArgumentSpecified
        {
            get
            {
                return (this._argument != null);
            }
        }

        internal bool ArgumentSplatted
        {
            get
            {
                if (this._argument == null)
                {
                    return false;
                }
                return this._argument.splatted;
            }
        }

        internal object ArgumentValue
        {
            get
            {
                if (this._argument == null)
                {
                    return UnboundParameter.Value;
                }
                return this._argument.value;
            }
        }

        internal IScriptExtent ErrorExtent
        {
            get
            {
                if ((this._argument != null) && (this._argument.extent != PositionUtilities.EmptyExtent))
                {
                    return this._argument.extent;
                }
                if (this._parameter == null)
                {
                    return PositionUtilities.EmptyExtent;
                }
                return this._parameter.extent;
            }
        }

        internal bool ParameterAndArgumentSpecified
        {
            get
            {
                return (this.ParameterNameSpecified && this.ArgumentSpecified);
            }
        }

        internal IScriptExtent ParameterExtent
        {
            get
            {
                if (this._parameter == null)
                {
                    return PositionUtilities.EmptyExtent;
                }
                return this._parameter.extent;
            }
        }

        internal string ParameterName
        {
            get
            {
                return this._parameter.parameterName;
            }
            set
            {
                this._parameter.parameterName = value;
            }
        }

        internal bool ParameterNameSpecified
        {
            get
            {
                return (this._parameter != null);
            }
        }

        internal string ParameterText
        {
            get
            {
                return this._parameter.parameterText;
            }
        }

        internal bool SpaceAfterParameter
        {
            get
            {
                return this._spaceAfterParameter;
            }
        }

        private class Argument
        {
            internal IScriptExtent extent;
            internal bool splatted;
            internal object value;
        }

        private class Parameter
        {
            internal IScriptExtent extent;
            internal string parameterName;
            internal string parameterText;
        }
    }
}

