namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    public static class GetHelpCodeMethods
    {
        private static bool DoesCurrentRunspaceIncludeCoreHelpCmdlet()
        {
            InitialSessionState initialSessionState = Runspace.DefaultRunspace.InitialSessionState;
            if (initialSessionState != null)
            {
                IEnumerable<SessionStateCommandEntry> source = from entry in initialSessionState.Commands["Get-Help"]
                    where entry.Visibility == SessionStateEntryVisibility.Public
                    select entry;
                if (source.Count<SessionStateCommandEntry>() != 1)
                {
                    return false;
                }
                foreach (SessionStateCommandEntry entry in source)
                {
                    SessionStateCmdletEntry entry2 = entry as SessionStateCmdletEntry;
                    if ((entry2 != null) && entry2.ImplementingType.Equals(typeof(GetHelpCommand)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string GetHelpUri(PSObject commandInfoPSObject)
        {
            if (commandInfoPSObject != null)
            {
                CommandInfo info = PSObject.Base(commandInfoPSObject) as CommandInfo;
                if ((info == null) || string.IsNullOrEmpty(info.Name))
                {
                    return string.Empty;
                }
                if ((((info is CmdletInfo) || (info is FunctionInfo)) || ((info is ExternalScriptInfo) || (info is ScriptInfo))) && !string.IsNullOrEmpty(info.CommandMetadata.HelpUri))
                {
                    return info.CommandMetadata.HelpUri;
                }
                AliasInfo info2 = info as AliasInfo;
                if (((info2 != null) && (info2._externalCommandMetadata != null)) && !string.IsNullOrEmpty(info2._externalCommandMetadata.HelpUri))
                {
                    return info2._externalCommandMetadata.HelpUri;
                }
                string name = info.Name;
                if (!string.IsNullOrEmpty(info.ModuleName))
                {
                    name = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", new object[] { info.ModuleName, info.Name });
                }
                if (DoesCurrentRunspaceIncludeCoreHelpCmdlet())
                {
                    ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                    if ((executionContextFromTLS != null) && (executionContextFromTLS.HelpSystem != null))
                    {
                        HelpRequest helpRequest = new HelpRequest(name, info.HelpCategory) {
                            ProviderContext = new ProviderContext(string.Empty, executionContextFromTLS, executionContextFromTLS.SessionState.Path),
                            CommandOrigin = CommandOrigin.Runspace
                        };
                        foreach (Uri uri in from helpInfo in executionContextFromTLS.HelpSystem.ExactMatchHelp(helpRequest)
                            select helpInfo.GetUriForOnlineHelp() into result
                            where null != result
                            select result)
                        {
                            return uri.OriginalString;
                        }
                    }
                }
                else
                {
                    using (PowerShell shell = PowerShell.Create(RunspaceMode.CurrentRunspace).AddCommand("get-help").AddParameter("Name", name).AddParameter("Category", info.HelpCategory.ToString()))
                    {
                        Collection<PSObject> collection = shell.Invoke();
                        if (collection != null)
                        {
                            for (int i = 0; i < collection.Count; i++)
                            {
                                HelpInfo info3;
                                if (LanguagePrimitives.TryConvertTo<HelpInfo>(collection[i], out info3))
                                {
                                    Uri uriForOnlineHelp = info3.GetUriForOnlineHelp();
                                    if (null != uriForOnlineHelp)
                                    {
                                        return uriForOnlineHelp.OriginalString;
                                    }
                                }
                                else
                                {
                                    Uri uriFromCommandPSObject = BaseCommandHelpInfo.GetUriFromCommandPSObject(collection[i]);
                                    return ((uriFromCommandPSObject != null) ? uriFromCommandPSObject.OriginalString : string.Empty);
                                }
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}

