namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Threading;

    internal class MUIFileSearcher
    {
        private Collection<string> _result;
        private System.Management.Automation.SearchMode _searchMode;
        private Collection<string> _searchPaths;
        private string _target;
        private Hashtable _uniqueMatches;

        private MUIFileSearcher(string target, Collection<string> searchPaths) : this(target, searchPaths, System.Management.Automation.SearchMode.Unique)
        {
        }

        private MUIFileSearcher(string target, Collection<string> searchPaths, System.Management.Automation.SearchMode searchMode)
        {
            this._searchMode = System.Management.Automation.SearchMode.Unique;
            this._uniqueMatches = new Hashtable(StringComparer.OrdinalIgnoreCase);
            this._target = target;
            this._searchPaths = searchPaths;
            this._searchMode = searchMode;
        }

        private static string GetMshDefaultInstallationPath()
        {
            string shellPathFromRegistry = CommandDiscovery.GetShellPathFromRegistry(Utils.DefaultPowerShellShellID);
            if (shellPathFromRegistry != null)
            {
                shellPathFromRegistry = Path.GetDirectoryName(shellPathFromRegistry);
            }
            return shellPathFromRegistry;
        }

        internal static string LocateFile(string file)
        {
            return LocateFile(file, new Collection<string>());
        }

        internal static string LocateFile(string file, Collection<string> searchPaths)
        {
            MUIFileSearcher searcher = new MUIFileSearcher(file, searchPaths, System.Management.Automation.SearchMode.First);
            if ((searcher.Result != null) && (searcher.Result.Count != 0))
            {
                return searcher.Result[0];
            }
            return null;
        }

        private static Collection<string> NormalizeSearchPaths(string target, Collection<string> searchPaths)
        {
            Collection<string> collection = new Collection<string>();
            if (!string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(Path.GetDirectoryName(target)))
            {
                string directoryName = Path.GetDirectoryName(target);
                if (Directory.Exists(directoryName))
                {
                    collection.Add(Path.GetFullPath(directoryName));
                }
                return collection;
            }
            if (searchPaths != null)
            {
                foreach (string str2 in searchPaths)
                {
                    if (!collection.Contains(str2) && Directory.Exists(str2))
                    {
                        collection.Add(str2);
                    }
                }
            }
            string mshDefaultInstallationPath = GetMshDefaultInstallationPath();
            if (((mshDefaultInstallationPath != null) && !collection.Contains(mshDefaultInstallationPath)) && Directory.Exists(mshDefaultInstallationPath))
            {
                collection.Add(mshDefaultInstallationPath);
            }
            return collection;
        }

        internal static Collection<string> SearchFiles(string pattern)
        {
            return SearchFiles(pattern, new Collection<string>());
        }

        internal static Collection<string> SearchFiles(string pattern, Collection<string> searchPaths)
        {
            MUIFileSearcher searcher = new MUIFileSearcher(pattern, searchPaths);
            return searcher.Result;
        }

        private void SearchForFiles()
        {
            if (!string.IsNullOrEmpty(this.Target))
            {
                string fileName = Path.GetFileName(this.Target);
                if (!string.IsNullOrEmpty(fileName))
                {
                    foreach (string str2 in NormalizeSearchPaths(this.Target, this.SearchPaths))
                    {
                        this.SearchForFiles(fileName, str2);
                        if ((this.SearchMode == System.Management.Automation.SearchMode.First) && (this.Result.Count > 0))
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void SearchForFiles(string pattern, string directory)
        {
            for (CultureInfo info = Thread.CurrentThread.CurrentUICulture; info != null; info = info.Parent)
            {
                string path = Path.Combine(directory, info.Name);
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path, pattern);
                    if (files == null)
                    {
                        return;
                    }
                    foreach (string str2 in files)
                    {
                        string item = Path.Combine(path, str2);
                        switch (this.SearchMode)
                        {
                            case System.Management.Automation.SearchMode.First:
                                this._result.Add(item);
                                return;

                            case System.Management.Automation.SearchMode.All:
                                this._result.Add(item);
                                break;

                            case System.Management.Automation.SearchMode.Unique:
                            {
                                string fileName = Path.GetFileName(str2);
                                string key = Path.Combine(directory, fileName);
                                if (!this._uniqueMatches.Contains(key))
                                {
                                    this._result.Add(item);
                                    this._uniqueMatches[key] = true;
                                }
                                break;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(info.Name))
                {
                    return;
                }
            }
        }

        internal Collection<string> Result
        {
            get
            {
                if (this._result == null)
                {
                    this._result = new Collection<string>();
                    this.SearchForFiles();
                }
                return this._result;
            }
        }

        internal System.Management.Automation.SearchMode SearchMode
        {
            get
            {
                return this._searchMode;
            }
        }

        internal Collection<string> SearchPaths
        {
            get
            {
                return this._searchPaths;
            }
        }

        internal string Target
        {
            get
            {
                return this._target;
            }
        }
    }
}

