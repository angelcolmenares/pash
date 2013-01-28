namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;

    public class RemoteCommandInfo : CommandInfo
    {
        private string definition;

        private RemoteCommandInfo(string name, CommandTypes type) : base(name, type)
        {
        }

        internal static RemoteCommandInfo FromPSObjectForRemoting(PSObject psObject)
        {
            RemoteCommandInfo info = null;
            if (SerializationUtilities.GetPsObjectPropertyBaseObject(psObject, "CommandInfo_CommandType") != null)
            {
                CommandTypes propertyValue = RemotingDecoder.GetPropertyValue<CommandTypes>(psObject, "CommandInfo_CommandType");
                info = new RemoteCommandInfo(RemotingDecoder.GetPropertyValue<string>(psObject, "CommandInfo_Name"), propertyValue) {
                    definition = RemotingDecoder.GetPropertyValue<string>(psObject, "CommandInfo_Definition"),
                    Visibility = RemotingDecoder.GetPropertyValue<SessionStateEntryVisibility>(psObject, "CommandInfo_Visibility")
                };
            }
            return info;
        }

        internal static void ToPSObjectForRemoting(CommandInfo commandInfo, PSObject psObject)
        {
            RemotingEncoder.ValueGetterDelegate<CommandTypes> valueGetter = null;
            RemotingEncoder.ValueGetterDelegate<string> delegate3 = null;
            RemotingEncoder.ValueGetterDelegate<string> delegate4 = null;
            RemotingEncoder.ValueGetterDelegate<SessionStateEntryVisibility> delegate5 = null;
            if (commandInfo != null)
            {
                if (valueGetter == null)
                {
                    valueGetter = () => commandInfo.CommandType;
                }
                RemotingEncoder.AddNoteProperty<CommandTypes>(psObject, "CommandInfo_CommandType", valueGetter);
                if (delegate3 == null)
                {
                    delegate3 = () => commandInfo.Definition;
                }
                RemotingEncoder.AddNoteProperty<string>(psObject, "CommandInfo_Definition", delegate3);
                if (delegate4 == null)
                {
                    delegate4 = () => commandInfo.Name;
                }
                RemotingEncoder.AddNoteProperty<string>(psObject, "CommandInfo_Name", delegate4);
                if (delegate5 == null)
                {
                    delegate5 = () => commandInfo.Visibility;
                }
                RemotingEncoder.AddNoteProperty<SessionStateEntryVisibility>(psObject, "CommandInfo_Visibility", delegate5);
            }
        }

        public override string Definition
        {
            get
            {
                return this.definition;
            }
        }

        public override ReadOnlyCollection<PSTypeName> OutputType
        {
            get
            {
                return null;
            }
        }
    }
}

