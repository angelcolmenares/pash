namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Text;

    [Cmdlet("New", "PSSessionConfigurationFile", HelpUri="http://go.microsoft.com/fwlink/?LinkID=217036")]
    public class NewPSSessionConfigurationFileCommand : PSCmdlet
    {
        private Hashtable[] aliasDefinitions;
        private string[] assembliesToLoad;
        private string author;
        private string companyName;
        private string copyright;
        private string description;
        private object environmentVariables;
        private Microsoft.PowerShell.ExecutionPolicy executionPolicy = Microsoft.PowerShell.ExecutionPolicy.Restricted;
        private string[] formatsToProcess;
        private Hashtable[] functionDefinitions;
        private System.Guid guid = System.Guid.NewGuid();
        private System.Management.Automation.Remoting.SessionType initialSessionState = System.Management.Automation.Remoting.SessionType.Default;
        private bool isLanguageModeSpecified;
        private PSLanguageMode languageMode = PSLanguageMode.NoLanguage;
        private object[] modulesToImport;
        private string path;
        private Version powerShellVersion;
        private Version schemaVersion = new Version("1.0.0.0");
        private string[] scriptsToProcess;
        private string[] typesToProcess;
        private object variableDefinitions;
        private string[] visibleAliases;
        private string[] visibleCmdlets;
        private string[] visibleFunctions;
        private string[] visibleProviders;

        private string CombineHashtable(Hashtable table, StreamWriter writer)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("@{");
            builder.Append(writer.NewLine);
            foreach (string str in from x in table.Keys.Cast<string>()
                orderby x
                select x)
            {
                builder.Append("    ");
                builder.Append(str);
                builder.Append("=");
                if (table[str] is ScriptBlock)
                {
                    builder.Append(WrapScriptBlock(table[str].ToString()));
                }
                else
                {
                    builder.Append(QuoteName(table[str].ToString()));
                }
                builder.Append(writer.NewLine);
            }
            builder.Append("}");
            return builder.ToString();
        }

        private string CombineHashtableArray(Hashtable[] tables, StreamWriter writer)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("@(");
            builder.Append(writer.NewLine);
            for (int i = 0; i < tables.Length; i++)
            {
                builder.Append(this.CombineHashtable(tables[i], writer));
                if (i < (tables.Length - 1))
                {
                    builder.Append(", ");
                }
                builder.Append(writer.NewLine);
            }
            builder.Append(")");
            builder.Append(writer.NewLine);
            return builder.ToString();
        }

        private string CombineHashTableOrStringArray(object[] values, StreamWriter writer)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                string str = values[i] as string;
                if (!string.IsNullOrEmpty(str))
                {
                    builder.Append(QuoteName(str));
                }
                else
                {
                    Hashtable table = values[i] as Hashtable;
                    if (table == null)
                    {
                        PSArgumentException exception = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeStringOrHashtableArray, ConfigFileContants.ModulesToImport));
                        base.ThrowTerminatingError(exception.ErrorRecord);
                    }
                    builder.Append(this.CombineHashtable(table, writer));
                }
                if (i < (values.Length - 1))
                {
                    builder.Append(", ");
                }
            }
            return builder.ToString();
        }

        private string CombineStringArray(string[] values)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrEmpty(values[i]))
                {
                    builder.Append(QuoteName(values[i]));
                    if (i < (values.Length - 1))
                    {
                        builder.Append(", ");
                    }
                }
            }
            return builder.ToString();
        }

        private static string ConfigFragment(string key, string resourceString, string value, StreamWriter streamWriter)
        {
            string newLine = streamWriter.NewLine;
            if (string.IsNullOrEmpty(value))
            {
                return string.Format(CultureInfo.InvariantCulture, "# {0}{1}# {2:19} = {3}{4}", new object[] { resourceString, newLine, key, newLine, newLine });
            }
            return string.Format(CultureInfo.InvariantCulture, "# {0}{1}{2:19} = {3}{4}{5}", new object[] { resourceString, newLine, key, value, newLine, newLine });
        }

        private string GetVisibilityDefault(string[] value)
        {
            if (value != null)
            {
                return this.CombineStringArray(value);
            }
            return string.Empty;
        }

        protected override void ProcessRecord()
        {
            ProviderInfo provider = null;
            PSDriveInfo info2;
            FileStream stream;
            StreamWriter writer;
            FileInfo info3;
            string filePath = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(this.path, out provider, out info2);
            if (!provider.NameEquals(base.Context.ProviderNames.FileSystem) || !filePath.EndsWith(".pssc", StringComparison.OrdinalIgnoreCase))
            {
                InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFilePath, this.path));
                ErrorRecord errorRecord = new ErrorRecord(exception, "InvalidPSSessionConfigurationFilePath", ErrorCategory.InvalidArgument, this.path);
                base.ThrowTerminatingError(errorRecord);
            }
            PathUtils.MasterStreamOpen(this, filePath, "unicode", false, false, false, false, out stream, out writer, out info3, false);
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("@{");
                builder.Append(writer.NewLine);
                builder.Append(writer.NewLine);
                builder.Append(ConfigFragment(ConfigFileContants.SchemaVersion, RemotingErrorIdStrings.DISCSchemaVersionComment, QuoteName(this.schemaVersion.ToString()), writer));
                builder.Append(ConfigFragment(ConfigFileContants.Guid, RemotingErrorIdStrings.DISCGUIDComment, QuoteName(this.guid.ToString()), writer));
                builder.Append(ConfigFragment(ConfigFileContants.ExecutionPolicy, RemotingErrorIdStrings.DISCExecutionPolicyComment, QuoteName(this.executionPolicy), writer));
                if (!this.isLanguageModeSpecified && (this.initialSessionState == System.Management.Automation.Remoting.SessionType.Default))
                {
                    this.languageMode = PSLanguageMode.FullLanguage;
                }
                builder.Append(ConfigFragment(ConfigFileContants.LanguageMode, RemotingErrorIdStrings.DISCLanguageModeComment, QuoteName(this.languageMode), writer));
                builder.Append(ConfigFragment(ConfigFileContants.SessionType, RemotingErrorIdStrings.DISCInitialSessionStateComment, QuoteName(this.initialSessionState.ToString()), writer));
                if (this.environmentVariables == null)
                {
                    builder.Append(ConfigFragment(ConfigFileContants.EnvironmentVariables, RemotingErrorIdStrings.DISCEnvironmentVariablesComment, string.Empty, writer));
                }
                else
                {
                    string environmentVariables = this.environmentVariables as string;
                    if (environmentVariables != null)
                    {
                        builder.Append(ConfigFragment(ConfigFileContants.EnvironmentVariables, RemotingErrorIdStrings.DISCEnvironmentVariablesComment, environmentVariables, writer));
                    }
                    else
                    {
                        Hashtable table = this.environmentVariables as Hashtable;
                        if (table != null)
                        {
                            builder.Append(ConfigFragment(ConfigFileContants.EnvironmentVariables, RemotingErrorIdStrings.DISCEnvironmentVariablesComment, this.CombineHashtable(table, writer), writer));
                        }
                        else
                        {
                            builder.Append(ConfigFragment(ConfigFileContants.EnvironmentVariables, RemotingErrorIdStrings.DISCEnvironmentVariablesComment, string.Empty, writer));
                        }
                    }
                }
                if (string.IsNullOrEmpty(this.author))
                {
                    this.author = Environment.UserName;
                }
                builder.Append(ConfigFragment(ConfigFileContants.Author, RemotingErrorIdStrings.DISCAuthorComment, QuoteName(this.author), writer));
                if (string.IsNullOrEmpty(this.companyName))
                {
                    this.companyName = Modules.DefaultCompanyName;
                }
                builder.Append(ConfigFragment(ConfigFileContants.CompanyName, RemotingErrorIdStrings.DISCCompanyNameComment, QuoteName(this.companyName), writer));
                if (string.IsNullOrEmpty(this.copyright))
                {
                    this.copyright = StringUtil.Format(Modules.DefaultCopyrightMessage, DateTime.Now.Year, this.author);
                }
                builder.Append(ConfigFragment(ConfigFileContants.Copyright, RemotingErrorIdStrings.DISCCopyrightComment, QuoteName(this.copyright), writer));
                builder.Append(ConfigFragment(ConfigFileContants.Description, RemotingErrorIdStrings.DISCDescriptionComment, string.IsNullOrEmpty(this.description) ? string.Empty : QuoteName(this.description), writer));
                builder.Append(ConfigFragment(ConfigFileContants.PowerShellVersion, RemotingErrorIdStrings.DISCPowerShellVersionComment, (this.powerShellVersion != null) ? QuoteName(this.powerShellVersion.ToString()) : string.Empty, writer));
                builder.Append(ConfigFragment(ConfigFileContants.ModulesToImport, RemotingErrorIdStrings.DISCModulesToImportComment, (this.modulesToImport != null) ? this.CombineHashTableOrStringArray(this.modulesToImport, writer) : string.Empty, writer));
                builder.Append(ConfigFragment(ConfigFileContants.AssembliesToLoad, RemotingErrorIdStrings.DISCAssembliesToLoadComment, (this.assembliesToLoad != null) ? this.CombineStringArray(this.assembliesToLoad) : string.Empty, writer));
                builder.Append(ConfigFragment(ConfigFileContants.VisibleAliases, RemotingErrorIdStrings.DISCVisibleAliasesComment, this.GetVisibilityDefault(this.visibleAliases), writer));
                builder.Append(ConfigFragment(ConfigFileContants.VisibleCmdlets, RemotingErrorIdStrings.DISCVisibleCmdletsComment, this.GetVisibilityDefault(this.visibleCmdlets), writer));
                builder.Append(ConfigFragment(ConfigFileContants.VisibleFunctions, RemotingErrorIdStrings.DISCVisibleFunctionsComment, this.GetVisibilityDefault(this.visibleFunctions), writer));
                builder.Append(ConfigFragment(ConfigFileContants.VisibleProviders, RemotingErrorIdStrings.DISCVisibleProvidersComment, this.GetVisibilityDefault(this.visibleProviders), writer));
                builder.Append(ConfigFragment(ConfigFileContants.AliasDefinitions, RemotingErrorIdStrings.DISCAliasDefinitionsComment, (this.aliasDefinitions != null) ? this.CombineHashtableArray(this.aliasDefinitions, writer) : string.Empty, writer));
                if (this.functionDefinitions == null)
                {
                    builder.Append(ConfigFragment(ConfigFileContants.FunctionDefinitions, RemotingErrorIdStrings.DISCFunctionDefinitionsComment, string.Empty, writer));
                }
                else
                {
                    Hashtable[] tables = DISCPowerShellConfiguration.TryGetHashtableArray(this.functionDefinitions);
                    if (tables != null)
                    {
                        builder.Append(ConfigFragment(ConfigFileContants.FunctionDefinitions, RemotingErrorIdStrings.DISCFunctionDefinitionsComment, this.CombineHashtableArray(tables, writer), writer));
                        foreach (Hashtable hashtable2 in tables)
                        {
                            if (!hashtable2.ContainsKey(ConfigFileContants.FunctionNameToken))
                            {
                                PSArgumentException exception2 = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, new object[] { ConfigFileContants.FunctionDefinitions, ConfigFileContants.FunctionNameToken, this.path }));
                                base.ThrowTerminatingError(exception2.ErrorRecord);
                            }
                            if (!hashtable2.ContainsKey(ConfigFileContants.FunctionValueToken))
                            {
                                PSArgumentException exception3 = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, new object[] { ConfigFileContants.FunctionDefinitions, ConfigFileContants.FunctionValueToken, this.path }));
                                base.ThrowTerminatingError(exception3.ErrorRecord);
                            }
                            if (!(hashtable2[ConfigFileContants.FunctionValueToken] is ScriptBlock))
                            {
                                PSArgumentException exception4 = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCKeyMustBeScriptBlock, new object[] { ConfigFileContants.FunctionValueToken, ConfigFileContants.FunctionDefinitions, this.path }));
                                base.ThrowTerminatingError(exception4.ErrorRecord);
                            }
                            foreach (string str4 in hashtable2.Keys)
                            {
                                if ((!string.Equals(str4, ConfigFileContants.FunctionNameToken, StringComparison.OrdinalIgnoreCase) && !string.Equals(str4, ConfigFileContants.FunctionValueToken, StringComparison.OrdinalIgnoreCase)) && !string.Equals(str4, ConfigFileContants.FunctionOptionsToken, StringComparison.OrdinalIgnoreCase))
                                {
                                    PSArgumentException exception5 = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeContainsInvalidKey, new object[] { str4, ConfigFileContants.FunctionDefinitions, this.path }));
                                    base.ThrowTerminatingError(exception5.ErrorRecord);
                                }
                            }
                        }
                    }
                    else
                    {
                        PSArgumentException exception6 = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeHashtableArray, ConfigFileContants.FunctionDefinitions, filePath));
                        base.ThrowTerminatingError(exception6.ErrorRecord);
                    }
                }
                if (this.variableDefinitions == null)
                {
                    builder.Append(ConfigFragment(ConfigFileContants.VariableDefinitions, RemotingErrorIdStrings.DISCVariableDefinitionsComment, string.Empty, writer));
                }
                else
                {
                    string variableDefinitions = this.variableDefinitions as string;
                    if (variableDefinitions != null)
                    {
                        builder.Append(ConfigFragment(ConfigFileContants.VariableDefinitions, RemotingErrorIdStrings.DISCVariableDefinitionsComment, variableDefinitions, writer));
                    }
                    else
                    {
                        Hashtable[] hashtableArray2 = DISCPowerShellConfiguration.TryGetHashtableArray(this.variableDefinitions);
                        if (hashtableArray2 != null)
                        {
                            builder.Append(ConfigFragment(ConfigFileContants.VariableDefinitions, RemotingErrorIdStrings.DISCVariableDefinitionsComment, this.CombineHashtableArray(hashtableArray2, writer), writer));
                            foreach (Hashtable hashtable3 in hashtableArray2)
                            {
                                if (!hashtable3.ContainsKey(ConfigFileContants.VariableNameToken))
                                {
                                    PSArgumentException exception7 = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, new object[] { ConfigFileContants.VariableDefinitions, ConfigFileContants.VariableNameToken, this.path }));
                                    base.ThrowTerminatingError(exception7.ErrorRecord);
                                }
                                if (!hashtable3.ContainsKey(ConfigFileContants.VariableValueToken))
                                {
                                    PSArgumentException exception8 = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustContainKey, new object[] { ConfigFileContants.VariableDefinitions, ConfigFileContants.VariableValueToken, this.path }));
                                    base.ThrowTerminatingError(exception8.ErrorRecord);
                                }
                                foreach (string str6 in hashtable3.Keys)
                                {
                                    if (!string.Equals(str6, ConfigFileContants.VariableNameToken, StringComparison.OrdinalIgnoreCase) && !string.Equals(str6, ConfigFileContants.VariableValueToken, StringComparison.OrdinalIgnoreCase))
                                    {
                                        PSArgumentException exception9 = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeContainsInvalidKey, new object[] { str6, ConfigFileContants.VariableDefinitions, this.path }));
                                        base.ThrowTerminatingError(exception9.ErrorRecord);
                                    }
                                }
                            }
                        }
                        else
                        {
                            PSArgumentException exception10 = new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.DISCTypeMustBeHashtableArray, ConfigFileContants.VariableDefinitions, filePath));
                            base.ThrowTerminatingError(exception10.ErrorRecord);
                        }
                    }
                }
                builder.Append(ConfigFragment(ConfigFileContants.TypesToProcess, RemotingErrorIdStrings.DISCTypesToProcessComment, (this.typesToProcess != null) ? this.CombineStringArray(this.typesToProcess) : string.Empty, writer));
                builder.Append(ConfigFragment(ConfigFileContants.FormatsToProcess, RemotingErrorIdStrings.DISCFormatsToProcessComment, (this.formatsToProcess != null) ? this.CombineStringArray(this.formatsToProcess) : string.Empty, writer));
                builder.Append(ConfigFragment(ConfigFileContants.ScriptsToProcess, RemotingErrorIdStrings.DISCScriptsToProcessComment, (this.scriptsToProcess != null) ? this.CombineStringArray(this.scriptsToProcess) : string.Empty, writer));
                builder.Append("}");
                builder.Append(writer.NewLine);
                builder.Append(writer.NewLine);
                writer.Write(builder.ToString());
            }
            finally
            {
                writer.Close();
            }
        }

        private static string QuoteName(object name)
        {
            if (name == null)
            {
                return "''";
            }
            return ("'" + name.ToString().Replace("'", "''") + "'");
        }

        private static string WrapScriptBlock(object sb)
        {
            if (sb == null)
            {
                return "{}";
            }
            return ("{" + sb.ToString() + "}");
        }

        [Parameter]
        public Hashtable[] AliasDefinitions
        {
            get
            {
                return this.aliasDefinitions;
            }
            set
            {
                this.aliasDefinitions = value;
            }
        }

        [Parameter]
        public string[] AssembliesToLoad
        {
            get
            {
                return this.assembliesToLoad;
            }
            set
            {
                this.assembliesToLoad = value;
            }
        }

        [Parameter]
        public string Author
        {
            get
            {
                return this.author;
            }
            set
            {
                this.author = value;
            }
        }

        [Parameter]
        public string CompanyName
        {
            get
            {
                return this.companyName;
            }
            set
            {
                this.companyName = value;
            }
        }

        [Parameter]
        public string Copyright
        {
            get
            {
                return this.copyright;
            }
            set
            {
                this.copyright = value;
            }
        }

        [Parameter]
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

        [Parameter]
        public object EnvironmentVariables
        {
            get
            {
                return this.environmentVariables;
            }
            set
            {
                this.environmentVariables = value;
            }
        }

        [Parameter]
        public Microsoft.PowerShell.ExecutionPolicy ExecutionPolicy
        {
            get
            {
                return this.executionPolicy;
            }
            set
            {
                this.executionPolicy = value;
            }
        }

        [Parameter]
        public string[] FormatsToProcess
        {
            get
            {
                return this.formatsToProcess;
            }
            set
            {
                this.formatsToProcess = value;
            }
        }

        [Parameter]
        public Hashtable[] FunctionDefinitions
        {
            get
            {
                return this.functionDefinitions;
            }
            set
            {
                this.functionDefinitions = value;
            }
        }

        [Parameter]
        public System.Guid Guid
        {
            get
            {
                return this.guid;
            }
            set
            {
                this.guid = value;
            }
        }

        [Parameter]
        public PSLanguageMode LanguageMode
        {
            get
            {
                return this.languageMode;
            }
            set
            {
                this.languageMode = value;
                this.isLanguageModeSpecified = true;
            }
        }

        [Parameter]
        public object[] ModulesToImport
        {
            get
            {
                return this.modulesToImport;
            }
            set
            {
                this.modulesToImport = value;
            }
        }

        [Parameter(Position=0, Mandatory=true), ValidateNotNullOrEmpty]
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }

        [Parameter]
        public Version PowerShellVersion
        {
            get
            {
                return this.powerShellVersion;
            }
            set
            {
                this.powerShellVersion = value;
            }
        }

        [ValidateNotNull, Parameter]
        public Version SchemaVersion
        {
            get
            {
                return this.schemaVersion;
            }
            set
            {
                this.schemaVersion = value;
            }
        }

        [Parameter]
        public string[] ScriptsToProcess
        {
            get
            {
                return this.scriptsToProcess;
            }
            set
            {
                this.scriptsToProcess = value;
            }
        }

        [Parameter]
        public System.Management.Automation.Remoting.SessionType SessionType
        {
            get
            {
                return this.initialSessionState;
            }
            set
            {
                this.initialSessionState = value;
            }
        }

        [Parameter]
        public string[] TypesToProcess
        {
            get
            {
                return this.typesToProcess;
            }
            set
            {
                this.typesToProcess = value;
            }
        }

        [Parameter]
        public object VariableDefinitions
        {
            get
            {
                return this.variableDefinitions;
            }
            set
            {
                this.variableDefinitions = value;
            }
        }

        [Parameter]
        public string[] VisibleAliases
        {
            get
            {
                return this.visibleAliases;
            }
            set
            {
                this.visibleAliases = value;
            }
        }

        [Parameter]
        public string[] VisibleCmdlets
        {
            get
            {
                return this.visibleCmdlets;
            }
            set
            {
                this.visibleCmdlets = value;
            }
        }

        [Parameter]
        public string[] VisibleFunctions
        {
            get
            {
                return this.visibleFunctions;
            }
            set
            {
                this.visibleFunctions = value;
            }
        }

        [Parameter]
        public string[] VisibleProviders
        {
            get
            {
                return this.visibleProviders;
            }
            set
            {
                this.visibleProviders = value;
            }
        }
    }
}

