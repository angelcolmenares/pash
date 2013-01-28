namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class MshConsoleInfo
    {
        private static readonly PSTraceSource _mshsnapinTracer = PSTraceSource.GetTracer("MshSnapinLoadUnload", "Loading and unloading mshsnapins", false);
        internal const string ConsoleInfoResourceBaseName = "ConsoleInfoErrorStrings";
        private Collection<PSSnapInInfo> defaultPSSnapIns;
        private readonly Collection<PSSnapInInfo> externalPSSnapIns;
        private string fileName;
        private bool isDirty;
        private readonly Version psVersion;

        private MshConsoleInfo(Version version)
        {
            this.psVersion = version;
            this.isDirty = false;
            this.fileName = null;
            this.defaultPSSnapIns = new Collection<PSSnapInInfo>();
            this.externalPSSnapIns = new Collection<PSSnapInInfo>();
        }

        internal PSSnapInInfo AddPSSnapIn(string mshSnapInID)
        {
            if (string.IsNullOrEmpty(mshSnapInID))
            {
                PSTraceSource.NewArgumentNullException("mshSnapInID");
            }
            if (IsDefaultPSSnapIn(mshSnapInID, this.defaultPSSnapIns))
            {
                _mshsnapinTracer.TraceError("MshSnapin {0} can't be added since it is a default mshsnapin", new object[] { mshSnapInID });
                throw PSTraceSource.NewArgumentException("mshSnapInID", "ConsoleInfoErrorStrings", "CannotLoadDefault", new object[0]);
            }
            if (this.IsActiveExternalPSSnapIn(mshSnapInID))
            {
                _mshsnapinTracer.TraceError("MshSnapin {0} is already loaded.", new object[] { mshSnapInID });
                throw PSTraceSource.NewArgumentException("mshSnapInID", "ConsoleInfoErrorStrings", "PSSnapInAlreadyExists", new object[] { mshSnapInID });
            }
            PSSnapInInfo item = PSSnapInReader.Read(this.MajorVersion, mshSnapInID);
            if (!Utils.IsPSVersionSupported(item.PSVersion.ToString()))
            {
                _mshsnapinTracer.TraceError("MshSnapin {0} and current monad engine's versions don't match.", new object[] { mshSnapInID });
                throw PSTraceSource.NewArgumentException("mshSnapInID", "ConsoleInfoErrorStrings", "AddPSSnapInBadMonadVersion", new object[] { item.PSVersion.ToString(), this.psVersion.ToString() });
            }
            this.externalPSSnapIns.Add(item);
            _mshsnapinTracer.WriteLine("MshSnapin {0} successfully added to consoleinfo list.", new object[] { mshSnapInID });
            this.isDirty = true;
            return item;
        }

        internal static MshConsoleInfo CreateDefaultConfiguration()
        {
            MshConsoleInfo info = new MshConsoleInfo(PSVersionInfo.PSVersion);
            try
            {
                info.defaultPSSnapIns = PSSnapInReader.ReadEnginePSSnapIns();
            }
            catch (PSArgumentException exception)
            {
                string cannotLoadDefaults = ConsoleInfoErrorStrings.CannotLoadDefaults;
                _mshsnapinTracer.TraceError(cannotLoadDefaults, new object[0]);
                throw new PSSnapInException(cannotLoadDefaults, exception);
            }
            catch (SecurityException exception2)
            {
                string errorMessageFormat = ConsoleInfoErrorStrings.CannotLoadDefaults;
                _mshsnapinTracer.TraceError(errorMessageFormat, new object[0]);
                throw new PSSnapInException(errorMessageFormat, exception2);
            }
            return info;
        }

        internal static MshConsoleInfo CreateFromConsoleFile(string fileName, out PSConsoleLoadException cle)
        {
            _mshsnapinTracer.WriteLine("Creating console info from file {0}", new object[] { fileName });
            MshConsoleInfo info = CreateDefaultConfiguration();
            string fullPath = Path.GetFullPath(fileName);
            info.fileName = fullPath;
            info.Load(fullPath, out cle);
            _mshsnapinTracer.WriteLine("Console info created successfully", new object[0]);
            return info;
        }

        internal Collection<PSSnapInInfo> GetPSSnapIn(string pattern, bool searchRegistry)
        {
            bool flag = WildcardPattern.ContainsWildcardCharacters(pattern);
            if (!flag)
            {
                PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(pattern);
            }
            Collection<PSSnapInInfo> collection = searchRegistry ? PSSnapInReader.ReadAll() : this.PSSnapIns;
            Collection<PSSnapInInfo> collection2 = new Collection<PSSnapInInfo>();
            if (collection != null)
            {
                if (!flag)
                {
                    foreach (PSSnapInInfo info in collection)
                    {
                        if (string.Equals(info.Name, pattern, StringComparison.OrdinalIgnoreCase))
                        {
                            collection2.Add(info);
                        }
                    }
                    return collection2;
                }
                WildcardPattern pattern2 = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
                foreach (PSSnapInInfo info2 in collection)
                {
                    if (pattern2.IsMatch(info2.Name))
                    {
                        collection2.Add(info2);
                    }
                }
            }
            return collection2;
        }

        private bool IsActiveExternalPSSnapIn(string mshSnapInID)
        {
            foreach (PSSnapInInfo info in this.externalPSSnapIns)
            {
                if (string.Equals(mshSnapInID, info.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsDefaultPSSnapIn(string mshSnapInID, IEnumerable<PSSnapInInfo> defaultSnapins)
        {
            foreach (PSSnapInInfo info in defaultSnapins)
            {
                if (string.Equals(mshSnapInID, info.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private Collection<PSSnapInInfo> Load(string path, out PSConsoleLoadException cle)
        {
            cle = null;
            _mshsnapinTracer.WriteLine("Load mshsnapins from console file {0}", new object[] { path });
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (!Path.IsPathRooted(path))
            {
                _mshsnapinTracer.TraceError("Console file {0} needs to be a absolute path.", new object[] { path });
                throw PSTraceSource.NewArgumentException("path", "ConsoleInfoErrorStrings", "PathNotAbsolute", new object[] { path });
            }
            if (!path.EndsWith(".psc1", StringComparison.OrdinalIgnoreCase))
            {
                _mshsnapinTracer.TraceError("Console file {0} needs to have {1} extension.", new object[] { path, ".psc1" });
                throw PSTraceSource.NewArgumentException("path", "ConsoleInfoErrorStrings", "BadConsoleExtension", new object[] { "" });
            }
            PSConsoleFileElement element = PSConsoleFileElement.CreateFromFile(path);
            if (!Utils.IsPSVersionSupported(element.MonadVersion))
            {
                _mshsnapinTracer.TraceError("Console version {0} is not supported in current monad session.", new object[] { element.MonadVersion });
                throw PSTraceSource.NewArgumentException("PSVersion", "ConsoleInfoErrorStrings", "BadMonadVersion", new object[] { element.MonadVersion, this.psVersion.ToString() });
            }
            Collection<PSSnapInException> exceptions = new Collection<PSSnapInException>();
            foreach (string str in element.PSSnapIns)
            {
                try
                {
                    this.AddPSSnapIn(str);
                }
                catch (PSArgumentException exception)
                {
                    PSSnapInException item = new PSSnapInException(str, exception.Message, exception);
                    exceptions.Add(item);
                }
                catch (SecurityException exception3)
                {
                    string pSSnapInReadError = ConsoleInfoErrorStrings.PSSnapInReadError;
                    PSSnapInException exception4 = new PSSnapInException(str, pSSnapInReadError, exception3);
                    exceptions.Add(exception4);
                }
            }
            if (exceptions.Count > 0)
            {
                cle = new PSConsoleLoadException(this, exceptions);
            }
            this.isDirty = false;
            return this.externalPSSnapIns;
        }

        private Collection<PSSnapInInfo> MergeDefaultExternalMshSnapins()
        {
            Collection<PSSnapInInfo> collection = new Collection<PSSnapInInfo>();
            foreach (PSSnapInInfo info in this.defaultPSSnapIns)
            {
                collection.Add(info);
            }
            foreach (PSSnapInInfo info2 in this.externalPSSnapIns)
            {
                collection.Add(info2);
            }
            return collection;
        }

        internal PSSnapInInfo RemovePSSnapIn(string mshSnapInID)
        {
            if (string.IsNullOrEmpty(mshSnapInID))
            {
                PSTraceSource.NewArgumentNullException("mshSnapInID");
            }
            PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(mshSnapInID);
            PSSnapInInfo info = null;
            foreach (PSSnapInInfo info2 in this.externalPSSnapIns)
            {
                if (string.Equals(mshSnapInID, info2.Name, StringComparison.OrdinalIgnoreCase))
                {
                    info = info2;
                    this.externalPSSnapIns.Remove(info2);
                    this.isDirty = true;
                    break;
                }
            }
            if (info != null)
            {
                return info;
            }
            if (IsDefaultPSSnapIn(mshSnapInID, this.defaultPSSnapIns))
            {
                _mshsnapinTracer.WriteLine("MshSnapin {0} can't be removed since it is a default mshsnapin.", new object[] { mshSnapInID });
                throw PSTraceSource.NewArgumentException("mshSnapInID", "ConsoleInfoErrorStrings", "CannotRemoveDefault", new object[] { mshSnapInID });
            }
            throw PSTraceSource.NewArgumentException("mshSnapInID", "ConsoleInfoErrorStrings", "CannotRemovePSSnapIn", new object[] { mshSnapInID });
        }

        internal void Save()
        {
            if (this.fileName == null)
            {
                throw PSTraceSource.NewInvalidOperationException("ConsoleInfoErrorStrings", "SaveDefaultError", new object[] { "" });
            }
            PSConsoleFileElement.WriteToFile(this.fileName, this.PSVersion, this.ExternalPSSnapIns);
            this.isDirty = false;
        }

        internal void SaveAsConsoleFile(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            string fullPath = path;
            if (!Path.IsPathRooted(fullPath))
            {
                fullPath = Path.GetFullPath(this.fileName);
            }
            if (!fullPath.EndsWith(".psc1", StringComparison.OrdinalIgnoreCase))
            {
                _mshsnapinTracer.TraceError("Console file {0} doesn't have the right extension {1}.", new object[] { path, ".psc1" });
                throw PSTraceSource.NewArgumentException("absolutePath", "ConsoleInfoErrorStrings", "BadConsoleExtension", new object[] { "" });
            }
            PSConsoleFileElement.WriteToFile(fullPath, this.PSVersion, this.ExternalPSSnapIns);
            this.fileName = fullPath;
            this.isDirty = false;
        }

        internal Collection<PSSnapInInfo> ExternalPSSnapIns
        {
            get
            {
                return this.externalPSSnapIns;
            }
        }

        internal string Filename
        {
            get
            {
                return this.fileName;
            }
        }

        internal bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
        }

        internal string MajorVersion
        {
            get
            {
                return this.psVersion.Major.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal Collection<PSSnapInInfo> PSSnapIns
        {
            get
            {
                return this.MergeDefaultExternalMshSnapins();
            }
        }

        internal Version PSVersion
        {
            get
            {
                return this.psVersion;
            }
        }
    }
}

