namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class AliasHelpProvider : HelpProvider
    {
        private CommandDiscovery _commandDiscovery;
        private readonly System.Management.Automation.ExecutionContext _context;
        private SessionState _sessionState;

        internal AliasHelpProvider(HelpSystem helpSystem) : base(helpSystem)
        {
            this._sessionState = helpSystem.ExecutionContext.SessionState;
            this._commandDiscovery = helpSystem.ExecutionContext.CommandDiscovery;
            this._context = helpSystem.ExecutionContext;
        }

        internal override IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest)
        {
            CommandInfo commandInfo = null;
            try
            {
                commandInfo = this._commandDiscovery.LookupCommandInfo(helpRequest.Target);
            }
            catch (CommandNotFoundException)
            {
            }
            if ((commandInfo != null) && (commandInfo.CommandType == CommandTypes.Alias))
            {
                AliasInfo aliasInfo = (AliasInfo) commandInfo;
                HelpInfo helpInfo = AliasHelpInfo.GetHelpInfo(aliasInfo);
                if (helpInfo != null)
                {
                    yield return helpInfo;
                    goto Label_PostSwitchInIterator;
                }
            }
            yield break;
        Label_PostSwitchInIterator:;
        }

        private static bool Match(HelpInfo helpInfo, HelpRequest helpRequest)
        {
            if (helpRequest != null)
            {
                if ((helpRequest.HelpCategory & helpInfo.HelpCategory) == System.Management.Automation.HelpCategory.None)
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

        private static bool Match(string target, string[] patterns)
        {
            if ((patterns == null) || (patterns.Length == 0))
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

        internal override IEnumerable<HelpInfo> SearchHelp(HelpRequest helpRequest, bool searchOnlyContent)
        {
            if (!searchOnlyContent)
            {
                string target = helpRequest.Target;
                string pattern = target;
                Hashtable iteratorVariable2 = new Hashtable(StringComparer.OrdinalIgnoreCase);
                if (!WildcardPattern.ContainsWildcardCharacters(target))
                {
                    pattern = pattern + "*";
                }
                WildcardPattern iteratorVariable3 = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
                IDictionary<string, AliasInfo> aliasTable = this._sessionState.Internal.GetAliasTable();
                foreach (string iteratorVariable5 in aliasTable.Keys)
                {
                    if (iteratorVariable3.IsMatch(iteratorVariable5))
                    {
                        HelpRequest iteratorVariable6 = helpRequest.Clone();
                        iteratorVariable6.Target = iteratorVariable5;
                        foreach (HelpInfo iteratorVariable7 in this.ExactMatchHelp(iteratorVariable6))
                        {
                            if (!Match(iteratorVariable7, helpRequest) || iteratorVariable2.ContainsKey(iteratorVariable5))
                            {
                                continue;
                            }
                            iteratorVariable2.Add(iteratorVariable5, null);
                            yield return iteratorVariable7;
                        }
                    }
                }
                CommandSearcher iteratorVariable8 = new CommandSearcher(pattern, SearchResolutionOptions.ResolveAliasPatterns, CommandTypes.Alias, this._context);
                while (iteratorVariable8.MoveNext())
                {
                    CommandInfo current = iteratorVariable8.Current;
                    if (this._context.CurrentPipelineStopping)
                    {
                        goto Label_0423;
                    }
                    AliasInfo iteratorVariable10 = current as AliasInfo;
                    if (iteratorVariable10 != null)
                    {
                        string name = iteratorVariable10.Name;
                        HelpRequest iteratorVariable12 = helpRequest.Clone();
                        iteratorVariable12.Target = name;
                        foreach (HelpInfo iteratorVariable13 in this.ExactMatchHelp(iteratorVariable12))
                        {
                            if (!Match(iteratorVariable13, helpRequest) || iteratorVariable2.ContainsKey(name))
                            {
                                continue;
                            }
                            iteratorVariable2.Add(name, null);
                            yield return iteratorVariable13;
                        }
                    }
                }
                foreach (CommandInfo iteratorVariable14 in ModuleUtils.GetMatchingCommands(pattern, this._context, helpRequest.CommandOrigin, false))
                {
                    if (this._context.CurrentPipelineStopping)
                    {
                        break;
                    }
                    AliasInfo aliasInfo = iteratorVariable14 as AliasInfo;
                    if (aliasInfo != null)
                    {
                        string key = aliasInfo.Name;
                        HelpInfo helpInfo = AliasHelpInfo.GetHelpInfo(aliasInfo);
                        if (!iteratorVariable2.ContainsKey(key))
                        {
                            iteratorVariable2.Add(key, null);
                            yield return helpInfo;
                        }
                    }
                }
            }
        Label_0423:
            yield break;
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.Alias;
            }
        }

        internal override string Name
        {
            get
            {
                return "Alias Help Provider";
            }
        }

        
    }
}

