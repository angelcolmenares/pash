namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Provider;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class RunspaceConfigForSingleShell : RunspaceConfiguration
    {
        private RunspaceConfigurationEntryCollection<CmdletConfigurationEntry> _cmdlets = new RunspaceConfigurationEntryCollection<CmdletConfigurationEntry>();
        private MshConsoleInfo _consoleInfo;
        private RunspaceConfigurationEntryCollection<FormatConfigurationEntry> _formats;
        private RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> _initializationScripts;
        private static PSTraceSource _mshsnapinTracer = PSTraceSource.GetTracer("MshSnapinLoadUnload", "Loading and unloading mshsnapins", false);
        private RunspaceConfigurationEntryCollection<ProviderConfigurationEntry> _providers = new RunspaceConfigurationEntryCollection<ProviderConfigurationEntry>();
        private RunspaceConfigurationEntryCollection<TypeConfigurationEntry> _types;

        private RunspaceConfigForSingleShell(MshConsoleInfo consoleInfo)
        {
            this._consoleInfo = consoleInfo;
        }

        private void AnalyzeMshSnapinAssembly(Assembly assembly, PSSnapInInfo mshsnapinInfo)
        {
            if (assembly != null)
            {
                _mshsnapinTracer.WriteLine("Analyzing assembly {0} for cmdlet and providers", new object[] { assembly.Location });
                string helpFile = GetHelpFile(assembly.Location);
                Type[] exportedTypes = null;
                try
                {
                    exportedTypes = assembly.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    string errorMessageFormat = exception.Message + "\nLoader Exceptions: \n";
                    if (exception.LoaderExceptions != null)
                    {
                        foreach (Exception exception2 in exception.LoaderExceptions)
                        {
                            errorMessageFormat = errorMessageFormat + "\n" + exception2.Message;
                        }
                    }
                    _mshsnapinTracer.TraceError(errorMessageFormat, new object[0]);
                    throw new PSSnapInException(mshsnapinInfo.Name, errorMessageFormat);
                }
                Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                Hashtable hashtable2 = new Hashtable(StringComparer.OrdinalIgnoreCase);
                foreach (Type type in exportedTypes)
                {
                    string cmdletName = null;
                    string providerName = null;
                    object[] customAttributes = type.GetCustomAttributes(typeof(CmdletAttribute), false);
                    if (customAttributes.Length > 0)
                    {
                        cmdletName = GetCmdletName(customAttributes[0] as CmdletAttribute);
                    }
                    else
                    {
                        customAttributes = type.GetCustomAttributes(typeof(CmdletProviderAttribute), false);
                        if (customAttributes.Length > 0)
                        {
                            providerName = GetProviderName(customAttributes[0] as CmdletProviderAttribute);
                        }
                    }
                    if (!string.IsNullOrEmpty(cmdletName))
                    {
                        if (IsCmdletClass(type) && HasDefaultConstructor(type))
                        {
                            if (hashtable.ContainsKey(cmdletName))
                            {
                                string str5 = StringUtil.Format(ConsoleInfoErrorStrings.PSSnapInDuplicateCmdlets, cmdletName, mshsnapinInfo.Name);
                                _mshsnapinTracer.TraceError(str5, new object[0]);
                                throw new PSSnapInException(mshsnapinInfo.Name, str5);
                            }
                            hashtable.Add(cmdletName, null);
                            CmdletConfigurationEntry item = new CmdletConfigurationEntry(cmdletName, type, helpFile, mshsnapinInfo);
                            this._cmdlets.AddBuiltInItem(item);
                            _mshsnapinTracer.WriteLine("{0} from type {1} is added as a cmdlet. ", new object[] { cmdletName, type.FullName });
                            continue;
                        }
                        _mshsnapinTracer.TraceWarning("{0} is not valid cmdlet because it doesn't derive from the Cmdlet type or it doesn't have a default constructor.", new object[] { cmdletName });
                    }
                    if (!string.IsNullOrEmpty(providerName))
                    {
                        if (IsProviderClass(type) && HasDefaultConstructor(type))
                        {
                            if (hashtable2.ContainsKey(providerName))
                            {
                                string str6 = StringUtil.Format(ConsoleInfoErrorStrings.PSSnapInDuplicateProviders, providerName, mshsnapinInfo.Name);
                                _mshsnapinTracer.TraceError(str6, new object[0]);
                                throw new PSSnapInException(mshsnapinInfo.Name, str6);
                            }
                            hashtable2.Add(providerName, null);
                            ProviderConfigurationEntry entry2 = new ProviderConfigurationEntry(providerName, type, helpFile, mshsnapinInfo);
                            this._providers.AddBuiltInItem(entry2);
                            _mshsnapinTracer.WriteLine("{0} from type {1} is added as a provider. ", new object[] { providerName, type.FullName });
                            continue;
                        }
                        _mshsnapinTracer.TraceWarning("{0} is not valid provider because it doesn't derive from the provider type or it doesn't have a default constructor.", new object[] { providerName });
                    }
                    if (typeof(IModuleAssemblyInitializer).IsAssignableFrom(type) && !type.Equals(typeof(IModuleAssemblyInitializer)))
                    {
                        _mshsnapinTracer.TraceWarning("Calling module initializer defined by type {0}", new object[] { type.FullName });
                        (Activator.CreateInstance(type, true) as IModuleAssemblyInitializer).OnImport();
                    }
                }
            }
        }

        private static PSConsoleLoadException CombinePSConsoleLoadException(PSConsoleLoadException e1, PSConsoleLoadException e2)
        {
            if (((e1 == null) || (e1.PSSnapInExceptions.Count == 0)) && ((e2 == null) || (e2.PSSnapInExceptions.Count == 0)))
            {
                return null;
            }
            if ((e1 == null) || (e1.PSSnapInExceptions.Count == 0))
            {
                return e2;
            }
            if ((e2 != null) && (e2.PSSnapInExceptions.Count != 0))
            {
                foreach (PSSnapInException exception in e2.PSSnapInExceptions)
                {
                    e1.PSSnapInExceptions.Add(exception);
                }
            }
            return e1;
        }

        internal static RunspaceConfigForSingleShell Create(string consoleFile, out PSConsoleLoadException warning)
        {
            PSConsoleLoadException cle = null;
            _mshsnapinTracer.WriteLine("Creating MshConsoleInfo. consoleFile={0}", new object[] { consoleFile });
            MshConsoleInfo consoleInfo = MshConsoleInfo.CreateFromConsoleFile(consoleFile, out cle);
            if (cle != null)
            {
                _mshsnapinTracer.TraceWarning("There was a warning while creating MshConsoleInfo: {0}", new object[] { cle.Message });
            }
            if (consoleInfo != null)
            {
                RunspaceConfigForSingleShell shell = new RunspaceConfigForSingleShell(consoleInfo);
                PSConsoleLoadException exception2 = null;
                shell.LoadConsole(out exception2);
                if (exception2 != null)
                {
                    _mshsnapinTracer.TraceWarning("There was a warning while loading console: {0}", new object[] { exception2.Message });
                }
                warning = CombinePSConsoleLoadException(cle, exception2);
                return shell;
            }
            warning = null;
            return null;
        }

        internal static RunspaceConfigForSingleShell CreateDefaultConfiguration()
        {
            _mshsnapinTracer.WriteLine("Creating default runspace configuration.", new object[0]);
            MshConsoleInfo consoleInfo = MshConsoleInfo.CreateDefaultConfiguration();
            if (consoleInfo != null)
            {
                RunspaceConfigForSingleShell shell = new RunspaceConfigForSingleShell(consoleInfo);
                PSConsoleLoadException warning = null;
                shell.LoadConsole(out warning);
                if (warning != null)
                {
                    _mshsnapinTracer.TraceWarning("There was a warning while loading console: {0}", new object[] { warning.Message });
                }
                return shell;
            }
            _mshsnapinTracer.WriteLine("Default runspace configuration created.", new object[0]);
            return null;
        }

        internal override PSSnapInInfo DoAddPSSnapIn(string name, out PSSnapInException warning)
        {
            warning = null;
            _mshsnapinTracer.WriteLine("Adding mshsnapin {0}", new object[] { name });
            if (this._consoleInfo == null)
            {
                return null;
            }
            PSSnapInInfo mshsnapinInfo = null;
            try
            {
                mshsnapinInfo = this._consoleInfo.AddPSSnapIn(name);
            }
            catch (PSArgumentException exception)
            {
                _mshsnapinTracer.TraceError(exception.Message, new object[0]);
                _mshsnapinTracer.WriteLine("Adding mshsnapin {0} failed.", new object[] { name });
                throw;
            }
            catch (PSArgumentNullException exception2)
            {
                _mshsnapinTracer.TraceError(exception2.Message, new object[0]);
                _mshsnapinTracer.WriteLine("Adding mshsnapin {0} failed.", new object[] { name });
                throw;
            }
            if (mshsnapinInfo == null)
            {
                return null;
            }
            this.LoadPSSnapIn(mshsnapinInfo, out warning);
            if (warning != null)
            {
                _mshsnapinTracer.TraceWarning("There was a warning when loading mshsnapin {0}: {1}", new object[] { name, warning.Message });
            }
            _mshsnapinTracer.WriteLine("MshSnapin {0} added", new object[] { name });
            return mshsnapinInfo;
        }

        internal override PSSnapInInfo DoRemovePSSnapIn(string name, out PSSnapInException warning)
        {
            warning = null;
            if (this._consoleInfo == null)
            {
                return null;
            }
            _mshsnapinTracer.WriteLine("Removing mshsnapin {0}", new object[] { name });
            PSSnapInInfo mshsnapinInfo = this._consoleInfo.RemovePSSnapIn(name);
            this.UnloadPSSnapIn(mshsnapinInfo, out warning);
            _mshsnapinTracer.WriteLine("MshSnapin {0} removed", new object[] { name });
            return mshsnapinInfo;
        }

        private static string GetCmdletName(CmdletAttribute cmdletAttribute)
        {
            string verbName = cmdletAttribute.VerbName;
            string nounName = cmdletAttribute.NounName;
            return (verbName + "-" + nounName);
        }

        private static string GetHelpFile(string assemblyPath)
        {
            return (Path.GetFileName(assemblyPath) + "-Help.xml");
        }

        private static string GetProperty(object obj, string propertyName)
        {
            PropertyInfo property = obj.GetType().GetProperty(propertyName);
            if (property == null)
            {
                return null;
            }
            return (string) property.GetValue(obj, null);
        }

        private static string GetProviderName(CmdletProviderAttribute providerAttribute)
        {
            return providerAttribute.ProviderName;
        }

        private static bool HasDefaultConstructor(Type type)
        {
            return (type.GetConstructor(Type.EmptyTypes) != null);
        }

        private static bool IsCmdletClass(Type type)
        {
            if (type == null)
            {
                return false;
            }
            return type.IsSubclassOf(typeof(Cmdlet));
        }

        private static bool IsProviderClass(Type type)
        {
            if (type == null)
            {
                return false;
            }
            return type.IsSubclassOf(typeof(CmdletProvider));
        }

        private void LoadConsole(out PSConsoleLoadException warning)
        {
            if (this._consoleInfo == null)
            {
                warning = null;
            }
            else
            {
                this.LoadPSSnapIns(this._consoleInfo.PSSnapIns, out warning);
            }
        }

        private void LoadCustomPSSnapIn(PSSnapInInfo mshsnapinInfo)
        {
            if ((mshsnapinInfo != null) && !string.IsNullOrEmpty(mshsnapinInfo.CustomPSSnapInType))
            {
                Assembly assembly = null;
                _mshsnapinTracer.WriteLine("Loading assembly for mshsnapin {0}", new object[] { mshsnapinInfo.Name });
                assembly = this.LoadMshSnapinAssembly(mshsnapinInfo);
                if (assembly == null)
                {
                    _mshsnapinTracer.TraceError("Loading assembly for mshsnapin {0} failed", new object[] { mshsnapinInfo.Name });
                }
                else
                {
                    CustomPSSnapIn customPSSnapIn = null;
                    try
                    {
                        if (assembly.GetType(mshsnapinInfo.CustomPSSnapInType, true) != null)
                        {
                            customPSSnapIn = (CustomPSSnapIn) assembly.CreateInstance(mshsnapinInfo.CustomPSSnapInType);
                        }
                        _mshsnapinTracer.WriteLine("Loading assembly for mshsnapin {0} succeeded", new object[] { mshsnapinInfo.Name });
                    }
                    catch (TypeLoadException exception)
                    {
                        throw new PSSnapInException(mshsnapinInfo.Name, exception.Message);
                    }
                    catch (ArgumentException exception2)
                    {
                        throw new PSSnapInException(mshsnapinInfo.Name, exception2.Message);
                    }
                    catch (MissingMethodException exception3)
                    {
                        throw new PSSnapInException(mshsnapinInfo.Name, exception3.Message);
                    }
                    catch (InvalidCastException exception4)
                    {
                        throw new PSSnapInException(mshsnapinInfo.Name, exception4.Message);
                    }
                    catch (TargetInvocationException exception5)
                    {
                        if (exception5.InnerException != null)
                        {
                            throw new PSSnapInException(mshsnapinInfo.Name, exception5.InnerException.Message);
                        }
                        throw new PSSnapInException(mshsnapinInfo.Name, exception5.Message);
                    }
                    this.MergeCustomPSSnapIn(mshsnapinInfo, customPSSnapIn);
                }
            }
        }

        private Assembly LoadMshSnapinAssembly(PSSnapInInfo mshsnapinInfo)
        {
            Assembly assembly = null;
            _mshsnapinTracer.WriteLine("Loading assembly from GAC. Assembly Name: {0}", new object[] { mshsnapinInfo.AssemblyName });
            try
            {
                assembly = Assembly.Load(mshsnapinInfo.AssemblyName);
            }
            catch (FileLoadException exception)
            {
                _mshsnapinTracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { mshsnapinInfo.AssemblyName, exception.Message });
            }
            catch (BadImageFormatException exception2)
            {
                _mshsnapinTracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { mshsnapinInfo.AssemblyName, exception2.Message });
            }
            catch (FileNotFoundException exception3)
            {
                _mshsnapinTracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { mshsnapinInfo.AssemblyName, exception3.Message });
            }
            if (assembly == null)
            {
                _mshsnapinTracer.WriteLine("Loading assembly from path: {0}", new object[] { mshsnapinInfo.AssemblyName });
                try
                {
                    Assembly assembly2 = Assembly.ReflectionOnlyLoadFrom(mshsnapinInfo.AbsoluteModulePath);
                    if (assembly2 == null)
                    {
                        return null;
                    }
                    if (string.Compare(assembly2.FullName, mshsnapinInfo.AssemblyName, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        string errorMessageFormat = StringUtil.Format(ConsoleInfoErrorStrings.PSSnapInAssemblyNameMismatch, mshsnapinInfo.AbsoluteModulePath, mshsnapinInfo.AssemblyName);
                        _mshsnapinTracer.TraceError(errorMessageFormat, new object[0]);
                        throw new PSSnapInException(mshsnapinInfo.Name, errorMessageFormat);
                    }
                    assembly = Assembly.LoadFrom(mshsnapinInfo.AbsoluteModulePath);
                }
                catch (FileLoadException exception4)
                {
                    _mshsnapinTracer.TraceError("Not able to load assembly {0}: {1}", new object[] { mshsnapinInfo.AssemblyName, exception4.Message });
                    throw new PSSnapInException(mshsnapinInfo.Name, exception4.Message);
                }
                catch (BadImageFormatException exception5)
                {
                    _mshsnapinTracer.TraceError("Not able to load assembly {0}: {1}", new object[] { mshsnapinInfo.AssemblyName, exception5.Message });
                    throw new PSSnapInException(mshsnapinInfo.Name, exception5.Message);
                }
                catch (FileNotFoundException exception6)
                {
                    _mshsnapinTracer.TraceError("Not able to load assembly {0}: {1}", new object[] { mshsnapinInfo.AssemblyName, exception6.Message });
                    throw new PSSnapInException(mshsnapinInfo.Name, exception6.Message);
                }
            }
            return assembly;
        }

        private void LoadPSSnapIn(PSSnapInInfo mshsnapinInfo)
        {
            if (mshsnapinInfo != null)
            {
                if (!string.IsNullOrEmpty(mshsnapinInfo.CustomPSSnapInType))
                {
                    this.LoadCustomPSSnapIn(mshsnapinInfo);
                }
                else
                {
                    Assembly assembly = null;
                    _mshsnapinTracer.WriteLine("Loading assembly for mshsnapin {0}", new object[] { mshsnapinInfo.Name });
                    assembly = this.LoadMshSnapinAssembly(mshsnapinInfo);
                    if (assembly == null)
                    {
                        _mshsnapinTracer.TraceError("Loading assembly for mshsnapin {0} failed", new object[] { mshsnapinInfo.Name });
                    }
                    else
                    {
                        _mshsnapinTracer.WriteLine("Loading assembly for mshsnapin {0} succeeded", new object[] { mshsnapinInfo.Name });
                        this.AnalyzeMshSnapinAssembly(assembly, mshsnapinInfo);
                        foreach (string str in mshsnapinInfo.Types)
                        {
                            string name = Path.Combine(mshsnapinInfo.ApplicationBase, str);
                            TypeConfigurationEntry entry = new TypeConfigurationEntry(name, name, mshsnapinInfo);
                            this.Types.AddBuiltInItem(entry);
                        }
                        foreach (string str3 in mshsnapinInfo.Formats)
                        {
                            string str4 = Path.Combine(mshsnapinInfo.ApplicationBase, str3);
                            FormatConfigurationEntry entry2 = new FormatConfigurationEntry(str4, str4, mshsnapinInfo);
                            this.Formats.AddBuiltInItem(entry2);
                        }
                        AssemblyConfigurationEntry item = new AssemblyConfigurationEntry(mshsnapinInfo.AssemblyName, mshsnapinInfo.AbsoluteModulePath, mshsnapinInfo);
                        this.Assemblies.AddBuiltInItem(item);
                    }
                }
            }
        }

        private void LoadPSSnapIn(PSSnapInInfo mshsnapinInfo, out PSSnapInException warning)
        {
            string str;
            warning = null;
            try
            {
                this.LoadPSSnapIn(mshsnapinInfo);
            }
            catch (PSSnapInException)
            {
                if (!mshsnapinInfo.IsDefault)
                {
                    this._consoleInfo.RemovePSSnapIn(mshsnapinInfo.Name);
                }
                throw;
            }
            this.UpdateAll(out str);
            if (!string.IsNullOrEmpty(str))
            {
                _mshsnapinTracer.TraceWarning("There was a warning while loading mshsnapin {0}:{1}", new object[] { mshsnapinInfo.Name, str });
                warning = new PSSnapInException(mshsnapinInfo.Name, str, true);
            }
        }

        private void LoadPSSnapIns(Collection<PSSnapInInfo> mshsnapinInfos, out PSConsoleLoadException warning)
        {
            warning = null;
            Collection<PSSnapInException> exceptions = new Collection<PSSnapInException>();
            bool flag = false;
            foreach (PSSnapInInfo info in mshsnapinInfos)
            {
                try
                {
                    this.LoadPSSnapIn(info);
                    flag = true;
                }
                catch (PSSnapInException exception)
                {
                    if (info.IsDefault)
                    {
                        throw;
                    }
                    this._consoleInfo.RemovePSSnapIn(info.Name);
                    exceptions.Add(exception);
                }
            }
            if (flag)
            {
                string str;
                this.UpdateAll(out str);
                if (!string.IsNullOrEmpty(str))
                {
                    _mshsnapinTracer.TraceWarning(str, new object[0]);
                    exceptions.Add(new PSSnapInException(null, str, true));
                }
            }
            if (exceptions.Count > 0)
            {
                warning = new PSConsoleLoadException(this._consoleInfo, exceptions);
                _mshsnapinTracer.TraceWarning(warning.Message, new object[0]);
            }
        }

        private void MergeCustomPSSnapIn(PSSnapInInfo mshsnapinInfo, CustomPSSnapIn customPSSnapIn)
        {
            if ((mshsnapinInfo != null) && (customPSSnapIn != null))
            {
                _mshsnapinTracer.WriteLine("Merging configuration from custom mshsnapin {0}", new object[] { mshsnapinInfo.Name });
                if (customPSSnapIn.Cmdlets != null)
                {
                    foreach (CmdletConfigurationEntry entry in customPSSnapIn.Cmdlets)
                    {
                        CmdletConfigurationEntry entry2 = new CmdletConfigurationEntry(entry.Name, entry.ImplementingType, entry.HelpFileName, mshsnapinInfo);
                        this._cmdlets.AddBuiltInItem(entry2);
                    }
                }
                if (customPSSnapIn.Providers != null)
                {
                    foreach (ProviderConfigurationEntry entry3 in customPSSnapIn.Providers)
                    {
                        ProviderConfigurationEntry entry4 = new ProviderConfigurationEntry(entry3.Name, entry3.ImplementingType, entry3.HelpFileName, mshsnapinInfo);
                        this._providers.AddBuiltInItem(entry4);
                    }
                }
                if (customPSSnapIn.Types != null)
                {
                    foreach (TypeConfigurationEntry entry5 in customPSSnapIn.Types)
                    {
                        string fileName = Path.Combine(mshsnapinInfo.ApplicationBase, entry5.FileName);
                        TypeConfigurationEntry entry6 = new TypeConfigurationEntry(entry5.Name, fileName, mshsnapinInfo);
                        this._types.AddBuiltInItem(entry6);
                    }
                }
                if (customPSSnapIn.Formats != null)
                {
                    foreach (FormatConfigurationEntry entry7 in customPSSnapIn.Formats)
                    {
                        string str2 = Path.Combine(mshsnapinInfo.ApplicationBase, entry7.FileName);
                        FormatConfigurationEntry entry8 = new FormatConfigurationEntry(entry7.Name, str2, mshsnapinInfo);
                        this._formats.AddBuiltInItem(entry8);
                    }
                }
                AssemblyConfigurationEntry item = new AssemblyConfigurationEntry(mshsnapinInfo.AssemblyName, mshsnapinInfo.AbsoluteModulePath, mshsnapinInfo);
                this.Assemblies.AddBuiltInItem(item);
                _mshsnapinTracer.WriteLine("Configuration from custom mshsnapin {0} merged", new object[] { mshsnapinInfo.Name });
            }
        }

        internal void SaveAsConsoleFile(string filename)
        {
            if (this._consoleInfo != null)
            {
                this._consoleInfo.SaveAsConsoleFile(filename);
            }
        }

        internal void SaveConsoleFile()
        {
            if (this._consoleInfo != null)
            {
                this._consoleInfo.Save();
            }
        }

        private void UnloadPSSnapIn(PSSnapInInfo mshsnapinInfo, out PSSnapInException warning)
        {
            warning = null;
            if (mshsnapinInfo != null)
            {
                string str;
                this.Cmdlets.RemovePSSnapIn(mshsnapinInfo.Name);
                this.Providers.RemovePSSnapIn(mshsnapinInfo.Name);
                this.Assemblies.RemovePSSnapIn(mshsnapinInfo.Name);
                this.Types.RemovePSSnapIn(mshsnapinInfo.Name);
                this.Formats.RemovePSSnapIn(mshsnapinInfo.Name);
                this.UpdateAll(out str);
                if (!string.IsNullOrEmpty(str))
                {
                    _mshsnapinTracer.TraceWarning(str, new object[0]);
                    warning = new PSSnapInException(mshsnapinInfo.Name, str, true);
                }
            }
        }

        internal void UpdateAll()
        {
            string errors = "";
            this.UpdateAll(out errors);
        }

        internal void UpdateAll(out string errors)
        {
            errors = "";
            this.Cmdlets.Update();
            this.Providers.Update();
            _mshsnapinTracer.WriteLine("Updating types and formats", new object[0]);
            try
            {
                this.Types.Update();
            }
            catch (RuntimeException exception)
            {
                _mshsnapinTracer.TraceWarning("There was a warning updating types: {0}", new object[] { exception.Message });
                errors = errors + exception.Message + "\n";
            }
            try
            {
                this.Formats.Update();
            }
            catch (RuntimeException exception2)
            {
                _mshsnapinTracer.TraceWarning("There was a warning updating formats: {0}", new object[] { exception2.Message });
                errors = errors + exception2.Message + "\n";
            }
            try
            {
                this.Assemblies.Update();
            }
            catch (RuntimeException exception3)
            {
                _mshsnapinTracer.TraceWarning("There was a warning updating assemblies: {0}", new object[] { exception3.Message });
                errors = errors + exception3.Message + "\n";
            }
            _mshsnapinTracer.WriteLine("Types and formats updated successfully", new object[0]);
        }

        public override RunspaceConfigurationEntryCollection<CmdletConfigurationEntry> Cmdlets
        {
            get
            {
                return this._cmdlets;
            }
        }

        internal MshConsoleInfo ConsoleInfo
        {
            get
            {
                return this._consoleInfo;
            }
        }

        public override RunspaceConfigurationEntryCollection<FormatConfigurationEntry> Formats
        {
            get
            {
                if (this._formats == null)
                {
                    this._formats = new RunspaceConfigurationEntryCollection<FormatConfigurationEntry>();
                }
                return this._formats;
            }
        }

        public override RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> InitializationScripts
        {
            get
            {
                if (this._initializationScripts == null)
                {
                    this._initializationScripts = new RunspaceConfigurationEntryCollection<ScriptConfigurationEntry>();
                }
                return this._initializationScripts;
            }
        }

        public override RunspaceConfigurationEntryCollection<ProviderConfigurationEntry> Providers
        {
            get
            {
                return this._providers;
            }
        }

        public override string ShellId
        {
            get
            {
                return Utils.DefaultPowerShellShellID;
            }
        }

        public override RunspaceConfigurationEntryCollection<TypeConfigurationEntry> Types
        {
            get
            {
                if (this._types == null)
                {
                    this._types = new RunspaceConfigurationEntryCollection<TypeConfigurationEntry>();
                }
                return this._types;
            }
        }
    }
}

