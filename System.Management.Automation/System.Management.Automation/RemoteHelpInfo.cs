namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.ObjectModel;

    internal class RemoteHelpInfo : BaseCommandHelpInfo
    {
        private PSObject deserializedRemoteHelp;

        internal RemoteHelpInfo(ExecutionContext context, RemoteRunspace remoteRunspace, string localCommandName, string remoteHelpTopic, string remoteHelpCategory, HelpCategory localHelpCategory) : base(localHelpCategory)
        {
            using (PowerShell shell = PowerShell.Create())
            {
                Collection<PSObject> collection;
                shell.AddCommand("Get-Help");
                shell.AddParameter("Name", remoteHelpTopic);
                if (!string.IsNullOrEmpty(remoteHelpCategory))
                {
                    shell.AddParameter("Category", remoteHelpCategory);
                }
                shell.Runspace = remoteRunspace;
                using (new PowerShellStopper(context, shell))
                {
                    collection = shell.Invoke();
                }
                if ((collection == null) || (collection.Count == 0))
                {
                    throw new HelpNotFoundException(remoteHelpTopic);
                }
                this.deserializedRemoteHelp = collection[0];
                this.deserializedRemoteHelp.Methods.Remove("ToString");
                PSPropertyInfo info = this.deserializedRemoteHelp.Properties["Name"];
                if (info != null)
                {
                    info.Value = localCommandName;
                }
                PSObject details = base.Details;
                if (details != null)
                {
                    info = details.Properties["Name"];
                    if (info != null)
                    {
                        info.Value = localCommandName;
                    }
                    else
                    {
                        details.InstanceMembers.Add(new PSNoteProperty("Name", localCommandName));
                    }
                }
            }
        }

        private string GetHelpProperty(string propertyName)
        {
            PSPropertyInfo info = this.deserializedRemoteHelp.Properties[propertyName];
            if (info == null)
            {
                return null;
            }
            return (info.Value as string);
        }

        internal override string Component
        {
            get
            {
                return this.GetHelpProperty("Component");
            }
        }

        internal override PSObject FullHelp
        {
            get
            {
                return this.deserializedRemoteHelp;
            }
        }

        internal override string Functionality
        {
            get
            {
                return this.GetHelpProperty("Functionality");
            }
        }

        internal override string Role
        {
            get
            {
                return this.GetHelpProperty("Role");
            }
        }
    }
}

