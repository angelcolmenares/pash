namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    [Cmdlet("New", "ModuleManifest", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=141555"), OutputType(new Type[] { typeof(string) })]
    public sealed class NewModuleManifestCommand : PSCmdlet
    {
        private string _author;
        private Version _ClrVersion;
        private string _companyName = "";
        private string _copyright;
        private string _defaultCommandPrefix;
        private string _description;
        private Version _DotNetFrameworkVersion;
        private string[] _exportedAliases = new string[] { "*" };
        private string[] _exportedCmdlets = new string[] { "*" };
        private string[] _exportedFunctions = new string[] { "*" };
        private string[] _exportedVariables = new string[] { "*" };
        private string[] _formats;
        private System.Guid _guid = System.Guid.NewGuid();
        private string _helpInfoUri;
        private string[] _miscFiles;
        private object[] _moduleList;
        private Version _moduleVersion = new Version(1, 0);
        private object[] _nestedModules;
        private bool _passThru;
        private string _path;
        private string _PowerShellHostName;
        private Version _PowerShellHostVersion;
        private Version _powerShellVersion;
        private object _privateData;
        private System.Reflection.ProcessorArchitecture? _processorArchitecture = null;
        private string[] _requiredAssemblies;
        private object[] _requiredModules;
        private string _rootModule;
        private string[] _scripts;
        private string[] _types;

        protected override void BeginProcessing()
        {
            if (this.ProcessorArchitecture == System.Reflection.ProcessorArchitecture.IA64)
            {
                InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.InvalidProcessorArchitectureInManifest, this.ProcessorArchitecture));
                ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_InvalidProcessorArchitectureInManifest", ErrorCategory.InvalidArgument, this.ProcessorArchitecture);
                base.ThrowTerminatingError(errorRecord);
            }
        }

        private void BuildModuleManifest(StringBuilder result, string key, string keyDescription, bool hasValue, Func<string> action, StreamWriter streamWriter)
        {
            if (hasValue)
            {
                result.Append(this.ManifestFragment(key, keyDescription, action(), streamWriter));
            }
            else
            {
                result.Append(this.ManifestFragmentForNonSpecifiedManifestMember(key, keyDescription, action(), streamWriter));
            }
        }

        private string ManifestComment(string insert, StreamWriter streamWriter)
        {
            if (!string.IsNullOrEmpty(insert))
            {
                insert = " " + insert;
            }
            return string.Format(CultureInfo.InvariantCulture, "#{0}{1}", new object[] { insert, streamWriter.NewLine });
        }

        private string ManifestFragment(string key, string resourceString, string value, StreamWriter streamWriter)
        {
            string newLine = streamWriter.NewLine;
            return string.Format(CultureInfo.InvariantCulture, "# {0}{1}{2:19} = {3}{4}{5}", new object[] { resourceString, newLine, key, value, newLine, newLine });
        }

        private string ManifestFragmentForNonSpecifiedManifestMember(string key, string resourceString, string value, StreamWriter streamWriter)
        {
            string newLine = streamWriter.NewLine;
            return string.Format(CultureInfo.InvariantCulture, "# {0}{1}{2:19} = {3}{4}{5}", new object[] { resourceString, newLine, "# " + key, value, newLine, newLine });
        }

        private IEnumerable PreProcessModuleSpec(IEnumerable moduleSpecs)
        {
            if (moduleSpecs != null)
            {
                IEnumerator enumerator = moduleSpecs.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    object current = enumerator.Current;
                    if (!(current is Hashtable))
                    {
                        yield return current.ToString();
                    }
                    else
                    {
                        yield return current;
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            ProviderInfo provider = null;
            PSDriveInfo info2;
            Func<string> action = null;
            Func<string> func14 = null;
            Func<string> func15 = null;
            Func<string> func16 = null;
            Func<string> func17 = null;
            Func<string> func18 = null;
            Func<string> func19 = null;
            Func<string> func20 = null;
            Func<string> func21 = null;
            Func<string> func22 = null;
            Func<string> func23 = null;
            Func<string> func24 = null;
            Func<string> func25 = null;
            Func<string> func26 = null;
            Func<string> func27 = null;
            Func<string> func28 = null;
            string o = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(this._path, out provider, out info2);
            if (!provider.NameEquals(base.Context.ProviderNames.FileSystem) || !o.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
            {
                InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.InvalidModuleManifestPath, this._path));
                ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_InvalidModuleManifestPath", ErrorCategory.InvalidArgument, this._path);
                base.ThrowTerminatingError(errorRecord);
            }
            string str3 = StringUtil.Format(Modules.CreatingModuleManifestFile, o);
            if (base.ShouldProcess(o, str3))
            {
                FileStream stream;
                FileInfo info3;
                Func<string> func = null;
                Func<string> func2 = null;
                Func<string> func3 = null;
                Func<string> func4 = null;
                Func<string> func5 = null;
                Func<string> func6 = null;
                Func<string> func7 = null;
                Func<string> func8 = null;
                Func<string> func9 = null;
                Func<string> func10 = null;
                Func<string> func11 = null;
                Func<string> func12 = null;
                StreamWriter streamWriter;
                if (string.IsNullOrEmpty(this._author))
                {
                    this._author = Environment.UserName;
                }
                if (string.IsNullOrEmpty(this._companyName))
                {
                    this._companyName = Modules.DefaultCompanyName;
                }
                if (string.IsNullOrEmpty(this._copyright))
                {
                    this._copyright = StringUtil.Format(Modules.DefaultCopyrightMessage, DateTime.Now.Year, this._author);
                }
                PathUtils.MasterStreamOpen(this, o, "unicode", false, false, false, false, out stream, out streamWriter, out info3, false);
                try
                {
                    StringBuilder result = new StringBuilder();
                    result.Append(this.ManifestComment("", streamWriter));
                    result.Append(this.ManifestComment(StringUtil.Format(Modules.ManifestHeaderLine1, System.IO.Path.GetFileNameWithoutExtension(o)), streamWriter));
                    result.Append(this.ManifestComment("", streamWriter));
                    result.Append(this.ManifestComment(StringUtil.Format(Modules.ManifestHeaderLine2, this._author), streamWriter));
                    result.Append(this.ManifestComment("", streamWriter));
                    result.Append(this.ManifestComment(StringUtil.Format(Modules.ManifestHeaderLine3, DateTime.Now.ToShortDateString()), streamWriter));
                    result.Append(this.ManifestComment("", streamWriter));
                    result.Append(streamWriter.NewLine);
                    result.Append("@{");
                    result.Append(streamWriter.NewLine);
                    result.Append(streamWriter.NewLine);
                    if (this._rootModule == null)
                    {
                        this._rootModule = string.Empty;
                    }
                    if (action == null)
                    {
                        action = () => this.QuoteName(this._rootModule);
                    }
                    this.BuildModuleManifest(result, "RootModule", Modules.RootModule, !string.IsNullOrEmpty(this._rootModule), action, streamWriter);
                    if (func14 == null)
                    {
                        func14 = () => this.QuoteName(this._moduleVersion.ToString());
                    }
                    this.BuildModuleManifest(result, "ModuleVersion", Modules.ModuleVersion, (this._moduleVersion != null) && !string.IsNullOrEmpty(this._moduleVersion.ToString()), func14, streamWriter);
                    if (func15 == null)
                    {
                        func15 = () => this.QuoteName(this._guid.ToString());
                    }
                    this.BuildModuleManifest(result, "GUID", Modules.GUID, !string.IsNullOrEmpty(this._guid.ToString()), func15, streamWriter);
                    if (func16 == null)
                    {
                        func16 = () => this.QuoteName(this.Author);
                    }
                    this.BuildModuleManifest(result, "Author", Modules.Author, !string.IsNullOrEmpty(this._author), func16, streamWriter);
                    if (func17 == null)
                    {
                        func17 = () => this.QuoteName(this._companyName);
                    }
                    this.BuildModuleManifest(result, "CompanyName", Modules.CompanyName, !string.IsNullOrEmpty(this._companyName), func17, streamWriter);
                    if (func18 == null)
                    {
                        func18 = () => this.QuoteName(this._copyright);
                    }
                    this.BuildModuleManifest(result, "Copyright", Modules.Copyright, !string.IsNullOrEmpty(this._copyright), func18, streamWriter);
                    if (func19 == null)
                    {
                        func19 = () => this.QuoteName(this._description);
                    }
                    this.BuildModuleManifest(result, "Description", Modules.Description, !string.IsNullOrEmpty(this._description), func19, streamWriter);
                    if (func20 == null)
                    {
                        func20 = () => this.QuoteName(this._powerShellVersion);
                    }
                    this.BuildModuleManifest(result, "PowerShellVersion", Modules.PowerShellVersion, (this._powerShellVersion != null) && !string.IsNullOrEmpty(this._powerShellVersion.ToString()), func20, streamWriter);
                    if (func21 == null)
                    {
                        func21 = () => this.QuoteName(this._PowerShellHostName);
                    }
                    this.BuildModuleManifest(result, "PowerShellHostName", Modules.PowerShellHostName, !string.IsNullOrEmpty(this._PowerShellHostName), func21, streamWriter);
                    if (func22 == null)
                    {
                        func22 = () => this.QuoteName(this._PowerShellHostVersion);
                    }
                    this.BuildModuleManifest(result, "PowerShellHostVersion", Modules.PowerShellHostVersion, (this._PowerShellHostVersion != null) && !string.IsNullOrEmpty(this._PowerShellHostVersion.ToString()), func22, streamWriter);
                    if (func23 == null)
                    {
                        func23 = () => this.QuoteName(this._DotNetFrameworkVersion);
                    }
                    this.BuildModuleManifest(result, "DotNetFrameworkVersion", Modules.DotNetFrameworkVersion, (this._DotNetFrameworkVersion != null) && !string.IsNullOrEmpty(this._DotNetFrameworkVersion.ToString()), func23, streamWriter);
                    if (func24 == null)
                    {
                        func24 = () => this.QuoteName(this._ClrVersion);
                    }
                    this.BuildModuleManifest(result, "CLRVersion", Modules.CLRVersion, (this._ClrVersion != null) && !string.IsNullOrEmpty(this._ClrVersion.ToString()), func24, streamWriter);
                    if (func25 == null)
                    {
                        func25 = () => this.QuoteName(this._processorArchitecture);
                    }
                    this.BuildModuleManifest(result, "ProcessorArchitecture", Modules.ProcessorArchitecture, this._processorArchitecture.HasValue, func25, streamWriter);
                    if (func == null)
                    {
                        func = () => this.QuoteModules(this._requiredModules, streamWriter);
                    }
                    this.BuildModuleManifest(result, "RequiredModules", Modules.RequiredModules, (this._requiredModules != null) && (this._requiredModules.Length > 0), func, streamWriter);
                    if (func2 == null)
                    {
                        func2 = () => this.QuoteFiles(this._requiredAssemblies, streamWriter);
                    }
                    this.BuildModuleManifest(result, "RequiredAssemblies", Modules.RequiredAssemblies, this._requiredAssemblies != null, func2, streamWriter);
                    if (func3 == null)
                    {
                        func3 = () => this.QuoteFiles(this._scripts, streamWriter);
                    }
                    this.BuildModuleManifest(result, "ScriptsToProcess", Modules.ScriptsToProcess, this._scripts != null, func3, streamWriter);
                    if (func4 == null)
                    {
                        func4 = () => this.QuoteFiles(this._types, streamWriter);
                    }
                    this.BuildModuleManifest(result, "TypesToProcess", Modules.TypesToProcess, this._types != null, func4, streamWriter);
                    if (func5 == null)
                    {
                        func5 = () => this.QuoteFiles(this._formats, streamWriter);
                    }
                    this.BuildModuleManifest(result, "FormatsToProcess", Modules.FormatsToProcess, this._formats != null, func5, streamWriter);
                    if (func6 == null)
                    {
                        func6 = () => this.QuoteModules(this.PreProcessModuleSpec(this._nestedModules), streamWriter);
                    }
                    this.BuildModuleManifest(result, "NestedModules", Modules.NestedModules, this._nestedModules != null, func6, streamWriter);
                    if (func7 == null)
                    {
                        func7 = () => this.QuoteNames(this._exportedFunctions, streamWriter);
                    }
                    this.BuildModuleManifest(result, "FunctionsToExport", Modules.FunctionsToExport, (this._exportedFunctions != null) && (this._exportedFunctions.Length > 0), func7, streamWriter);
                    if (func8 == null)
                    {
                        func8 = () => this.QuoteNames(this._exportedCmdlets, streamWriter);
                    }
                    this.BuildModuleManifest(result, "CmdletsToExport", Modules.CmdletsToExport, (this._exportedCmdlets != null) && (this._exportedCmdlets.Length > 0), func8, streamWriter);
                    if (func9 == null)
                    {
                        func9 = () => this.QuoteNames(this._exportedVariables, streamWriter);
                    }
                    this.BuildModuleManifest(result, "VariablesToExport", Modules.VariablesToExport, (this._exportedVariables != null) && (this._exportedVariables.Length > 0), func9, streamWriter);
                    if (func10 == null)
                    {
                        func10 = () => this.QuoteNames(this._exportedAliases, streamWriter);
                    }
                    this.BuildModuleManifest(result, "AliasesToExport", Modules.AliasesToExport, (this._exportedAliases != null) && (this._exportedAliases.Length > 0), func10, streamWriter);
                    if (func11 == null)
                    {
                        func11 = () => this.QuoteModules(this._moduleList, streamWriter);
                    }
                    this.BuildModuleManifest(result, "ModuleList", Modules.ModuleList, this._moduleList != null, func11, streamWriter);
                    if (func12 == null)
                    {
                        func12 = () => this.QuoteFiles(this._miscFiles, streamWriter);
                    }
                    this.BuildModuleManifest(result, "FileList", Modules.FileList, this._miscFiles != null, func12, streamWriter);
                    if (func26 == null)
                    {
                        func26 = () => this.QuoteName((string) LanguagePrimitives.ConvertTo(this._privateData, typeof(string), CultureInfo.InvariantCulture));
                    }
                    this.BuildModuleManifest(result, "PrivateData", Modules.PrivateData, this._privateData != null, func26, streamWriter);
                    if (func27 == null)
                    {
                        func27 = () => this.QuoteName(this._helpInfoUri);
                    }
                    this.BuildModuleManifest(result, "HelpInfoURI", Modules.HelpInfoURI, !string.IsNullOrEmpty(this._helpInfoUri), func27, streamWriter);
                    if (func28 == null)
                    {
                        func28 = () => this.QuoteName(this._defaultCommandPrefix);
                    }
                    this.BuildModuleManifest(result, "DefaultCommandPrefix", Modules.DefaultCommandPrefix, !string.IsNullOrEmpty(this._defaultCommandPrefix), func28, streamWriter);
                    result.Append("}");
                    result.Append(streamWriter.NewLine);
                    result.Append(streamWriter.NewLine);
                    string sendToPipeline = result.ToString();
                    if (this._passThru)
                    {
                        base.WriteObject(sendToPipeline);
                    }
                    streamWriter.Write(sendToPipeline);
                }
                finally
                {
                    streamWriter.Close();
                }
            }
        }

        private string QuoteFiles(IEnumerable names, StreamWriter streamWriter)
        {
            List<string> list = new List<string>();
            if (names != null)
            {
                foreach (string str in names)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        foreach (string str2 in this.TryResolveFilePath(str))
                        {
                            list.Add(str2);
                        }
                    }
                }
            }
            return this.QuoteNames(list, streamWriter);
        }

        private string QuoteModules(IEnumerable moduleSpecs, StreamWriter streamWriter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("@(");
            if (moduleSpecs != null)
            {
                bool flag = true;
                foreach (object obj2 in moduleSpecs)
                {
                    if (obj2 != null)
                    {
                        ModuleSpecification specification = (ModuleSpecification) LanguagePrimitives.ConvertTo(obj2, typeof(ModuleSpecification), CultureInfo.InvariantCulture);
                        if (!flag)
                        {
                            builder.Append(", ");
                            builder.Append(streamWriter.NewLine);
                            builder.Append("               ");
                        }
                        flag = false;
                        if (!specification.Guid.HasValue && (specification.Version == null))
                        {
                            builder.Append(this.QuoteName(specification.Name));
                        }
                        else
                        {
                            builder.Append("@{");
                            builder.Append("ModuleName = ");
                            builder.Append(this.QuoteName(specification.Name));
                            builder.Append("; ");
                            if (specification.Guid.HasValue)
                            {
                                builder.Append("GUID = ");
                                builder.Append(this.QuoteName(specification.Guid.ToString()));
                                builder.Append("; ");
                            }
                            if (specification.Version != null)
                            {
                                builder.Append("ModuleVersion = ");
                                builder.Append(this.QuoteName(specification.Version.ToString()));
                                builder.Append("; ");
                            }
                            builder.Append("}");
                        }
                    }
                }
            }
            builder.Append(")");
            return builder.ToString();
        }

        private string QuoteName(object name)
        {
            if (name == null)
            {
                return "''";
            }
            return ("'" + name.ToString().Replace("'", "''") + "'");
        }

        private string QuoteNames(IEnumerable names, StreamWriter streamWriter)
        {
            if (names == null)
            {
                return "@()";
            }
            StringBuilder builder = new StringBuilder();
            int num = 15;
            bool flag = true;
            foreach (string str in names)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        builder.Append(", ");
                    }
                    string str2 = this.QuoteName(str);
                    num += str2.Length;
                    if (num > 80)
                    {
                        builder.Append(streamWriter.NewLine);
                        builder.Append("               ");
                        num = 15 + str2.Length;
                    }
                    builder.Append(str2);
                }
            }
            if (builder.Length == 0)
            {
                return "@()";
            }
            return builder.ToString();
        }

        private List<string> TryResolveFilePath(string filePath)
        {
            List<string> list = new List<string>();
            ProviderInfo provider = null;
            SessionState sessionState = base.Context.SessionState;
            try
            {
                Collection<string> resolvedProviderPathFromPSPath = sessionState.Path.GetResolvedProviderPathFromPSPath(filePath, out provider);
                if ((!provider.NameEquals(base.Context.ProviderNames.FileSystem) || (resolvedProviderPathFromPSPath == null)) || (resolvedProviderPathFromPSPath.Count < 1))
                {
                    list.Add(filePath);
                    return list;
                }
                foreach (string str in resolvedProviderPathFromPSPath)
                {
                    string item = base.SessionState.Path.NormalizeRelativePath(str, base.SessionState.Path.CurrentLocation.ProviderPath);
                    if (item.StartsWith(@".\", StringComparison.OrdinalIgnoreCase) || item.StartsWith("./", StringComparison.OrdinalIgnoreCase))
                    {
                        item = item.Substring(2);
                    }
                    list.Add(item);
                }
            }
            catch (ItemNotFoundException)
            {
                list.Add(filePath);
            }
            return list;
        }

        [AllowEmptyCollection, Parameter]
        public string[] AliasesToExport
        {
            get
            {
                return this._exportedAliases;
            }
            set
            {
                this._exportedAliases = value;
            }
        }

        [Parameter, AllowEmptyString]
        public string Author
        {
            get
            {
                return this._author;
            }
            set
            {
                this._author = value;
            }
        }

        [Parameter]
        public Version ClrVersion
        {
            get
            {
                return this._ClrVersion;
            }
            set
            {
                this._ClrVersion = value;
            }
        }

        [AllowEmptyCollection, Parameter]
        public string[] CmdletsToExport
        {
            get
            {
                return this._exportedCmdlets;
            }
            set
            {
                this._exportedCmdlets = value;
            }
        }

        [AllowEmptyString, Parameter]
        public string CompanyName
        {
            get
            {
                return this._companyName;
            }
            set
            {
                this._companyName = value;
            }
        }

        [AllowEmptyString, Parameter]
        public string Copyright
        {
            get
            {
                return this._copyright;
            }
            set
            {
                this._copyright = value;
            }
        }

        [Parameter, AllowNull]
        public string DefaultCommandPrefix
        {
            get
            {
                return this._defaultCommandPrefix;
            }
            set
            {
                this._defaultCommandPrefix = value;
            }
        }

        [AllowEmptyString, Parameter]
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }

        [Parameter]
        public Version DotNetFrameworkVersion
        {
            get
            {
                return this._DotNetFrameworkVersion;
            }
            set
            {
                this._DotNetFrameworkVersion = value;
            }
        }

        [AllowEmptyCollection, Parameter]
        public string[] FileList
        {
            get
            {
                return this._miscFiles;
            }
            set
            {
                this._miscFiles = value;
            }
        }

        [Parameter, AllowEmptyCollection]
        public string[] FormatsToProcess
        {
            get
            {
                return this._formats;
            }
            set
            {
                this._formats = value;
            }
        }

        [AllowEmptyCollection, Parameter]
        public string[] FunctionsToExport
        {
            get
            {
                return this._exportedFunctions;
            }
            set
            {
                this._exportedFunctions = value;
            }
        }

        [Parameter]
        public System.Guid Guid
        {
            get
            {
                return this._guid;
            }
            set
            {
                this._guid = value;
            }
        }

        [Parameter, AllowNull]
        public string HelpInfoUri
        {
            get
            {
                return this._helpInfoUri;
            }
            set
            {
                this._helpInfoUri = value;
            }
        }

        [ArgumentTypeConverter(new Type[] { typeof(ModuleSpecification[]) }), Parameter, AllowEmptyCollection]
        public object[] ModuleList
        {
            get
            {
                return this._moduleList;
            }
            set
            {
                this._moduleList = value;
            }
        }

        [ValidateNotNull, Parameter]
        public Version ModuleVersion
        {
            get
            {
                return this._moduleVersion;
            }
            set
            {
                this._moduleVersion = value;
            }
        }

        [Parameter, AllowEmptyCollection]
        public object[] NestedModules
        {
            get
            {
                return this._nestedModules;
            }
            set
            {
                this._nestedModules = value;
            }
        }

        [Parameter]
        public SwitchParameter PassThru
        {
            get
            {
                return this._passThru;
            }
            set
            {
                this._passThru = (bool) value;
            }
        }

        [Parameter(Mandatory=true, Position=0)]
        public string Path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
            }
        }

        [Parameter]
        public string PowerShellHostName
        {
            get
            {
                return this._PowerShellHostName;
            }
            set
            {
                this._PowerShellHostName = value;
            }
        }

        [Parameter]
        public Version PowerShellHostVersion
        {
            get
            {
                return this._PowerShellHostVersion;
            }
            set
            {
                this._PowerShellHostVersion = value;
            }
        }

        [Parameter]
        public Version PowerShellVersion
        {
            get
            {
                return this._powerShellVersion;
            }
            set
            {
                this._powerShellVersion = value;
            }
        }

        [Parameter(Mandatory=false), AllowNull]
        public object PrivateData
        {
            get
            {
                return this._privateData;
            }
            set
            {
                this._privateData = value;
            }
        }

        [Parameter]
        public System.Reflection.ProcessorArchitecture ProcessorArchitecture
        {
            get
            {
                if (!this._processorArchitecture.HasValue)
                {
                    return System.Reflection.ProcessorArchitecture.None;
                }
                return this._processorArchitecture.Value;
            }
            set
            {
                this._processorArchitecture = new System.Reflection.ProcessorArchitecture?(value);
            }
        }

        [Parameter, AllowEmptyCollection]
        public string[] RequiredAssemblies
        {
            get
            {
                return this._requiredAssemblies;
            }
            set
            {
                this._requiredAssemblies = value;
            }
        }

        [Parameter, ArgumentTypeConverter(new Type[] { typeof(ModuleSpecification[]) })]
        public object[] RequiredModules
        {
            get
            {
                return this._requiredModules;
            }
            set
            {
                this._requiredModules = value;
            }
        }

        [Parameter, AllowEmptyString, Alias(new string[] { "ModuleToProcess" })]
        public string RootModule
        {
            get
            {
                return this._rootModule;
            }
            set
            {
                this._rootModule = value;
            }
        }

        [Parameter, AllowEmptyCollection]
        public string[] ScriptsToProcess
        {
            get
            {
                return this._scripts;
            }
            set
            {
                this._scripts = value;
            }
        }

        [Parameter, AllowEmptyCollection]
        public string[] TypesToProcess
        {
            get
            {
                return this._types;
            }
            set
            {
                this._types = value;
            }
        }

        [Parameter, AllowEmptyCollection]
        public string[] VariablesToExport
        {
            get
            {
                return this._exportedVariables;
            }
            set
            {
                this._exportedVariables = value;
            }
        }

        
    }
}

