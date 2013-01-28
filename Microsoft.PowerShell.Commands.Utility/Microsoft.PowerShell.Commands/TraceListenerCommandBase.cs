namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Security;

    public class TraceListenerCommandBase : TraceCommandBase
    {
        private bool debugger;
        private DefaultTraceListener defaultListener;
        private string file;
        private Collection<TextWriterTraceListener> fileListeners;
        private Collection<FileStream> fileStreams;
        private bool forceWrite;
        private bool host;
        private PSHostTraceListener hostListener;
        private string[] names = new string[0];
        private PSTraceSourceOptions options = PSTraceSourceOptions.All;
        internal bool optionsSpecified;
        private Dictionary<PSTraceSource, KeyValuePair<PSTraceSourceOptions, Collection<TraceListener>>> storedTraceSourceState = new Dictionary<PSTraceSource, KeyValuePair<PSTraceSourceOptions, Collection<TraceListener>>>();
        private TraceOptions traceOptions;
        internal bool traceOptionsSpecified;

        private static void AddListenerToSources(Collection<PSTraceSource> matchingSources, TraceListener listener)
        {
            foreach (PSTraceSource source in matchingSources)
            {
                source.Listeners.Add(listener);
            }
        }

        internal void AddTraceListenersToSources(Collection<PSTraceSource> matchingSources)
        {
            if (this.DebuggerListener)
            {
                if (this.defaultListener == null)
                {
                    this.defaultListener = new DefaultTraceListener();
                    this.defaultListener.Name = "Debug";
                }
                AddListenerToSources(matchingSources, this.defaultListener);
            }
            if (this.PSHostListener != 0)
            {
                if (this.hostListener == null)
                {
                    ((MshCommandRuntime) base.CommandRuntime).DebugPreference = ActionPreference.Continue;
                    this.hostListener = new PSHostTraceListener(this);
                    this.hostListener.Name = "Host";
                }
                AddListenerToSources(matchingSources, this.hostListener);
            }
            if (this.FileListener != null)
            {
                if (this.fileListeners == null)
                {
                    this.fileListeners = new Collection<TextWriterTraceListener>();
                    this.fileStreams = new Collection<FileStream>();
                    Exception exception = null;
                    try
                    {
                        Collection<string> resolvedProviderPathFromPSPath = new Collection<string>();
                        try
                        {
                            ProviderInfo provider = null;
                            resolvedProviderPathFromPSPath = base.SessionState.Path.GetResolvedProviderPathFromPSPath(this.file, out provider);
                            if (!provider.NameEquals(base.Context.ProviderNames.FileSystem))
                            {
                                throw new PSNotSupportedException(StringUtil.Format(TraceCommandStrings.TraceFileOnly, this.file, provider.FullName));
                            }
                        }
                        catch (ItemNotFoundException)
                        {
                            PSDriveInfo drive = null;
                            ProviderInfo info3 = null;
                            string item = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(this.file, new CmdletProviderContext(base.Context), out info3, out drive);
                            if (!info3.NameEquals(base.Context.ProviderNames.FileSystem))
                            {
                                throw new PSNotSupportedException(StringUtil.Format(TraceCommandStrings.TraceFileOnly, this.file, info3.FullName));
                            }
                            resolvedProviderPathFromPSPath.Add(item);
                        }
                        if (resolvedProviderPathFromPSPath.Count > 1)
                        {
                            throw new PSNotSupportedException(StringUtil.Format(TraceCommandStrings.TraceSingleFileOnly, this.file));
                        }
                        string path = resolvedProviderPathFromPSPath[0];
                        Exception exception2 = null;
                        try
                        {
                            if (this.ForceWrite && File.Exists(path))
                            {
                                FileInfo info4 = new FileInfo(path);
                                if ((info4 != null) && ((info4.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly))
                                {
                                    info4.Attributes &= ~System.IO.FileAttributes.ReadOnly;
                                }
                            }
                            FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                            this.fileStreams.Add(stream);
                            TextWriterTraceListener listener = new TextWriterTraceListener(stream, path) {
                                Name = this.file
                            };
                            this.fileListeners.Add(listener);
                        }
                        catch (IOException exception3)
                        {
                            exception2 = exception3;
                        }
                        catch (SecurityException exception4)
                        {
                            exception2 = exception4;
                        }
                        catch (UnauthorizedAccessException exception5)
                        {
                            exception2 = exception5;
                        }
                        if (exception2 != null)
                        {
                            ErrorRecord errorRecord = new ErrorRecord(exception2, "FileListenerPathResolutionFailed", ErrorCategory.OpenError, path);
                            base.WriteError(errorRecord);
                        }
                    }
                    catch (ProviderNotFoundException exception6)
                    {
                        exception = exception6;
                    }
                    catch (System.Management.Automation.DriveNotFoundException exception7)
                    {
                        exception = exception7;
                    }
                    catch (NotSupportedException exception8)
                    {
                        exception = exception8;
                    }
                    if (exception != null)
                    {
                        ErrorRecord record2 = new ErrorRecord(exception, "FileListenerPathResolutionFailed", ErrorCategory.InvalidArgument, this.file);
                        base.WriteError(record2);
                    }
                }
                foreach (TraceListener listener2 in this.fileListeners)
                {
                    AddListenerToSources(matchingSources, listener2);
                }
            }
        }

        protected void ClearStoredState()
        {
            foreach (KeyValuePair<PSTraceSourceOptions, Collection<TraceListener>> pair in this.storedTraceSourceState.Values)
            {
                foreach (TraceListener listener in pair.Value)
                {
                    listener.Flush();
                    listener.Close();
                }
            }
            this.storedTraceSourceState.Clear();
        }

        internal Collection<PSTraceSource> ConfigureTraceSource(string[] sourceNames, bool preConfigure, out Collection<PSTraceSource> preconfiguredSources)
        {
            preconfiguredSources = new Collection<PSTraceSource>();
            Collection<string> notMatched = null;
            Collection<PSTraceSource> matchingSources = base.GetMatchingTraceSource(sourceNames, false, out notMatched);
            if (preConfigure)
            {
                if (this.optionsSpecified)
                {
                    this.SetFlags(matchingSources);
                }
                this.AddTraceListenersToSources(matchingSources);
                this.SetTraceListenerOptions(matchingSources);
            }
            foreach (string str in notMatched)
            {
                if (!string.IsNullOrEmpty(str) && !WildcardPattern.ContainsWildcardCharacters(str))
                {
                    PSTraceSource item = PSTraceSource.GetNewTraceSource(str, string.Empty, true);
                    preconfiguredSources.Add(item);
                }
            }
            if (preconfiguredSources.Count > 0)
            {
                if (preConfigure)
                {
                    if (this.optionsSpecified)
                    {
                        this.SetFlags(preconfiguredSources);
                    }
                    this.AddTraceListenersToSources(preconfiguredSources);
                    this.SetTraceListenerOptions(preconfiguredSources);
                }
                foreach (PSTraceSource source2 in preconfiguredSources)
                {
                    if (!PSTraceSource.PreConfiguredTraceSource.ContainsKey(source2.Name))
                    {
                        PSTraceSource.PreConfiguredTraceSource.Add(source2.Name, source2);
                    }
                }
            }
            return matchingSources;
        }

        internal static void RemoveListenersByName(Collection<PSTraceSource> matchingSources, string[] listenerNames, bool fileListenersOnly)
        {
            Collection<WildcardPattern> patterns = SessionStateUtilities.CreateWildcardsFromStrings(listenerNames, WildcardOptions.IgnoreCase);
            foreach (PSTraceSource source in matchingSources)
            {
                for (int i = source.Listeners.Count - 1; i >= 0; i--)
                {
                    TraceListener listener = source.Listeners[i];
                    if ((!fileListenersOnly || (listener is TextWriterTraceListener)) && SessionStateUtilities.MatchesAnyWildcardPattern(listener.Name, patterns, true))
                    {
                        listener.Flush();
                        listener.Close();
                        source.Listeners.RemoveAt(i);
                    }
                }
            }
        }

        internal void ResetTracing(Collection<PSTraceSource> matchingSources)
        {
            foreach (PSTraceSource source in matchingSources)
            {
                foreach (TraceListener listener in source.Listeners)
                {
                    listener.Flush();
                }
                if (this.storedTraceSourceState.ContainsKey(source))
                {
                    KeyValuePair<PSTraceSourceOptions, Collection<TraceListener>> pair = this.storedTraceSourceState[source];
                    source.Listeners.Clear();
                    foreach (TraceListener listener2 in pair.Value)
                    {
                        source.Listeners.Add(listener2);
                    }
                    source.Options = pair.Key;
                }
                else
                {
                    source.Listeners.Clear();
                    source.Options = PSTraceSourceOptions.None;
                }
            }
        }

        internal void SetFlags(Collection<PSTraceSource> matchingSources)
        {
            foreach (PSTraceSource source in matchingSources)
            {
                source.Options = this.OptionsInternal;
            }
        }

        internal void SetTraceListenerOptions(Collection<PSTraceSource> matchingSources)
        {
            if (this.traceOptionsSpecified)
            {
                foreach (PSTraceSource source in matchingSources)
                {
                    foreach (TraceListener listener in source.Listeners)
                    {
                        listener.TraceOutputOptions = this.ListenerOptionsInternal;
                    }
                }
            }
        }

        internal void TurnOnTracing(Collection<PSTraceSource> matchingSources, bool preConfigured)
        {
            foreach (PSTraceSource source in matchingSources)
            {
                if (!this.storedTraceSourceState.ContainsKey(source))
                {
                    Collection<TraceListener> collection = new Collection<TraceListener>();
                    foreach (TraceListener listener in source.Listeners)
                    {
                        collection.Add(listener);
                    }
                    if (preConfigured)
                    {
                        this.storedTraceSourceState[source] = new KeyValuePair<PSTraceSourceOptions, Collection<TraceListener>>(PSTraceSourceOptions.None, new Collection<TraceListener>());
                    }
                    else
                    {
                        this.storedTraceSourceState[source] = new KeyValuePair<PSTraceSourceOptions, Collection<TraceListener>>(source.Options, collection);
                    }
                }
                source.Options = this.OptionsInternal;
            }
            this.AddTraceListenersToSources(matchingSources);
            this.SetTraceListenerOptions(matchingSources);
        }

        internal bool DebuggerListener
        {
            get
            {
                return this.debugger;
            }
            set
            {
                this.debugger = value;
            }
        }

        internal string FileListener
        {
            get
            {
                return this.file;
            }
            set
            {
                this.file = value;
            }
        }

        internal Collection<FileStream> FileStreams
        {
            get
            {
                return this.fileStreams;
            }
        }

        public bool ForceWrite
        {
            get
            {
                return this.forceWrite;
            }
            set
            {
                this.forceWrite = value;
            }
        }

        internal TraceOptions ListenerOptionsInternal
        {
            get
            {
                return this.traceOptions;
            }
            set
            {
                this.traceOptionsSpecified = true;
                this.traceOptions = value;
            }
        }

        internal string[] NameInternal
        {
            get
            {
                return this.names;
            }
            set
            {
                this.names = value;
            }
        }

        internal PSTraceSourceOptions OptionsInternal
        {
            get
            {
                return this.options;
            }
            set
            {
                this.options = value;
                this.optionsSpecified = true;
            }
        }

        internal SwitchParameter PSHostListener
        {
            get
            {
                return this.host;
            }
            set
            {
                this.host = (bool) value;
            }
        }
    }
}

