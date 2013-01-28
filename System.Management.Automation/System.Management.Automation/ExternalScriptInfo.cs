namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation.Host;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Security;
    using System.Text;
    using System.Threading;

    public class ExternalScriptInfo : CommandInfo, IScriptCommandInfo
    {
        private System.Management.Automation.CommandMetadata _commandMetadata;
        private Encoding _originalEncoding;
        private readonly string _path;
        private System.Management.Automation.ScriptBlock _scriptBlock;
        private string _scriptContents;
        private bool _signatureChecked;

        internal ExternalScriptInfo(ExternalScriptInfo other) : base(other)
        {
            this._path = string.Empty;
            this._path = other._path;
            this.CommonInitialization();
        }

        internal ExternalScriptInfo(string name, string path) : base(name, CommandTypes.ExternalScript)
        {
            this._path = string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            this._path = System.IO.Path.GetFullPath(path);
            this.CommonInitialization();
        }

        internal ExternalScriptInfo(string name, string path, System.Management.Automation.ExecutionContext context) : base(name, CommandTypes.ExternalScript, context)
        {
            this._path = string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            this._path = System.IO.Path.GetFullPath(path);
            this.CommonInitialization();
        }

        private void CommonInitialization()
        {
            if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce)
            {
                if (SystemPolicy.GetLockdownPolicy(this._path, null) != SystemEnforcementMode.Enforce)
                {
                    base.DefiningLanguageMode = (PSLanguageMode)0;
                }
                else
                {
                    base.DefiningLanguageMode = (PSLanguageMode)3;
                }
            }
            else
            {
                base.DefiningLanguageMode = PSLanguageMode.FullLanguage;
            }
        }

        internal override CommandInfo CreateGetCommandCopy(object[] argumentList)
        {
            return new ExternalScriptInfo(this) { IsGetCommandCopy = true, Arguments = argumentList };
        }

        private ScriptRequirements GetRequiresData()
        {
            return this.GetScriptBlockAst().ScriptRequirements;
        }

        internal ScriptBlockAst GetScriptBlockAst()
        {
            ParseError[] errorArray;
            string scriptContents = this.ScriptContents;
            if (this._scriptBlock == null)
            {
                this.ScriptBlock = System.Management.Automation.ScriptBlock.TryGetCachedScriptBlock(this._path, scriptContents);
            }
            if (this._scriptBlock != null)
            {
                return (ScriptBlockAst) this._scriptBlock.Ast;
            }
            ScriptBlockAst ast = new Parser().Parse(this._path, this.ScriptContents, null, out errorArray);
            if (errorArray.Length == 0)
            {
                this.ScriptBlock = new System.Management.Automation.ScriptBlock(ast, false);
                System.Management.Automation.ScriptBlock.CacheScriptBlock(this._scriptBlock.Clone(false), this._path, scriptContents);
            }
            return ast;
        }

        private void ReadScriptContents()
        {
            if (this._scriptContents == null)
            {
                try
                {
                    using (FileStream stream = new FileStream(this._path, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.Default))
                        {
                            this._scriptContents = reader.ReadToEnd();
                            this._originalEncoding = reader.CurrentEncoding;
                            if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce)
                            {
                                if (SystemPolicy.GetLockdownPolicy(this._path, stream.SafeFileHandle) != SystemEnforcementMode.Enforce)
                                {
                                    base.DefiningLanguageMode = (PSLanguageMode)0;
                                }
                                else
                                {
                                    base.DefiningLanguageMode = (PSLanguageMode)3;
                                }
                            }
                            else if (base.Context != null)
                            {
                                base.DefiningLanguageMode = new PSLanguageMode?(base.Context.LanguageMode);
                            }
                        }
                    }
                }
                catch (ArgumentException exception)
                {
                    ThrowCommandNotFoundException(exception);
                }
                catch (IOException exception2)
                {
                    ThrowCommandNotFoundException(exception2);
                }
                catch (NotSupportedException exception3)
                {
                    ThrowCommandNotFoundException(exception3);
                }
                catch (UnauthorizedAccessException exception4)
                {
                    ThrowCommandNotFoundException(exception4);
                }
            }
        }

        private static void ThrowCommandNotFoundException(Exception innerException)
        {
            CommandNotFoundException exception = new CommandNotFoundException(innerException.Message, innerException);
            throw exception;
        }

        public void ValidateScriptInfo(PSHost host)
        {
            if (!this._signatureChecked)
            {
                System.Management.Automation.ExecutionContext context = base.Context ?? LocalPipeline.GetExecutionContextFromTLS();
                this.ReadScriptContents();
                if (context != null)
                {
                    CommandDiscovery.ShouldRun(context, host, this, CommandOrigin.Internal);
                    this._signatureChecked = true;
                }
            }
        }

        internal int ApplicationIDLineNumber
        {
            get
            {
                return 0;
            }
        }

        internal override System.Management.Automation.CommandMetadata CommandMetadata
        {
            get
            {
                return (this._commandMetadata ?? (this._commandMetadata = new System.Management.Automation.CommandMetadata(this.ScriptBlock, base.Name, LocalPipeline.GetExecutionContextFromTLS())));
            }
        }

        public override string Definition
        {
            get
            {
                return this.Path;
            }
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.ExternalScript;
            }
        }

        internal override bool ImplementsDynamicParameters
        {
            get
            {
                try
                {
                    return this.ScriptBlock.HasDynamicParameters;
                }
                catch (ParseException)
                {
                }
                catch (ScriptRequiresException)
                {
                }
                this._scriptBlock = null;
                this._scriptContents = null;
                return false;
            }
        }

        public Encoding OriginalEncoding
        {
            get
            {
                if (this._scriptContents == null)
                {
                    this.ReadScriptContents();
                }
                return this._originalEncoding;
            }
        }

        public override ReadOnlyCollection<PSTypeName> OutputType
        {
            get
            {
                return this.ScriptBlock.OutputType;
            }
        }

        public string Path
        {
            get
            {
                return this._path;
            }
        }

        internal int PSVersionLineNumber
        {
            get
            {
                return 0;
            }
        }

        internal string RequiresApplicationID
        {
            get
            {
                ScriptRequirements requiresData = this.GetRequiresData();
                if (requiresData != null)
                {
                    return requiresData.RequiredApplicationId;
                }
                return null;
            }
        }

        internal IEnumerable<ModuleSpecification> RequiresModules
        {
            get
            {
                ScriptRequirements requiresData = this.GetRequiresData();
                if (requiresData != null)
                {
                    return requiresData.RequiredModules;
                }
                return null;
            }
        }

        internal IEnumerable<PSSnapInSpecification> RequiresPSSnapIns
        {
            get
            {
                ScriptRequirements requiresData = this.GetRequiresData();
                if (requiresData != null)
                {
                    return requiresData.RequiresPSSnapIns;
                }
                return null;
            }
        }

        internal Version RequiresPSVersion
        {
            get
            {
                ScriptRequirements requiresData = this.GetRequiresData();
                if (requiresData != null)
                {
                    return requiresData.RequiredPSVersion;
                }
                return null;
            }
        }

        public System.Management.Automation.ScriptBlock ScriptBlock
        {
            get
            {
                if (this._scriptBlock == null)
                {
                    if (!this._path.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
                    {
                        this.ValidateScriptInfo(null);
                    }
                    System.Management.Automation.ScriptBlock block = System.Management.Automation.ScriptBlock.Create(new Parser(), this._path, this.ScriptContents);
                    this.ScriptBlock = block;
                }
                return this._scriptBlock;
            }
            private set
            {
                this._scriptBlock = value;
                if (value != null)
                {
                    this._scriptBlock.LanguageMode = base.DefiningLanguageMode;
                }
            }
        }

        public string ScriptContents
        {
            get
            {
                if (this._scriptContents == null)
                {
                    this.ReadScriptContents();
                }
                return this._scriptContents;
            }
        }

        internal bool SignatureChecked
        {
            set
            {
                this._signatureChecked = value;
            }
        }

        internal override string Syntax
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (CommandParameterSetInfo info in base.ParameterSets)
                {
                    builder.AppendLine(string.Format(Thread.CurrentThread.CurrentCulture, "{0} {1}", new object[] { base.Name, info }));
                }
                return builder.ToString();
            }
        }

        public override SessionStateEntryVisibility Visibility
        {
            get
            {
                if (base.Context == null)
                {
                    return SessionStateEntryVisibility.Public;
                }
                return base.Context.EngineSessionState.CheckScriptVisibility(this._path);
            }
            set
            {
                throw PSTraceSource.NewNotImplementedException();
            }
        }
    }
}

