namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;
    using System.Xml;

    internal class ProviderHelpProvider : HelpProviderWithCache
    {
        private readonly Hashtable _helpFiles;
        private readonly SessionState _sessionState;

        internal ProviderHelpProvider(HelpSystem helpSystem) : base(helpSystem)
        {
            this._helpFiles = new Hashtable();
            this._sessionState = helpSystem.ExecutionContext.SessionState;
        }

        internal override IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest)
        {
            Collection<ProviderInfo> iteratorVariable0 = null;
            try
            {
                iteratorVariable0 = this._sessionState.Provider.Get(helpRequest.Target);
            }
            catch (ProviderNotFoundException exception)
            {
                if (this.HelpSystem.LastHelpCategory == System.Management.Automation.HelpCategory.Provider)
                {
                    ErrorRecord item = new ErrorRecord(exception, "ProviderLoadError", ErrorCategory.ResourceUnavailable, null) {
                        ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "ProviderLoadError", new object[] { helpRequest.Target, exception.Message })
                    };
                    this.HelpSystem.LastErrors.Add(item);
                }
            }
            if (iteratorVariable0 != null)
            {
                foreach (ProviderInfo iteratorVariable1 in iteratorVariable0)
                {
                    try
                    {
                        this.LoadHelpFile(iteratorVariable1);
                    }
                    catch (IOException exception2)
                    {
                        this.ReportHelpFileError(exception2, helpRequest.Target, iteratorVariable1.HelpFile);
                    }
                    catch (SecurityException exception3)
                    {
                        this.ReportHelpFileError(exception3, helpRequest.Target, iteratorVariable1.HelpFile);
                    }
                    catch (XmlException exception4)
                    {
                        this.ReportHelpFileError(exception4, helpRequest.Target, iteratorVariable1.HelpFile);
                    }
                    HelpInfo cache = this.GetCache(iteratorVariable1.PSSnapInName + @"\" + iteratorVariable1.Name);
                    if (cache != null)
                    {
                        yield return cache;
                    }
                }
            }
        }

        private static string GetProviderAssemblyPath(ProviderInfo providerInfo)
        {
            if (providerInfo == null)
            {
                return null;
            }
            if (providerInfo.ImplementingType == null)
            {
                return null;
            }
            return Path.GetDirectoryName(providerInfo.ImplementingType.Assembly.Location);
        }

        private void LoadHelpFile(ProviderInfo providerInfo)
        {
            if (providerInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerInfo");
            }
            string helpFile = providerInfo.HelpFile;
            if (!string.IsNullOrEmpty(helpFile) && !this._helpFiles.Contains(helpFile))
            {
                string file = helpFile;
                PSSnapInInfo pSSnapIn = providerInfo.PSSnapIn;
                Collection<string> searchPaths = new Collection<string>();
                if (pSSnapIn != null)
                {
                    file = Path.Combine(pSSnapIn.ApplicationBase, helpFile);
                }
                else if ((providerInfo.Module != null) && !string.IsNullOrEmpty(providerInfo.Module.Path))
                {
                    file = Path.Combine(providerInfo.Module.ModuleBase, helpFile);
                }
                else
                {
                    searchPaths.Add(base.GetDefaultShellSearchPath());
                    searchPaths.Add(GetProviderAssemblyPath(providerInfo));
                }
                string str3 = MUIFileSearcher.LocateFile(file, searchPaths);
                if (string.IsNullOrEmpty(str3))
                {
                    throw new FileNotFoundException(helpFile);
                }
                XmlDocument document = InternalDeserializer.LoadUnsafeXmlDocument(new FileInfo(str3), false, null);
                this._helpFiles[helpFile] = 0;
                System.Xml.XmlNode node = null;
                if (document.HasChildNodes)
                {
                    for (int i = 0; i < document.ChildNodes.Count; i++)
                    {
                        System.Xml.XmlNode node2 = document.ChildNodes[i];
                        if ((node2.NodeType == XmlNodeType.Element) && (string.Compare(node2.Name, "helpItems", StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            node = node2;
                            break;
                        }
                    }
                }
                if (node != null)
                {
                    using (base.HelpSystem.Trace(str3))
                    {
                        if (node.HasChildNodes)
                        {
                            for (int j = 0; j < node.ChildNodes.Count; j++)
                            {
                                System.Xml.XmlNode xmlNode = node.ChildNodes[j];
                                if ((xmlNode.NodeType == XmlNodeType.Element) && (string.Compare(xmlNode.Name, "providerHelp", StringComparison.OrdinalIgnoreCase) == 0))
                                {
                                    HelpInfo helpInfo = ProviderHelpInfo.Load(xmlNode);
                                    if (helpInfo != null)
                                    {
                                        base.HelpSystem.TraceErrors(helpInfo.Errors);
                                        helpInfo.FullHelp.TypeNames.Insert(0, string.Format(CultureInfo.InvariantCulture, "ProviderHelpInfo#{0}#{1}", new object[] { providerInfo.PSSnapInName, helpInfo.Name }));
                                        if (!string.IsNullOrEmpty(providerInfo.PSSnapInName))
                                        {
                                            helpInfo.FullHelp.Properties.Add(new PSNoteProperty("PSSnapIn", providerInfo.PSSnapIn));
                                            helpInfo.FullHelp.TypeNames.Insert(1, string.Format(CultureInfo.InvariantCulture, "ProviderHelpInfo#{0}", new object[] { providerInfo.PSSnapInName }));
                                        }
                                        base.AddCache(providerInfo.PSSnapInName + @"\" + helpInfo.Name, helpInfo);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal override IEnumerable<HelpInfo> ProcessForwardedHelp(HelpInfo helpInfo, HelpRequest helpRequest)
        {
            ProviderCommandHelpInfo iteratorVariable0 = new ProviderCommandHelpInfo(helpInfo, helpRequest.ProviderContext);
            yield return iteratorVariable0;
        }

        internal override void Reset()
        {
            base.Reset();
            this._helpFiles.Clear();
        }

        internal override IEnumerable<HelpInfo> SearchHelp(HelpRequest helpRequest, bool searchOnlyContent)
        {
            int iteratorVariable0 = 0;
            string pattern = helpRequest.Target;
            string name = pattern;
            WildcardPattern iteratorVariable3 = null;
            bool iteratorVariable4 = !WildcardPattern.ContainsWildcardCharacters(pattern);
            if (!searchOnlyContent)
            {
                if (iteratorVariable4)
                {
                    name = name + "*";
                }
            }
            else
            {
                string target = helpRequest.Target;
                if (iteratorVariable4)
                {
                    target = "*" + helpRequest.Target + "*";
                }
                iteratorVariable3 = new WildcardPattern(target, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
                name = "*";
            }
            PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(name);
            if (instance != null)
            {
                foreach (ProviderInfo iteratorVariable6 in this._sessionState.Provider.GetAll())
                {
                    if (!iteratorVariable6.IsMatch(name))
                    {
                        continue;
                    }
                    try
                    {
                        this.LoadHelpFile(iteratorVariable6);
                    }
                    catch (IOException exception)
                    {
                        if (!iteratorVariable4)
                        {
                            this.ReportHelpFileError(exception, iteratorVariable6.Name, iteratorVariable6.HelpFile);
                        }
                    }
                    catch (SecurityException exception2)
                    {
                        if (!iteratorVariable4)
                        {
                            this.ReportHelpFileError(exception2, iteratorVariable6.Name, iteratorVariable6.HelpFile);
                        }
                    }
                    catch (XmlException exception3)
                    {
                        if (!iteratorVariable4)
                        {
                            this.ReportHelpFileError(exception3, iteratorVariable6.Name, iteratorVariable6.HelpFile);
                        }
                    }
                    HelpInfo cache = this.GetCache(iteratorVariable6.PSSnapInName + @"\" + iteratorVariable6.Name);
                    if ((cache != null) && (!searchOnlyContent || cache.MatchPatternInContent(iteratorVariable3)))
                    {
                        iteratorVariable0++;
                        yield return cache;
                        if ((iteratorVariable0 >= helpRequest.MaxResults) && (helpRequest.MaxResults > 0))
                        {
                            break;
                        }
                    }
                }
            }
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.Provider;
            }
        }

        internal override string Name
        {
            get
            {
                return "Provider Help Provider";
            }
        }

        
    }
}

