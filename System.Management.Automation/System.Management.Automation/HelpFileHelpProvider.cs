namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    internal class HelpFileHelpProvider : HelpProviderWithCache
    {
        private Hashtable _helpFiles;

        internal HelpFileHelpProvider(HelpSystem helpSystem) : base(helpSystem)
        {
            this._helpFiles = new Hashtable();
        }

        internal override IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest)
        {
            int iteratorVariable0 = 0;
            string pattern = helpRequest.Target + ".help.txt";
            Collection<string> iteratorVariable2 = MUIFileSearcher.SearchFiles(pattern, this.GetSearchPaths());
            foreach (string iteratorVariable3 in iteratorVariable2)
            {
                if (!this._helpFiles.ContainsKey(iteratorVariable3))
                {
                    try
                    {
                        this.LoadHelpFile(iteratorVariable3);
                    }
                    catch (IOException exception)
                    {
                        this.ReportHelpFileError(exception, helpRequest.Target, iteratorVariable3);
                    }
                    catch (SecurityException exception2)
                    {
                        this.ReportHelpFileError(exception2, helpRequest.Target, iteratorVariable3);
                    }
                }
                HelpInfo cache = this.GetCache(iteratorVariable3);
                if (cache != null)
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

        private HelpInfo LoadHelpFile(string path)
        {
            string fileName = Path.GetFileName(path);
            if (!path.EndsWith(".help.txt", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            string str2 = fileName.Substring(0, fileName.Length - 9);
            if (string.IsNullOrEmpty(str2))
            {
                return null;
            }
            HelpInfo cache = base.GetCache(path);
            if (cache == null)
            {
                TextReader reader = new StreamReader(path);
                string text = null;
                try
                {
                    text = reader.ReadToEnd();
                }
                finally
                {
                    reader.Close();
                }
                this._helpFiles[path] = 0;
                cache = HelpFileHelpInfo.GetHelpInfo(str2, text, path);
                base.AddCache(path, cache);
            }
            return cache;
        }

        internal override void Reset()
        {
            base.Reset();
            this._helpFiles.Clear();
        }

        internal override IEnumerable<HelpInfo> SearchHelp(HelpRequest helpRequest, bool searchOnlyContent)
        {
            string pattern = helpRequest.Target;
            string iteratorVariable1 = pattern;
            int iteratorVariable2 = 0;
            WildcardPattern iteratorVariable3 = null;
            if (!searchOnlyContent && !WildcardPattern.ContainsWildcardCharacters(pattern))
            {
                iteratorVariable1 = "*" + iteratorVariable1 + "*";
            }
            if (searchOnlyContent)
            {
                string target = helpRequest.Target;
                if (!WildcardPattern.ContainsWildcardCharacters(helpRequest.Target))
                {
                    target = "*" + target + "*";
                }
                iteratorVariable3 = new WildcardPattern(target, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
                iteratorVariable1 = "*";
            }
            iteratorVariable1 = iteratorVariable1 + ".help.txt";
            Collection<string> iteratorVariable4 = MUIFileSearcher.SearchFiles(iteratorVariable1, this.GetSearchPaths());
            if (iteratorVariable4 != null)
            {
                foreach (string iteratorVariable5 in iteratorVariable4)
                {
                    if (!this._helpFiles.ContainsKey(iteratorVariable5))
                    {
                        try
                        {
                            this.LoadHelpFile(iteratorVariable5);
                        }
                        catch (IOException exception)
                        {
                            this.ReportHelpFileError(exception, helpRequest.Target, iteratorVariable5);
                        }
                        catch (SecurityException exception2)
                        {
                            this.ReportHelpFileError(exception2, helpRequest.Target, iteratorVariable5);
                        }
                    }
                    HelpFileHelpInfo cache = this.GetCache(iteratorVariable5) as HelpFileHelpInfo;
                    if ((cache != null) && (!searchOnlyContent || cache.MatchPatternInContent(iteratorVariable3)))
                    {
                        iteratorVariable2++;
                        yield return cache;
                        if ((iteratorVariable2 >= helpRequest.MaxResults) && (helpRequest.MaxResults > 0))
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
                return System.Management.Automation.HelpCategory.HelpFile;
            }
        }

        internal override string Name
        {
            get
            {
                return "HelpFile Help Provider";
            }
        }

        
    }
}

