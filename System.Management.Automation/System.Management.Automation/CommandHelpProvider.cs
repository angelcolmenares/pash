namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Help;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;
    using System.Xml;

    internal class CommandHelpProvider : HelpProviderWithCache
    {
        private readonly System.Management.Automation.ExecutionContext _context;
        private readonly Hashtable _helpFiles;
        private static Dictionary<string, string> engineModuleHelpFileCache = new Dictionary<string, string>();
        [TraceSource("CommandHelpProvider", "CommandHelpProvider")]
        private static readonly PSTraceSource tracer = PSTraceSource.GetTracer("CommandHelpProvider", "CommandHelpProvider");

        static CommandHelpProvider()
        {
            engineModuleHelpFileCache.Add("Microsoft.PowerShell.Diagnostics", "Microsoft.PowerShell.Commands.Diagnostics.dll-Help.xml");
            engineModuleHelpFileCache.Add("Microsoft.PowerShell.Core", "System.Management.Automation.dll-Help.xml");
            engineModuleHelpFileCache.Add("Microsoft.PowerShell.Utility", "Microsoft.PowerShell.Commands.Utility.dll-Help.xml");
            engineModuleHelpFileCache.Add("Microsoft.PowerShell.Host", "Microsoft.PowerShell.ConsoleHost.dll-Help.xml");
            engineModuleHelpFileCache.Add("Microsoft.PowerShell.Management", "Microsoft.PowerShell.Commands.Management.dll-Help.xml");
            engineModuleHelpFileCache.Add("Microsoft.PowerShell.Security", "Microsoft.PowerShell.Security.dll-Help.xml");
            engineModuleHelpFileCache.Add("Microsoft.WSMan.Management", "Microsoft.Wsman.Management.dll-Help.xml");
        }

        internal CommandHelpProvider(HelpSystem helpSystem) : base(helpSystem)
        {
            this._helpFiles = new Hashtable();
            this._context = helpSystem.ExecutionContext;
        }

        private void AddToCommandCache(string mshSnapInId, string cmdletName, MamlCommandHelpInfo helpInfo)
        {
            string target = cmdletName;
            helpInfo.FullHelp.TypeNames.Insert(0, string.Format(CultureInfo.InvariantCulture, "MamlCommandHelpInfo#{0}#{1}", new object[] { mshSnapInId, cmdletName }));
            if (!string.IsNullOrEmpty(mshSnapInId))
            {
                target = mshSnapInId + @"\" + target;
                helpInfo.FullHelp.TypeNames.Insert(1, string.Format(CultureInfo.InvariantCulture, "MamlCommandHelpInfo#{0}", new object[] { mshSnapInId }));
            }
            base.AddCache(target, helpInfo);
        }

        internal override IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest)
        {
            int iteratorVariable0 = 0;
            string target = helpRequest.Target;
            Hashtable iteratorVariable2 = new Hashtable(StringComparer.OrdinalIgnoreCase);
            CommandSearcher commandSearcherForExactMatch = this.GetCommandSearcherForExactMatch(target, this._context);
        Label_PostSwitchInIterator:;
            while (commandSearcherForExactMatch.MoveNext())
            {
                CommandInfo current = commandSearcherForExactMatch.Current;
                if (SessionState.IsVisible(helpRequest.CommandOrigin, current))
                {
                    CmdletInfo cmdletInfo = current as CmdletInfo;
                    HelpInfo helpInfo = null;
                    string key = null;
                    if (cmdletInfo != null)
                    {
                        helpInfo = this.GetHelpInfo(cmdletInfo, true);
                        key = cmdletInfo.FullName;
                    }
                    else
                    {
                        IScriptCommandInfo scriptCommandInfo = current as IScriptCommandInfo;
                        if (scriptCommandInfo != null)
                        {
                            key = current.Name;
                            helpInfo = this.GetHelpInfo(scriptCommandInfo, true, false);
                        }
                    }
                    if ((helpInfo != null) && (key != null))
                    {
                        if ((helpInfo.ForwardHelpCategory == helpRequest.HelpCategory) && helpInfo.ForwardTarget.Equals(helpRequest.Target, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new PSInvalidOperationException(HelpErrors.CircularDependencyInHelpForwarding);
                        }
                        if (!iteratorVariable2.ContainsKey(key) && Match(helpInfo, helpRequest, current))
                        {
                            iteratorVariable0++;
                            iteratorVariable2.Add(key, null);
                            yield return helpInfo;
                            if ((iteratorVariable0 >= helpRequest.MaxResults) && (helpRequest.MaxResults > 0))
                            {
                                break;
                            }
                            goto Label_PostSwitchInIterator;
                        }
                    }
                }
            }
        }

        private string FindHelpFile(CmdletInfo cmdletInfo)
        {
            if (cmdletInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdletInfo");
            }
            string helpFile = cmdletInfo.HelpFile;
            if (string.IsNullOrEmpty(helpFile))
            {
                if ((cmdletInfo.Module != null) && InitialSessionState.IsEngineModule(cmdletInfo.Module.Name))
                {
                    return Path.Combine(cmdletInfo.Module.ModuleBase, Thread.CurrentThread.CurrentCulture.Name, engineModuleHelpFileCache[cmdletInfo.Module.Name]);
                }
                return helpFile;
            }
            string file = helpFile;
            PSSnapInInfo pSSnapIn = cmdletInfo.PSSnapIn;
            Collection<string> searchPaths = new Collection<string>();
            if (pSSnapIn != null)
            {
                file = Path.Combine(pSSnapIn.ApplicationBase, helpFile);
            }
            else if ((cmdletInfo.Module != null) && !string.IsNullOrEmpty(cmdletInfo.Module.Path))
            {
                file = Path.Combine(cmdletInfo.Module.ModuleBase, helpFile);
            }
            else
            {
                searchPaths.Add(base.GetDefaultShellSearchPath());
                searchPaths.Add(GetCmdletAssemblyPath(cmdletInfo));
            }
            string str3 = MUIFileSearcher.LocateFile(file, searchPaths);
            if (string.IsNullOrEmpty(str3))
            {
                tracer.WriteLine("Unable to load file {0}", new object[] { file });
            }
            return str3;
        }

        private static string GetCmdletAssemblyPath(CmdletInfo cmdletInfo)
        {
            if (cmdletInfo == null)
            {
                return null;
            }
            if (cmdletInfo.ImplementingType == null)
            {
                return null;
            }
            return Path.GetDirectoryName(cmdletInfo.ImplementingType.Assembly.Location);
        }

        internal virtual CommandSearcher GetCommandSearcherForExactMatch(string commandName, System.Management.Automation.ExecutionContext context)
        {
            return new CommandSearcher(commandName, SearchResolutionOptions.None, CommandTypes.Cmdlet, context);
        }

        internal virtual CommandSearcher GetCommandSearcherForSearch(string pattern, System.Management.Automation.ExecutionContext context)
        {
            return new CommandSearcher(pattern, SearchResolutionOptions.CommandNameIsPattern, CommandTypes.Cmdlet, context);
        }

        private HelpInfo GetFromCommandCache(string helpFileIdentifier, CommandInfo commandInfo)
        {
            HelpInfo info = this.GetFromCommandCache(helpFileIdentifier, commandInfo.Name, commandInfo.HelpCategory);
            if (((info == null) && (commandInfo.Module != null)) && !string.IsNullOrEmpty(commandInfo.Prefix))
            {
                MamlCommandHelpInfo fromCommandCacheByRemovingPrefix = this.GetFromCommandCacheByRemovingPrefix(helpFileIdentifier, commandInfo);
                if (fromCommandCacheByRemovingPrefix != null)
                {
                    this.AddToCommandCache(helpFileIdentifier, commandInfo.Name, fromCommandCacheByRemovingPrefix);
                    return fromCommandCacheByRemovingPrefix;
                }
            }
            return info;
        }

        private HelpInfo GetFromCommandCache(string helpFileIdentifier, string commandName, System.Management.Automation.HelpCategory helpCategory)
        {
            string target = commandName;
            if (!string.IsNullOrEmpty(helpFileIdentifier))
            {
                target = helpFileIdentifier + @"\" + target;
            }
            HelpInfo cache = base.GetCache(target);
            if ((cache != null) && (cache.HelpCategory != helpCategory))
            {
                cache = ((MamlCommandHelpInfo) cache).Copy(helpCategory);
            }
            return cache;
        }

        private MamlCommandHelpInfo GetFromCommandCacheByRemovingPrefix(string helpIdentifier, CommandInfo cmdInfo)
        {
            MamlCommandHelpInfo info = null;
            MamlCommandHelpInfo info2 = this.GetFromCommandCache(helpIdentifier, ModuleCmdletBase.RemovePrefixFromCommandName(cmdInfo.Name, cmdInfo.Prefix), cmdInfo.HelpCategory) as MamlCommandHelpInfo;
            if (info2 != null)
            {
                info = info2.Copy();
                if (info.FullHelp.Properties["Name"] != null)
                {
                    info.FullHelp.Properties.Remove("Name");
                }
                info.FullHelp.Properties.Add(new PSNoteProperty("Name", cmdInfo.Name));
                if ((info.FullHelp.Properties["Details"] == null) || (info.FullHelp.Properties["Details"].Value == null))
                {
                    return info;
                }
                PSObject obj2 = PSObject.AsPSObject(info.FullHelp.Properties["Details"].Value).Copy();
                if (obj2.Properties["Name"] != null)
                {
                    obj2.Properties.Remove("Name");
                }
                obj2.Properties.Add(new PSNoteProperty("Name", cmdInfo.Name));
                info.FullHelp.Properties["Details"].Value = obj2;
            }
            return info;
        }

        private HelpInfo GetFromCommandCacheOrCmdletInfo(CmdletInfo cmdletInfo)
        {
            HelpInfo info = this.GetFromCommandCache(cmdletInfo.ModuleName, cmdletInfo.Name, cmdletInfo.HelpCategory);
            if (((info == null) && (cmdletInfo.Module != null)) && !string.IsNullOrEmpty(cmdletInfo.Prefix))
            {
                MamlCommandHelpInfo fromCommandCacheByRemovingPrefix = this.GetFromCommandCacheByRemovingPrefix(cmdletInfo.ModuleName, cmdletInfo);
                if (fromCommandCacheByRemovingPrefix != null)
                {
                    if ((fromCommandCacheByRemovingPrefix.FullHelp.Properties["Details"] != null) && (fromCommandCacheByRemovingPrefix.FullHelp.Properties["Details"].Value != null))
                    {
                        PSObject obj2 = PSObject.AsPSObject(fromCommandCacheByRemovingPrefix.FullHelp.Properties["Details"].Value);
                        if (obj2.Properties["Noun"] != null)
                        {
                            obj2.Properties.Remove("Noun");
                        }
                        obj2.Properties.Add(new PSNoteProperty("Noun", cmdletInfo.Noun));
                    }
                    this.AddToCommandCache(cmdletInfo.ModuleName, cmdletInfo.Name, fromCommandCacheByRemovingPrefix);
                    return fromCommandCacheByRemovingPrefix;
                }
            }
            if (info == null)
            {
                PSObject pSObjectFromCmdletInfo = DefaultCommandHelpObjectBuilder.GetPSObjectFromCmdletInfo(cmdletInfo);
                pSObjectFromCmdletInfo.TypeNames.Clear();
                pSObjectFromCmdletInfo.TypeNames.Add(DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp);
                pSObjectFromCmdletInfo.TypeNames.Add("CmdletHelpInfo");
                pSObjectFromCmdletInfo.TypeNames.Add("HelpInfo");
                info = new MamlCommandHelpInfo(pSObjectFromCmdletInfo, cmdletInfo.HelpCategory);
            }
            return info;
        }

        private HelpInfo GetHelpInfo(CmdletInfo cmdletInfo, bool reportErrors)
        {
            if (this.GetFromCommandCache(cmdletInfo.ModuleName, cmdletInfo.Name, cmdletInfo.HelpCategory) == null)
            {
                string key = this.FindHelpFile(cmdletInfo);
                if ((key != null) && !this._helpFiles.Contains(key))
                {
                    this.LoadHelpFile(key, cmdletInfo.ModuleName, cmdletInfo.Name, reportErrors);
                }
            }
            HelpInfo fromCommandCacheOrCmdletInfo = this.GetFromCommandCacheOrCmdletInfo(cmdletInfo);
            if (fromCommandCacheOrCmdletInfo != null)
            {
                if (fromCommandCacheOrCmdletInfo.FullHelp.Properties["PSSnapIn"] == null)
                {
                    fromCommandCacheOrCmdletInfo.FullHelp.Properties.Add(new PSNoteProperty("PSSnapIn", cmdletInfo.PSSnapIn));
                }
                if (fromCommandCacheOrCmdletInfo.FullHelp.Properties["ModuleName"] == null)
                {
                    fromCommandCacheOrCmdletInfo.FullHelp.Properties.Add(new PSNoteProperty("ModuleName", cmdletInfo.ModuleName));
                }
            }
            return fromCommandCacheOrCmdletInfo;
        }

        private HelpInfo GetHelpInfo(IScriptCommandInfo scriptCommandInfo, bool reportErrors, bool searchOnlyContent)
        {
            CommandInfo commandInfo = (CommandInfo) scriptCommandInfo;
            HelpInfo helpInfoFromWorkflow = null;
            ScriptBlock scriptBlock = null;
            try
            {
                scriptBlock = scriptCommandInfo.ScriptBlock;
            }
            catch (RuntimeException)
            {
                return null;
            }
            if (scriptBlock != null)
            {
                string helpFile = null;
                string str2 = null;
                string helpUriFromDotLink = null;
                helpInfoFromWorkflow = scriptBlock.GetHelpInfo(this._context, commandInfo, searchOnlyContent, base.HelpSystem.ScriptBlockTokenCache, out helpFile, out helpUriFromDotLink);
                if (!string.IsNullOrEmpty(helpUriFromDotLink))
                {
                    try
                    {
                        new Uri(helpUriFromDotLink);
                        str2 = helpUriFromDotLink;
                    }
                    catch (UriFormatException)
                    {
                    }
                }
                if (helpInfoFromWorkflow != null)
                {
                    Uri uriForOnlineHelp = helpInfoFromWorkflow.GetUriForOnlineHelp();
                    if (uriForOnlineHelp != null)
                    {
                        str2 = uriForOnlineHelp.ToString();
                    }
                }
                if (helpFile != null)
                {
                    if (!this._helpFiles.Contains(helpFile))
                    {
                        this.LoadHelpFile(helpFile, helpFile, commandInfo.Name, reportErrors);
                    }
                    helpInfoFromWorkflow = this.GetFromCommandCache(helpFile, commandInfo) ?? helpInfoFromWorkflow;
                }
                if (helpInfoFromWorkflow == null)
                {
                    if ((commandInfo.CommandType == CommandTypes.ExternalScript) || (commandInfo.CommandType == CommandTypes.Script))
                    {
                        helpInfoFromWorkflow = SyntaxHelpInfo.GetHelpInfo(commandInfo.Name, commandInfo.Syntax, commandInfo.HelpCategory);
                    }
                    else
                    {
                        if (commandInfo.CommandType == CommandTypes.Workflow)
                        {
                            helpInfoFromWorkflow = this.GetHelpInfoFromWorkflow(commandInfo, reportErrors);
                        }
                        if (helpInfoFromWorkflow == null)
                        {
                            PSObject pSObjectFromCmdletInfo = DefaultCommandHelpObjectBuilder.GetPSObjectFromCmdletInfo(commandInfo);
                            pSObjectFromCmdletInfo.TypeNames.Clear();
                            pSObjectFromCmdletInfo.TypeNames.Add(DefaultCommandHelpObjectBuilder.TypeNameForDefaultHelp);
                            pSObjectFromCmdletInfo.TypeNames.Add("CmdletHelpInfo");
                            pSObjectFromCmdletInfo.TypeNames.Add("HelpInfo");
                            helpInfoFromWorkflow = new MamlCommandHelpInfo(pSObjectFromCmdletInfo, commandInfo.HelpCategory);
                        }
                    }
                }
                if (helpInfoFromWorkflow.GetUriForOnlineHelp() == null)
                {
                    if (!string.IsNullOrEmpty(commandInfo.CommandMetadata.HelpUri))
                    {
                        DefaultCommandHelpObjectBuilder.AddRelatedLinksProperties(helpInfoFromWorkflow.FullHelp, commandInfo.CommandMetadata.HelpUri);
                    }
                    else if (!string.IsNullOrEmpty(str2))
                    {
                        DefaultCommandHelpObjectBuilder.AddRelatedLinksProperties(helpInfoFromWorkflow.FullHelp, str2);
                    }
                }
            }
            if ((helpInfoFromWorkflow != null) && (helpInfoFromWorkflow.FullHelp.Properties["ModuleName"] == null))
            {
                helpInfoFromWorkflow.FullHelp.Properties.Add(new PSNoteProperty("ModuleName", commandInfo.ModuleName));
            }
            return helpInfoFromWorkflow;
        }

        private HelpInfo GetHelpInfoFromWorkflow(CommandInfo workflow, bool reportErrors)
        {
            HelpInfo info = null;
            if (workflow.Module == null)
            {
                return info;
            }
            if (workflow.Module.NestedModules != null)
            {
                foreach (PSModuleInfo info2 in workflow.Module.NestedModules)
                {
                    string str = Path.GetFileName(info2.Path) + "-Help.xml";
                    if (!this._helpFiles.Contains(str))
                    {
                        this.LoadHelpFile(Path.Combine(workflow.Module.ModuleBase, Thread.CurrentThread.CurrentCulture.Name, str), str, workflow.Name, reportErrors);
                    }
                    info = this.GetFromCommandCache(str, workflow) ?? info;
                    if (info != null)
                    {
                        return info;
                    }
                }
            }
            if (info != null)
            {
                return info;
            }
            string key = workflow.Module.Name + "-Help.xml";
            if (!this._helpFiles.Contains(key))
            {
                this.LoadHelpFile(Path.Combine(workflow.Module.ModuleBase, Thread.CurrentThread.CurrentCulture.Name, key), key, workflow.Name, reportErrors);
            }
            return (this.GetFromCommandCache(key, workflow) ?? info);
        }

        internal static bool IsMamlHelp(string helpFile, System.Xml.XmlNode helpItemsNode)
        {
            if (helpFile.EndsWith(".maml", true, CultureInfo.CurrentCulture))
            {
                return true;
            }
            if (helpItemsNode.Attributes != null)
            {
                foreach (System.Xml.XmlNode node in helpItemsNode.Attributes)
                {
                    if (node.Name.Equals("schema", StringComparison.OrdinalIgnoreCase) && node.Value.Equals("maml", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void LoadHelpFile(string helpFile, string helpFileIdentifier)
        {
            XmlDocument document = InternalDeserializer.LoadUnsafeXmlDocument(new FileInfo(helpFile), false, null);
            this._helpFiles[helpFile] = 0;
            System.Xml.XmlNode helpItemsNode = null;
            if (document.HasChildNodes)
            {
                for (int i = 0; i < document.ChildNodes.Count; i++)
                {
                    System.Xml.XmlNode node2 = document.ChildNodes[i];
                    if ((node2.NodeType == XmlNodeType.Element) && (string.Compare(node2.LocalName, "helpItems", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        helpItemsNode = node2;
                        break;
                    }
                }
            }
            if (helpItemsNode == null)
            {
                tracer.WriteLine("Unable to find 'helpItems' element in file {0}", new object[] { helpFile });
            }
            else
            {
                bool flag = IsMamlHelp(helpFile, helpItemsNode);
                using (base.HelpSystem.Trace(helpFile))
                {
                    if (helpItemsNode.HasChildNodes)
                    {
                        for (int j = 0; j < helpItemsNode.ChildNodes.Count; j++)
                        {
                            System.Xml.XmlNode xmlNode = helpItemsNode.ChildNodes[j];
                            if ((xmlNode.NodeType == XmlNodeType.Element) && (string.Compare(xmlNode.LocalName, "command", StringComparison.OrdinalIgnoreCase) == 0))
                            {
                                MamlCommandHelpInfo helpInfo = null;
                                if (flag)
                                {
                                    helpInfo = MamlCommandHelpInfo.Load(xmlNode, System.Management.Automation.HelpCategory.Cmdlet);
                                }
                                if (helpInfo != null)
                                {
                                    base.HelpSystem.TraceErrors(helpInfo.Errors);
                                    this.AddToCommandCache(helpFileIdentifier, helpInfo.Name, helpInfo);
                                }
                            }
                            if ((xmlNode.NodeType == XmlNodeType.Element) && (string.Compare(xmlNode.Name, "UserDefinedData", StringComparison.OrdinalIgnoreCase) == 0))
                            {
                                UserDefinedHelpData userDefinedHelpData = UserDefinedHelpData.Load(xmlNode);
                                this.ProcessUserDefineddHelpData(helpFileIdentifier, userDefinedHelpData);
                            }
                        }
                    }
                }
            }
        }

        private void LoadHelpFile(string helpFile, string helpFileIdentifier, string commandName, bool reportErrors)
        {
            Exception exception = null;
            try
            {
                this.LoadHelpFile(helpFile, helpFileIdentifier);
            }
            catch (IOException exception2)
            {
                exception = exception2;
            }
            catch (SecurityException exception3)
            {
                exception = exception3;
            }
            catch (XmlException exception4)
            {
                exception = exception4;
            }
            catch (NotSupportedException exception5)
            {
                exception = exception5;
            }
            catch (UnauthorizedAccessException exception6)
            {
                exception = exception6;
            }
            catch (InvalidOperationException exception7)
            {
                exception = exception7;
            }
            if (reportErrors && (exception != null))
            {
                base.ReportHelpFileError(exception, commandName, helpFile);
            }
        }

        private static bool Match(string target, ICollection<string> patterns)
        {
            if ((patterns == null) || (patterns.Count == 0))
            {
                return true;
            }
            foreach (string str in patterns)
            {
                if (Match(target, str))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool Match(string target, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }
            if (string.IsNullOrEmpty(target))
            {
                target = "";
            }
            WildcardPattern pattern2 = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
            return pattern2.IsMatch(target);
        }

        private static bool Match(HelpInfo helpInfo, HelpRequest helpRequest, CommandInfo commandInfo)
        {
            if (helpRequest != null)
            {
                if ((helpRequest.HelpCategory & commandInfo.HelpCategory) == System.Management.Automation.HelpCategory.None)
                {
                    return false;
                }
                if (!(helpInfo is BaseCommandHelpInfo))
                {
                    return false;
                }
                if (!Match(helpInfo.Component, helpRequest.Component))
                {
                    return false;
                }
                if (!Match(helpInfo.Role, helpRequest.Role))
                {
                    return false;
                }
                if (!Match(helpInfo.Functionality, helpRequest.Functionality))
                {
                    return false;
                }
            }
            return true;
        }

        internal override IEnumerable<HelpInfo> ProcessForwardedHelp(HelpInfo helpInfo, HelpRequest helpRequest)
        {
            System.Management.Automation.HelpCategory iteratorVariable0 = System.Management.Automation.HelpCategory.Workflow | System.Management.Automation.HelpCategory.ExternalScript | System.Management.Automation.HelpCategory.Filter | System.Management.Automation.HelpCategory.Function | System.Management.Automation.HelpCategory.ScriptCommand | System.Management.Automation.HelpCategory.Alias;
            if ((helpInfo.HelpCategory & iteratorVariable0) != System.Management.Automation.HelpCategory.None)
            {
                HelpRequest iteratorVariable1 = helpRequest.Clone();
                iteratorVariable1.Target = helpInfo.ForwardTarget;
                iteratorVariable1.CommandOrigin = CommandOrigin.Internal;
                if ((helpInfo.ForwardHelpCategory != System.Management.Automation.HelpCategory.None) && (helpInfo.HelpCategory != System.Management.Automation.HelpCategory.Alias))
                {
                    iteratorVariable1.HelpCategory = helpInfo.ForwardHelpCategory;
                }
                else
                {
                    try
                    {
                        CommandInfo commandInfo = this._context.CommandDiscovery.LookupCommandInfo(iteratorVariable1.Target);
                        iteratorVariable1.HelpCategory = commandInfo.HelpCategory;
                    }
                    catch (CommandNotFoundException)
                    {
                    }
                }
                IEnumerator<HelpInfo> enumerator = this.ExactMatchHelp(iteratorVariable1).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    HelpInfo current = enumerator.Current;
                    yield return current;
                }
            }
            else
            {
                yield return helpInfo;
            }
        }

        private void ProcessUserDefineddHelpData(string mshSnapInId, UserDefinedHelpData userDefinedHelpData)
        {
            if ((userDefinedHelpData != null) && !string.IsNullOrEmpty(userDefinedHelpData.Name))
            {
                HelpInfo info = this.GetFromCommandCache(mshSnapInId, userDefinedHelpData.Name, System.Management.Automation.HelpCategory.Cmdlet);
                if (info != null)
                {
                    MamlCommandHelpInfo info2 = info as MamlCommandHelpInfo;
                    if (info2 != null)
                    {
                        info2.AddUserDefinedData(userDefinedHelpData);
                    }
                }
            }
        }

        internal override void Reset()
        {
            base.Reset();
            this._helpFiles.Clear();
        }

        internal override IEnumerable<HelpInfo> SearchHelp(HelpRequest helpRequest, bool searchOnlyContent)
        {
            string item = helpRequest.Target;
            Collection<string> iteratorVariable1 = new Collection<string>();
            WildcardPattern pattern = null;
            bool iteratorVariable3 = !WildcardPattern.ContainsWildcardCharacters(helpRequest.Target);
            if (!searchOnlyContent)
            {
                if (iteratorVariable3)
                {
                    if (item.IndexOf('-') >= 0)
                    {
                        iteratorVariable1.Add(item + "*");
                    }
                    else
                    {
                        iteratorVariable1.Add("*" + item + "*");
                    }
                }
                else
                {
                    iteratorVariable1.Add(item);
                }
            }
            else
            {
                iteratorVariable1.Add("*");
                string target = helpRequest.Target;
                if (iteratorVariable3)
                {
                    target = "*" + helpRequest.Target + "*";
                }
                pattern = new WildcardPattern(target, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
            }
            int iteratorVariable4 = 0;
            Hashtable iteratorVariable5 = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable iteratorVariable6 = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (string iteratorVariable7 in iteratorVariable1)
            {
                CommandSearcher commandSearcherForSearch = this.GetCommandSearcherForSearch(iteratorVariable7, this._context);
                while (commandSearcherForSearch.MoveNext())
                {
                    if (this._context.CurrentPipelineStopping)
                    {
                        break;
                    }
                    CommandInfo current = commandSearcherForSearch.Current;
                    CmdletInfo cmdletInfo = current as CmdletInfo;
                    HelpInfo helpInfo = null;
                    string key = null;
                    if (cmdletInfo != null)
                    {
                        helpInfo = this.GetHelpInfo(cmdletInfo, !iteratorVariable3);
                        key = cmdletInfo.FullName;
                    }
                    else
                    {
                        IScriptCommandInfo scriptCommandInfo = current as IScriptCommandInfo;
                        if (scriptCommandInfo != null)
                        {
                            key = current.Name;
                            helpInfo = this.GetHelpInfo(scriptCommandInfo, !iteratorVariable3, searchOnlyContent);
                        }
                    }
                    if (helpInfo != null)
                    {
                        if (!SessionState.IsVisible(helpRequest.CommandOrigin, current))
                        {
                            if (!iteratorVariable6.ContainsKey(key))
                            {
                                iteratorVariable6.Add(key, null);
                            }
                        }
                        else if ((!iteratorVariable5.ContainsKey(key) && Match(helpInfo, helpRequest, current)) && (!searchOnlyContent || helpInfo.MatchPatternInContent(pattern)))
                        {
                            iteratorVariable5.Add(key, null);
                            iteratorVariable4++;
                            yield return helpInfo;
                            if ((iteratorVariable4 < helpRequest.MaxResults) || (helpRequest.MaxResults <= 0))
                            {
                                continue;
                            }
                            break;
                        }
                    }
                }
                if (this.HelpCategory == (System.Management.Automation.HelpCategory.Cmdlet | System.Management.Automation.HelpCategory.Alias))
                {
                    foreach (CommandInfo iteratorVariable13 in ModuleUtils.GetMatchingCommands(iteratorVariable7, this._context, helpRequest.CommandOrigin, false))
                    {
                        if (this._context.CurrentPipelineStopping)
                        {
                            break;
                        }
                        if (SessionState.IsVisible(helpRequest.CommandOrigin, iteratorVariable13))
                        {
                            CmdletInfo iteratorVariable14 = iteratorVariable13 as CmdletInfo;
                            HelpInfo iteratorVariable15 = null;
                            string fullName = null;
                            if (iteratorVariable14 != null)
                            {
                                iteratorVariable15 = this.GetHelpInfo(iteratorVariable14, !iteratorVariable3);
                                fullName = iteratorVariable14.FullName;
                            }
                            else
                            {
                                IScriptCommandInfo info2 = iteratorVariable13 as IScriptCommandInfo;
                                if (info2 != null)
                                {
                                    fullName = iteratorVariable13.Name;
                                    iteratorVariable15 = this.GetHelpInfo(info2, !iteratorVariable3, searchOnlyContent);
                                }
                            }
                            if ((((iteratorVariable15 != null) && !iteratorVariable5.ContainsKey(fullName)) && (!iteratorVariable6.ContainsKey(fullName) && Match(iteratorVariable15, helpRequest, iteratorVariable13))) && (!searchOnlyContent || iteratorVariable15.MatchPatternInContent(pattern)))
                            {
                                iteratorVariable5.Add(fullName, null);
                                iteratorVariable4++;
                                yield return iteratorVariable15;
                                if ((iteratorVariable4 >= helpRequest.MaxResults) && (helpRequest.MaxResults > 0))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return (System.Management.Automation.HelpCategory.Cmdlet | System.Management.Automation.HelpCategory.Alias);
            }
        }

        internal override string Name
        {
            get
            {
                return "Command Help Provider";
            }
        }

        
    }
}

