namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Xml;

    public class PSTraceSource
    {
        private bool alreadyTracing;
        private const string constructorLeavingFormatter = "Leave Ctor {0}";
        private const string constructorOutputFormatter = "Enter Ctor {0}";
        private const string delegateHandlerLeavingFormatter = "Leave delegate handler: {0}";
        private const string delegateHandlerOutputFormatter = "Enter delegate handler: {0}:";
        private string description = string.Empty;
        private const string disposeLeavingFormatter = "Leave Disposer {0}";
        private const string disposeOutputFormatter = "Enter Disposer {0}";
        private const string errorFormatter = "ERROR: ";
        private const string eventHandlerLeavingFormatter = "Leave event handler: {0}";
        private const string eventHandlerOutputFormatter = "Enter event handler: {0}:";
        private const string exceptionOutputFormatter = "{0}: {1}\n{2}";
        private PSTraceSourceOptions flags;
        private string fullName = string.Empty;
        private static object getTracerLock = new object();
        private static bool globalTraceInitialized;
        private const string innermostExceptionOutputFormatter = "Inner-most {0}: {1}\n{2}";
        private const string lockAcquiringFormatter = "Acquiring Lock: {0}";
        private const string lockEnterFormatter = "Enter Lock: {0}";
        private const string lockLeavingFormatter = "Leave Lock: {0}";
        private const string methodLeavingFormatter = "Leave {0}";
        private const string methodOutputFormatter = "Enter {0}:";
        private string name;
        private static Dictionary<string, PSTraceSource> preConfiguredTraceSource = new Dictionary<string, PSTraceSource>(StringComparer.OrdinalIgnoreCase);
        private const string propertyLeavingFormatter = "Leave property {0}";
        private const string propertyOutputFormatter = "Enter property {0}:";
        private bool showHeaders = true;
        private static LocalDataStoreSlot threadIndentLevel = Thread.AllocateDataSlot();
        private static Dictionary<string, PSTraceSource> traceCatalog = new Dictionary<string, PSTraceSource>(StringComparer.OrdinalIgnoreCase);
        private System.Diagnostics.TraceSource traceSource;
        private const string verboseFormatter = "Verbose: ";
        private const string warningFormatter = "Warning: ";
        private const string writeLineFormatter = "";

		static PSTraceSource ()
		{
			if (OSHelper.IsUnix) {
				Environment.SetEnvironmentVariable ("MONO_EVENTLOG_TYPE", "local");
			}
		}

        internal PSTraceSource(string fullName, string name, string description, bool traceHeaders)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                throw new ArgumentNullException("fullName");
            }
            try
            {
                this.fullName = fullName;
                this.name = name;
                if (string.Equals(Environment.GetEnvironmentVariable("MshEnableTrace"), "True", StringComparison.OrdinalIgnoreCase))
                {
                    string str2 = this.TraceSource.Attributes["Options"];
                    if (str2 != null)
                    {
                        this.flags = (PSTraceSourceOptions) Enum.Parse(typeof(PSTraceSourceOptions), str2, true);
                    }
                }
                this.showHeaders = traceHeaders;
                this.description = description;
            }
            catch (XmlException)
            {
                this.flags = PSTraceSourceOptions.None;
            }
            catch (ConfigurationException)
            {
                this.flags = PSTraceSourceOptions.None;
            }
        }

        private static void AddTab(ref StringBuilder lineBuilder)
        {
            int indentSize = Trace.IndentSize;
            int threadIndentLevel = ThreadIndentLevel;
            for (int i = 0; i < (indentSize * threadIndentLevel); i++)
            {
                lineBuilder.Append(" ");
            }
        }

        private void FormatOutputLine(PSTraceSourceOptions flag, string classFormatter, string format, params object[] args)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                if (classFormatter != null)
                {
                    builder.Append(classFormatter);
                }
                if (format != null)
                {
                    builder.AppendFormat(Thread.CurrentThread.CurrentCulture, format, args);
                }
                this.OutputLine(flag, builder.ToString(), new object[0]);
            }
            catch
            {
            }
        }

        private static string GetCallingMethodNameAndParameters(int skipFrames)
        {
            StringBuilder builder = null;
            try
            {
                MethodBase method = new StackFrame(++skipFrames).GetMethod();
                Type declaringType = method.DeclaringType;
                builder = new StringBuilder();
                builder.AppendFormat(Thread.CurrentThread.CurrentCulture, "{0}.{1}(", new object[] { declaringType.Name, method.Name });
                builder.Append(")");
            }
            catch
            {
            }
            return builder.ToString();
        }

        private static StringBuilder GetLinePrefix(PSTraceSourceOptions flag)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(Thread.CurrentThread.CurrentCulture, " {0,-11} ", new object[] { Enum.GetName(typeof(PSTraceSourceOptions), flag) });
            return builder;
        }

        internal static PSTraceSource GetNewTraceSource(string name, string description, bool traceHeaders)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name");
            }
            return new PSTraceSource(name, name, description, traceHeaders);
        }

        internal static PSTraceSource GetTracer(string name, string description)
        {
            return GetTracer(name, description, true);
        }

        internal static PSTraceSource GetTracer(string name, string description, bool traceHeaders)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            lock (getTracerLock)
            {
                PSTraceSource source = null;
                if (TraceCatalog.ContainsKey(name))
                {
                    source = TraceCatalog[name];
                }
                if (source == null)
                {
                    string key = name;
                    if (!PreConfiguredTraceSource.ContainsKey(key))
                    {
                        if (key.Length > 0x10)
                        {
                            key = key.Substring(0, 0x10);
                            if (!PreConfiguredTraceSource.ContainsKey(key))
                            {
                                key = null;
                            }
                        }
                        else
                        {
                            key = null;
                        }
                    }
                    if (key != null)
                    {
                        PSTraceSource source2 = PreConfiguredTraceSource[key];
                        source = GetNewTraceSource(key, description, traceHeaders);
                        source.Options = source2.Options;
                        source.Listeners.Clear();
                        source.Listeners.AddRange(source2.Listeners);
                        TraceCatalog.Add(key, source);
                        PreConfiguredTraceSource.Remove(key);
                    }
                }
                if (source == null)
                {
                    source = GetNewTraceSource(name, description, traceHeaders);
                    TraceCatalog[source.FullName] = source;
                }
                if ((source.Options != PSTraceSourceOptions.None) && traceHeaders)
                {
                    source.TraceGlobalAppDomainHeader();
                    source.TracerObjectHeader(Assembly.GetCallingAssembly());
                }
                return source;
            }
        }

        internal static PSArgumentException NewArgumentException(string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                throw new ArgumentNullException("paramName");
            }
            return new PSArgumentException(System.Management.Automation.Internal.StringUtil.Format(AutomationExceptions.Argument, paramName), paramName);
        }

        internal static PSArgumentException NewArgumentException(string paramName, string baseName, string resourceId, params object[] args)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                throw NewArgumentNullException("paramName");
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw NewArgumentNullException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw NewArgumentNullException("resourceId");
            }
            return new PSArgumentException(ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args), paramName);
        }

        internal static PSArgumentNullException NewArgumentNullException(string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                throw new ArgumentNullException("paramName");
            }
            return new PSArgumentNullException(paramName, System.Management.Automation.Internal.StringUtil.Format(AutomationExceptions.ArgumentNull, paramName));
        }

        internal static PSArgumentNullException NewArgumentNullException(string paramName, string baseName, string resourceId, params object[] args)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                throw NewArgumentNullException("paramName");
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw NewArgumentNullException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw NewArgumentNullException("resourceId");
            }
            return new PSArgumentNullException(paramName, ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args));
        }

        internal static PSArgumentOutOfRangeException NewArgumentOutOfRangeException(string paramName, object actualValue)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                throw new ArgumentNullException("paramName");
            }
            return new PSArgumentOutOfRangeException(paramName, actualValue, System.Management.Automation.Internal.StringUtil.Format(AutomationExceptions.ArgumentOutOfRange, paramName));
        }

        internal static PSArgumentOutOfRangeException NewArgumentOutOfRangeException(string paramName, object actualValue, string baseName, string resourceId, params object[] args)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                throw NewArgumentNullException("paramName");
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw NewArgumentNullException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw NewArgumentNullException("resourceId");
            }
            return new PSArgumentOutOfRangeException(paramName, actualValue, ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args));
        }

        internal static PSInvalidOperationException NewInvalidOperationException()
        {
            return new PSInvalidOperationException(System.Management.Automation.Internal.StringUtil.Format(AutomationExceptions.InvalidOperation, new StackTrace().GetFrame(1).GetMethod().Name));
        }

        internal static PSInvalidOperationException NewInvalidOperationException(string baseName, string resourceId, params object[] args)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                throw NewArgumentNullException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw NewArgumentNullException("resourceId");
            }
            return new PSInvalidOperationException(ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args));
        }

        internal static PSInvalidOperationException NewInvalidOperationException(Exception innerException, string baseName, string resourceId, params object[] args)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                throw NewArgumentNullException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw NewArgumentNullException("resourceId");
            }
            return new PSInvalidOperationException(ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args), innerException);
        }

        internal static PSNotImplementedException NewNotImplementedException()
        {
            return new PSNotImplementedException(System.Management.Automation.Internal.StringUtil.Format(AutomationExceptions.NotImplemented, new StackTrace().GetFrame(0).ToString()));
        }

        internal static PSNotSupportedException NewNotSupportedException()
        {
            return new PSNotSupportedException(System.Management.Automation.Internal.StringUtil.Format(AutomationExceptions.NotSupported, new StackTrace().GetFrame(0).ToString()));
        }

        internal static PSNotSupportedException NewNotSupportedException(string baseName, string resourceId, params object[] args)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                throw NewArgumentNullException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw NewArgumentNullException("resourceId");
            }
            return new PSNotSupportedException(ResourceManagerCache.FormatResourceString(Assembly.GetCallingAssembly(), baseName, resourceId, args));
        }

        internal static PSObjectDisposedException NewObjectDisposedException(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                throw NewArgumentNullException("objectName");
            }
            return new PSObjectDisposedException(objectName, System.Management.Automation.Internal.StringUtil.Format(AutomationExceptions.ObjectDisposed, objectName));
        }

        internal void OutputLine(PSTraceSourceOptions flag, string format, params object[] args)
        {
            if (!this.alreadyTracing)
            {
                this.alreadyTracing = true;
                try
                {
                    StringBuilder lineBuilder = new StringBuilder();
                    if (this.showHeaders)
                    {
                        lineBuilder.Append(GetLinePrefix(flag));
                    }
                    AddTab(ref lineBuilder);
                    if ((args != null) && (args.Length > 0))
                    {
                        for (int i = 0; i < args.Length; i++)
                        {
                            if (args[i] == null)
                            {
                                args[i] = "null";
                            }
                        }
                        lineBuilder.AppendFormat(Thread.CurrentThread.CurrentCulture, format, args);
                    }
                    else
                    {
                        lineBuilder.Append(format);
                    }
                    this.TraceSource.TraceInformation(lineBuilder.ToString());
                }
                finally
                {
                    this.alreadyTracing = false;
                }
            }
        }

        internal void TraceError(string errorMessageFormat, params object[] args)
        {
            if ((this.flags & PSTraceSourceOptions.Error) != PSTraceSourceOptions.None)
            {
                this.FormatOutputLine(PSTraceSourceOptions.Error, "ERROR: ", errorMessageFormat, args);
            }
        }

        internal IDisposable TraceEventHandlers()
        {
            if ((this.flags & PSTraceSourceOptions.Events) != PSTraceSourceOptions.None)
            {
                try
                {
                    return new ScopeTracer(this, PSTraceSourceOptions.Events, "Enter event handler: {0}:", "Leave event handler: {0}", GetCallingMethodNameAndParameters(1), "", new object[0]);
                }
                catch
                {
                }
            }
            return null;
        }

        internal IDisposable TraceEventHandlers(string format, params object[] args)
        {
            if ((this.flags & PSTraceSourceOptions.Events) != PSTraceSourceOptions.None)
            {
                try
                {
                    return new ScopeTracer(this, PSTraceSourceOptions.Events, "Enter event handler: {0}:", "Leave event handler: {0}", GetCallingMethodNameAndParameters(1), format, args);
                }
                catch
                {
                }
            }
            return null;
        }

        internal void TraceGlobalAppDomainHeader()
        {
            if (!globalTraceInitialized)
            {
                this.OutputLine(PSTraceSourceOptions.All, "Initializing tracing for AppDomain: {0}", new object[] { AppDomain.CurrentDomain.FriendlyName });
                this.OutputLine(PSTraceSourceOptions.All, "\tCurrent time: {0}", new object[] { DateTime.Now });
                this.OutputLine(PSTraceSourceOptions.All, "\tOS Build: {0}", new object[] { Environment.OSVersion.ToString() });
                this.OutputLine(PSTraceSourceOptions.All, "\tFramework Build: {0}\n", new object[] { Environment.Version.ToString() });
                globalTraceInitialized = true;
            }
        }

        internal IDisposable TraceLock(string lockName)
        {
            if ((this.flags & PSTraceSourceOptions.Lock) != PSTraceSourceOptions.None)
            {
                try
                {
                    return new ScopeTracer(this, PSTraceSourceOptions.Lock, "Enter Lock: {0}", "Leave Lock: {0}", lockName);
                }
                catch
                {
                }
            }
            return null;
        }

        internal void TraceLockAcquired(string lockName)
        {
            if ((this.flags & PSTraceSourceOptions.Lock) != PSTraceSourceOptions.None)
            {
                this.TraceLockHelper("Enter Lock: {0}", lockName);
            }
        }

        internal void TraceLockAcquiring(string lockName)
        {
            if ((this.flags & PSTraceSourceOptions.Lock) != PSTraceSourceOptions.None)
            {
                this.TraceLockHelper("Acquiring Lock: {0}", lockName);
            }
        }

        private void TraceLockHelper(string formatter, string lockName)
        {
            try
            {
                this.OutputLine(PSTraceSourceOptions.Lock, formatter, new object[] { lockName });
            }
            catch
            {
            }
        }

        internal void TraceLockReleased(string lockName)
        {
            if ((this.flags & PSTraceSourceOptions.Lock) != PSTraceSourceOptions.None)
            {
                this.TraceLockHelper("Leave Lock: {0}", lockName);
            }
        }

        internal IDisposable TraceMethod(string format, params object[] args)
        {
            if ((this.flags & PSTraceSourceOptions.Method) != PSTraceSourceOptions.None)
            {
                try
                {
                    return new ScopeTracer(this, PSTraceSourceOptions.Method, "Enter {0}:", "Leave {0}", GetCallingMethodNameAndParameters(1), format, args);
                }
                catch
                {
                }
            }
            return null;
        }

        internal void TracerObjectHeader(Assembly callingAssembly)
        {
            if (this.flags != PSTraceSourceOptions.None)
            {
                this.OutputLine(PSTraceSourceOptions.All, "Creating tracer:", new object[0]);
                this.OutputLine(PSTraceSourceOptions.All, "\tCategory: {0}", new object[] { this.Name });
                this.OutputLine(PSTraceSourceOptions.All, "\tDescription: {0}", new object[] { this.Description });
                if (callingAssembly != null)
                {
                    this.OutputLine(PSTraceSourceOptions.All, "\tAssembly: {0}", new object[] { callingAssembly.FullName });
                    this.OutputLine(PSTraceSourceOptions.All, "\tAssembly Location: {0}", new object[] { callingAssembly.Location });
                    FileInfo info = new FileInfo(callingAssembly.Location);
                    this.OutputLine(PSTraceSourceOptions.All, "\tAssembly File Timestamp: {0}", new object[] { info.CreationTime });
                }
                StringBuilder builder = new StringBuilder();
                builder.Append("\tFlags: ");
                builder.Append(this.flags.ToString());
                this.OutputLine(PSTraceSourceOptions.All, builder.ToString(), new object[0]);
            }
        }

        internal IDisposable TraceScope(string format, params object[] args)
        {
            if ((this.flags & PSTraceSourceOptions.Scope) != PSTraceSourceOptions.None)
            {
                try
                {
                    return new ScopeTracer(this, PSTraceSourceOptions.Scope, null, null, string.Empty, format, args);
                }
                catch
                {
                }
            }
            return null;
        }

        internal void TraceVerbose(string verboseMessageFormat, params object[] args)
        {
            if ((this.flags & PSTraceSourceOptions.Verbose) != PSTraceSourceOptions.None)
            {
                this.FormatOutputLine(PSTraceSourceOptions.Verbose, "Verbose: ", verboseMessageFormat, args);
            }
        }

        internal void TraceWarning(string warningMessageFormat, params object[] args)
        {
            if ((this.flags & PSTraceSourceOptions.Warning) != PSTraceSourceOptions.None)
            {
                this.FormatOutputLine(PSTraceSourceOptions.Warning, "Warning: ", warningMessageFormat, args);
            }
        }

        internal void WriteLine(object arg)
        {
            if ((this.flags & PSTraceSourceOptions.WriteLine) != PSTraceSourceOptions.None)
            {
                this.WriteLine("{0}", new object[] { (arg == null) ? "null" : arg.ToString() });
            }
        }

        internal void WriteLine(string format, params object[] args)
        {
			//DEBUG: Debug.WriteLine ("TraceSource: " + this.Name + " - " + format, args);
            if ((this.flags & PSTraceSourceOptions.WriteLine) != PSTraceSourceOptions.None)
            {
                this.FormatOutputLine(PSTraceSourceOptions.WriteLine, "", format, args);
            }
        }

        public StringDictionary Attributes
        {
            get
            {
                return this.TraceSource.Attributes;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        internal string FullName
        {
            get
            {
                return this.fullName;
            }
        }

        public TraceListenerCollection Listeners
        {
            get
            {
                return this.TraceSource.Listeners;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public PSTraceSourceOptions Options
        {
            get
            {
                return this.flags;
            }
            set
            {
                this.flags = value;
                this.TraceSource.Switch.Level = (SourceLevels) this.flags;
            }
        }

        internal static Dictionary<string, PSTraceSource> PreConfiguredTraceSource
        {
            get
            {
                return preConfiguredTraceSource;
            }
        }

        internal bool ShowHeaders
        {
            get
            {
                return this.showHeaders;
            }
            set
            {
                this.showHeaders = value;
            }
        }

        public SourceSwitch Switch
        {
            get
            {
                return this.TraceSource.Switch;
            }
            set
            {
                this.TraceSource.Switch = value;
            }
        }

        internal static int ThreadIndentLevel
        {
            get
            {
                object data = Thread.GetData(threadIndentLevel);
                if (data == null)
                {
                    int num = 0;
                    Thread.SetData(threadIndentLevel, num);
                    data = Thread.GetData(threadIndentLevel);
                }
                return (int) data;
            }
            set
            {
                if (value >= 0)
                {
                    Thread.SetData(threadIndentLevel, value);
                }
            }
        }

        internal static Dictionary<string, PSTraceSource> TraceCatalog
        {
            get
            {
                return traceCatalog;
            }
        }

        internal System.Diagnostics.TraceSource TraceSource
        {
            get
            {
                if (this.traceSource == null)
                {
                    this.traceSource = new MonadTraceSource(this.name);
                }
                return this.traceSource;
            }
        }
    }
}

