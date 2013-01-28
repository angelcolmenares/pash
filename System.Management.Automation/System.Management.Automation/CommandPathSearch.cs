namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;

    internal class CommandPathSearch : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
    {
        private ExecutionContext _context;
        private bool allowAnyExtension;
        private HashSet<string> allowedExtensions;
        private Collection<string> currentDirectoryResults;
        private IEnumerator<string> currentDirectoryResultsEnumerator;
        private bool justReset;
        private LookupPathCollection lookupPaths;
        private IEnumerator<string> lookupPathsEnumerator;
        private IEnumerator<string> patternEnumerator;
        private IEnumerable<string> patterns;
        [TraceSource("CommandSearch", "CommandSearch")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("CommandSearch", "CommandSearch");

        internal CommandPathSearch(IEnumerable<string> patterns, IEnumerable<string> lookupPaths, ExecutionContext context) : this(patterns, lookupPaths, null, true, context)
        {
        }

        internal CommandPathSearch(IEnumerable<string> patterns, IEnumerable<string> lookupPaths, HashSet<string> allowedExtensions, bool allowAnyExtension, ExecutionContext context)
        {
            if (patterns == null)
            {
                throw PSTraceSource.NewArgumentNullException("patterns");
            }
            if (lookupPaths == null)
            {
                throw PSTraceSource.NewArgumentNullException("lookupPaths");
            }
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            this._context = context;
            this.patterns = patterns;
            this.lookupPaths = new LookupPathCollection(lookupPaths);
            this.ResolveCurrentDirectoryInLookupPaths();
            this.Reset();
            this.allowAnyExtension = allowAnyExtension;
            this.allowedExtensions = allowedExtensions;
        }

        public void Dispose()
        {
            this.Reset();
            GC.SuppressFinalize(this);
        }

        private static Collection<string> GetMatchingPathsInDirectory(string pattern, string directory, bool allowAnyExtension, HashSet<string> allowedExtensions)
        {
            Collection<string> collection = new Collection<string>();
            try
            {
                CommandDiscovery.discoveryTracer.WriteLine("Looking for {0} in {1}", new object[] { pattern, directory });
                if (!Directory.Exists(directory))
                {
                    return collection;
                }
                string[] files = null;
                if (!".".Equals(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    files = Directory.GetFiles(directory, pattern);
                }
                List<string> list = null;
                if (files != null)
                {
                    list = new List<string>();
                    if (allowAnyExtension)
                    {
                        list.AddRange(files);
                    }
                    else
                    {
                        foreach (string str in files)
                        {
                            string extension = Path.GetExtension(str);
                            if (!string.IsNullOrEmpty(extension) && allowedExtensions.Contains(extension))
                            {
                                list.Add(str);
                            }
                        }
                    }
                }
                collection = SessionStateUtilities.ConvertListToCollection<string>(list);
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (NotSupportedException)
            {
            }
            return collection;
        }

        private void GetNewDirectoryResults(string pattern, string directory)
        {
            this.currentDirectoryResults = GetMatchingPathsInDirectory(pattern, directory, this.allowAnyExtension, this.allowedExtensions);
            this.currentDirectoryResultsEnumerator = this.currentDirectoryResults.GetEnumerator();
        }

        public bool MoveNext()
        {
            bool flag = false;
            if (this.justReset)
            {
                this.justReset = false;
                if (!this.patternEnumerator.MoveNext())
                {
                    tracer.TraceError("No patterns were specified", new object[0]);
                    return false;
                }
                if (!this.lookupPathsEnumerator.MoveNext())
                {
                    tracer.TraceError("No lookup paths were specified", new object[0]);
                    return false;
                }
                this.GetNewDirectoryResults(this.patternEnumerator.Current, this.lookupPathsEnumerator.Current);
            }
        Label_0075:
            if (!this.currentDirectoryResultsEnumerator.MoveNext())
            {
                tracer.WriteLine("Current directory results are invalid", new object[0]);
                if (!this.patternEnumerator.MoveNext())
                {
                    tracer.WriteLine("Current patterns exhausted in current directory: {0}", new object[] { this.lookupPathsEnumerator.Current });
                    goto Label_0118;
                }
                this.GetNewDirectoryResults(this.patternEnumerator.Current, this.lookupPathsEnumerator.Current);
            }
            else
            {
                tracer.WriteLine("Next path found: {0}", new object[] { this.currentDirectoryResultsEnumerator.Current });
                flag = true;
                goto Label_0118;
            }
            if (!flag)
            {
                goto Label_0075;
            }
        Label_0118:
            if (!flag)
            {
                if (!this.lookupPathsEnumerator.MoveNext())
                {
                    tracer.WriteLine("All lookup paths exhausted, no more matches can be found", new object[0]);
                }
                else
                {
                    this.patternEnumerator = this.patterns.GetEnumerator();
                    if (!this.patternEnumerator.MoveNext())
                    {
                        tracer.WriteLine("All patterns exhausted, no more matches can be found", new object[0]);
                    }
                    else
                    {
                        this.GetNewDirectoryResults(this.patternEnumerator.Current, this.lookupPathsEnumerator.Current);
                        if (!flag)
                        {
                            goto Label_0075;
                        }
                    }
                }
            }
            tracer.WriteLine("result = {0}", new object[] { flag });
            return flag;
        }

        public void Reset()
        {
            this.lookupPathsEnumerator = this.lookupPaths.GetEnumerator();
            this.patternEnumerator = this.patterns.GetEnumerator();
            this.currentDirectoryResults = new Collection<string>();
            this.currentDirectoryResultsEnumerator = this.currentDirectoryResults.GetEnumerator();
            this.justReset = true;
        }

        private void ResolveCurrentDirectoryInLookupPaths()
        {
            SortedList list = new SortedList();
            int num = 0;
            string fileSystem = this._context.ProviderNames.FileSystem;
            SessionStateInternal engineSessionState = this._context.EngineSessionState;
            bool flag = ((engineSessionState.CurrentDrive != null) && engineSessionState.CurrentDrive.Provider.NameEquals(fileSystem)) && engineSessionState.IsProviderLoaded(fileSystem);
            string currentDirectory = Environment.CurrentDirectory;
			if (string.IsNullOrEmpty (currentDirectory) && OSHelper.IsUnix) currentDirectory = "/";

            LocationGlobber locationGlobber = this._context.LocationGlobber;
            foreach (int num2 in this.lookupPaths.IndexOfRelativePath())
            {
                string item = null;
                string providerPath = null;
                CommandDiscovery.discoveryTracer.WriteLine("Lookup directory \"{0}\" appears to be a relative path. Attempting resolution...", new object[] { this.lookupPaths[num2] });
                if (flag)
                {
                    ProviderInfo provider = null;
                    try
                    {
                        providerPath = locationGlobber.GetProviderPath(this.lookupPaths[num2], out provider);
                    }
                    catch (ProviderInvocationException exception)
                    {
                        CommandDiscovery.discoveryTracer.WriteLine("The relative path '{0}', could not be resolved because the provider threw an exception: '{1}'", new object[] { this.lookupPaths[num2], exception.Message });
                    }
                    catch (InvalidOperationException)
                    {
                        CommandDiscovery.discoveryTracer.WriteLine("The relative path '{0}', could not resolve a home directory for the provider", new object[] { this.lookupPaths[num2] });
                    }
                    if (!string.IsNullOrEmpty(providerPath))
                    {
                        CommandDiscovery.discoveryTracer.TraceError("The relative path resolved to: {0}", new object[] { providerPath });
                        item = providerPath;
                    }
                    else
                    {
                        CommandDiscovery.discoveryTracer.WriteLine("The relative path was not a file system path. {0}", new object[] { this.lookupPaths[num2] });
                    }
                }
                else
                {
                    CommandDiscovery.discoveryTracer.TraceWarning("The current drive is not set, using the process current directory: {0}", new object[] { currentDirectory });
                    item = currentDirectory;
                }
                if (item != null)
                {
                    int index = this.lookupPaths.IndexOf(item);
                    if (index == -1)
                    {
                        this.lookupPaths[num2] = item;
                    }
                    else if (index > num2)
                    {
                        list.Add(num++, index);
                        this.lookupPaths[num2] = item;
                    }
                    else
                    {
                        list.Add(num++, num2);
                    }
                }
                else
                {
                    list.Add(num++, num2);
                }
            }
            for (int i = list.Count; i > 0; i--)
            {
                int num5 = (int) list[i - 1];
                this.lookupPaths.RemoveAt(num5);
            }
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        string IEnumerator<string>.Current
        {
            get
            {
                if (this.currentDirectoryResults == null)
                {
                    throw PSTraceSource.NewInvalidOperationException();
                }
                return this.currentDirectoryResultsEnumerator.Current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        public string Current
        {
            get { return this.currentDirectoryResultsEnumerator.Current; }
        }

    }
}

