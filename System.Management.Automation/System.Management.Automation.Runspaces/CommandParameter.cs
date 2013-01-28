namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Language;

    public sealed class CommandParameter
    {
        private string _name;
        private object _value;

        public CommandParameter(string name) : this(name, null)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
        }

        public CommandParameter(string name, object value)
        {
            if (name != null)
            {
                if (name.Trim().Length == 0)
                {
                    throw PSTraceSource.NewArgumentException("name");
                }
                this._name = name;
            }
            else
            {
                this._name = name;
            }
            this._value = value;
        }

        internal static CommandParameter FromCommandParameterInternal(CommandParameterInternal internalParameter)
        {
            if (internalParameter == null)
            {
                throw PSTraceSource.NewArgumentNullException("internalParameter");
            }
            string name = null;
            if (internalParameter.ParameterNameSpecified)
            {
                name = internalParameter.ParameterText;
                if (internalParameter.SpaceAfterParameter)
                {
                    name = name + " ";
                }
            }
            if (internalParameter.ParameterAndArgumentSpecified)
            {
                return new CommandParameter(name, internalParameter.ArgumentValue);
            }
            if (name != null)
            {
                return new CommandParameter(name);
            }
            return new CommandParameter(null, internalParameter.ArgumentValue);
        }

        internal static CommandParameter FromPSObjectForRemoting(PSObject parameterAsPSObject)
        {
            if (parameterAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameterAsPSObject");
            }
            string propertyValue = RemotingDecoder.GetPropertyValue<string>(parameterAsPSObject, "N");
            return new CommandParameter(propertyValue, RemotingDecoder.GetPropertyValue<object>(parameterAsPSObject, "V"));
        }

        internal static CommandParameterInternal ToCommandParameterInternal(CommandParameter publicParameter, bool forNativeCommand)
        {
            string str2;
            if (publicParameter == null)
            {
                throw PSTraceSource.NewArgumentNullException("publicParameter");
            }
            string name = publicParameter.Name;
            object obj2 = publicParameter.Value;
            if (name == null)
            {
                return CommandParameterInternal.CreateArgument(PositionUtilities.EmptyExtent, obj2, false);
            }
            if (!name[0].IsDash())
            {
                str2 = forNativeCommand ? name : ("-" + name);
                return CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, name, str2, PositionUtilities.EmptyExtent, obj2, true);
            }
            bool spaceAfterParameter = false;
            int length = name.Length;
            while ((length > 0) && char.IsWhiteSpace(name[length - 1]))
            {
                spaceAfterParameter = true;
                length--;
            }
            str2 = name.Substring(0, length);
            bool flag2 = name[length - 1] == ':';
            string parameterName = str2.Substring(1, str2.Length - (flag2 ? 2 : 1));
            if (!flag2 && (obj2 == null))
            {
                return CommandParameterInternal.CreateParameter(PositionUtilities.EmptyExtent, parameterName, str2);
            }
            return CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, parameterName, str2, PositionUtilities.EmptyExtent, obj2, spaceAfterParameter);
        }

        internal PSObject ToPSObjectForRemoting()
        {
            PSObject obj2 = RemotingEncoder.CreateEmptyPSObject();
            obj2.Properties.Add(new PSNoteProperty("N", this.Name));
            obj2.Properties.Add(new PSNoteProperty("V", this.Value));
            return obj2;
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public object Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

