namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public class CmdletInfo : CommandInfo
    {
        private List<PSTypeName> _outputType;
        private PSSnapInInfo _PSSnapin;
        private System.Management.Automation.CommandMetadata cmdletMetadata;
        private string helpFilePath;
        private Type implementingType;
        private string noun;
        private ScopedItemOptions options;
        private string verb;

        internal CmdletInfo(CmdletInfo other) : base(other)
        {
            this.verb = string.Empty;
            this.noun = string.Empty;
            this.helpFilePath = string.Empty;
            this.verb = other.verb;
            this.noun = other.noun;
            this.implementingType = other.implementingType;
            this.helpFilePath = other.helpFilePath;
            this._PSSnapin = other._PSSnapin;
            this.options = ScopedItemOptions.ReadOnly;
        }

        public CmdletInfo(string name, Type implementingType) : base(name, CommandTypes.Cmdlet, null)
        {
            this.verb = string.Empty;
            this.noun = string.Empty;
            this.helpFilePath = string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            if (implementingType == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            if (!typeof(Cmdlet).IsAssignableFrom(implementingType))
            {
                throw PSTraceSource.NewInvalidOperationException("DiscoveryExceptions", "CmdletDoesNotDeriveFromCmdletType", new object[] { "implementingType", implementingType.FullName });
            }
            if (!SplitCmdletName(name, out this.verb, out this.noun))
            {
                throw PSTraceSource.NewArgumentException("name", "DiscoveryExceptions", "InvalidCmdletNameFormat", new object[] { name });
            }
            this.implementingType = implementingType;
            this.helpFilePath = string.Empty;
            this._PSSnapin = null;
            this.options = ScopedItemOptions.ReadOnly;
        }

        internal CmdletInfo(string name, Type implementingType, string helpFile, PSSnapInInfo PSSnapin, System.Management.Automation.ExecutionContext context) : base(name, CommandTypes.Cmdlet, context)
        {
            this.verb = string.Empty;
            this.noun = string.Empty;
            this.helpFilePath = string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (!SplitCmdletName(name, out this.verb, out this.noun))
            {
                throw PSTraceSource.NewArgumentException("name", "DiscoveryExceptions", "InvalidCmdletNameFormat", new object[] { name });
            }
            this.implementingType = implementingType;
            this.helpFilePath = helpFile;
            this._PSSnapin = PSSnapin;
            this.options = ScopedItemOptions.ReadOnly;
            base.DefiningLanguageMode = 0;
        }

        internal override CommandInfo CreateGetCommandCopy(object[] arguments)
        {
            return new CmdletInfo(this) { IsGetCommandCopy = true, Arguments = arguments };
        }

        private static string GetFullName(CmdletInfo cmdletInfo)
        {
            return GetFullName(cmdletInfo.ModuleName, cmdletInfo.Name);
        }

        internal static string GetFullName(PSObject psObject)
        {
            if (psObject.BaseObject is CmdletInfo)
            {
                CmdletInfo baseObject = (CmdletInfo) psObject.BaseObject;
                return GetFullName(baseObject);
            }
            PSPropertyInfo info2 = psObject.Properties["Name"];
            PSPropertyInfo info3 = psObject.Properties["PSSnapIn"];
            string cmdletName = (info2 == null) ? "" : ((string) info2.Value);
            string moduleName = (info3 == null) ? "" : ((string) info3.Value);
            return GetFullName(moduleName, cmdletName);
        }

        private static string GetFullName(string moduleName, string cmdletName)
        {
            string str = cmdletName;
            if (!string.IsNullOrEmpty(moduleName))
            {
                str = moduleName + '\\' + str;
            }
            return str;
        }

        internal void SetOptions(ScopedItemOptions newOptions, bool force)
        {
            if ((this.options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None)
            {
                SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Cmdlet, "CmdletIsReadOnly", SessionStateStrings.CmdletIsReadOnly);
                throw exception;
            }
            this.options = newOptions;
        }

        internal static bool SplitCmdletName(string name, out string verb, out string noun)
        {
            noun = verb = string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                int length = 0;
                for (int i = 0; i < name.Length; i++)
                {
                    if (SpecialCharacters.IsDash(name[i]))
                    {
                        length = i;
                        break;
                    }
                }
                if (length > 0)
                {
                    verb = name.Substring(0, length);
                    noun = name.Substring(length + 1);
                    return true;
                }
            }
            return false;
        }

        internal override System.Management.Automation.CommandMetadata CommandMetadata
        {
            get
            {
                if (this.cmdletMetadata == null)
                {
                    this.cmdletMetadata = System.Management.Automation.CommandMetadata.Get(base.Name, this.ImplementingType, base.Context);
                }
                return this.cmdletMetadata;
            }
        }

        public string DefaultParameterSet
        {
            get
            {
                return this.CommandMetadata.DefaultParameterSetName;
            }
        }

        public override string Definition
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                if (this.ImplementingType != null)
                {
                    foreach (CommandParameterSetInfo info in base.ParameterSets)
                    {
                        builder.AppendLine();
                        builder.AppendLine(string.Format(Thread.CurrentThread.CurrentCulture, "{0}{1}{2} {3}", new object[] { this.verb, '-', this.noun, info.ToString((base.CommandType & CommandTypes.Workflow) == CommandTypes.Workflow) }));
                    }
                }
                else
                {
                    builder.AppendLine(string.Format(Thread.CurrentThread.CurrentCulture, "{0}{1}{2}", new object[] { this.verb, '-', this.noun }));
                }
                return builder.ToString();
            }
        }

        internal string FullName
        {
            get
            {
                return GetFullName(this);
            }
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.Cmdlet;
            }
        }

        public string HelpFile
        {
            get
            {
                return this.helpFilePath;
            }
        }

        public Type ImplementingType
        {
            get
            {
                return this.implementingType;
            }
        }

        internal override bool ImplementsDynamicParameters
        {
            get
            {
                return ((this.ImplementingType != null) && (this.ImplementingType.GetInterface(typeof(IDynamicParameters).Name, true) != null));
            }
        }

        public string Noun
        {
            get
            {
                return this.noun;
            }
        }

        public ScopedItemOptions Options
        {
            get
            {
                return this.options;
            }
            set
            {
                this.SetOptions(value, false);
            }
        }

        public override ReadOnlyCollection<PSTypeName> OutputType
        {
            get
            {
                if (this._outputType == null)
                {
                    this._outputType = new List<PSTypeName>();
                    if (this.ImplementingType != null)
                    {
                        foreach (object obj2 in this.ImplementingType.GetCustomAttributes(typeof(OutputTypeAttribute), false))
                        {
                            OutputTypeAttribute attribute = (OutputTypeAttribute) obj2;
                            this._outputType.AddRange(attribute.Type);
                        }
                    }
                }
                List<PSTypeName> listToAppend = new List<PSTypeName>();
                if (base.Context != null)
                {
                    ProviderInfo provider = null;
                    if (base.Arguments != null)
                    {
                        for (int i = 0; i < (base.Arguments.Length - 1); i++)
                        {
                            string str = base.Arguments[i] as string;
                            if ((str != null) && (str.Equals("-Path", StringComparison.OrdinalIgnoreCase) || str.Equals("-LiteralPath", StringComparison.OrdinalIgnoreCase)))
                            {
                                string path = base.Arguments[i + 1] as string;
                                if (path != null)
                                {
                                    base.Context.SessionState.Path.GetResolvedProviderPathFromPSPath(path, true, out provider);
                                }
                            }
                        }
                    }
                    if (provider == null)
                    {
                        provider = base.Context.SessionState.Path.CurrentLocation.Provider;
                    }
                    provider.GetOutputTypes(base.Name, listToAppend);
                    if (listToAppend.Count > 0)
                    {
                        listToAppend.InsertRange(0, this._outputType);
                        return new ReadOnlyCollection<PSTypeName>(listToAppend);
                    }
                }
                return new ReadOnlyCollection<PSTypeName>(this._outputType);
            }
        }

        public PSSnapInInfo PSSnapIn
        {
            get
            {
                return this._PSSnapin;
            }
        }

        internal string PSSnapInName
        {
            get
            {
                string name = null;
                if (this._PSSnapin != null)
                {
                    name = this._PSSnapin.Name;
                }
                return name;
            }
        }

        public string Verb
        {
            get
            {
                return this.verb;
            }
        }
    }
}

