namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation.Provider;
    using System.Reflection;
    using System.Security;
    using System.Threading;

    public class ProviderInfo
    {
        private PSModuleInfo _module;
        private PSNoteProperty _noteProperty;
        private ProviderCapabilities capabilities;
        private bool capabilitiesRead;
        private string description;
        private string helpFile;
        private PSDriveInfo hiddenDrive;
        private string home;
        private Type implementingType;
        private string name;
        private Dictionary<string, List<PSTypeName>> providerOutputType;
        private PSSnapInInfo pssnapin;
        private SessionState sessionState;

        protected ProviderInfo(ProviderInfo providerInfo)
        {
            this.helpFile = "";
            if (providerInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerInfo");
            }
            this.name = providerInfo.Name;
            this.implementingType = providerInfo.ImplementingType;
            this.capabilities = providerInfo.capabilities;
            this.description = providerInfo.description;
            this.hiddenDrive = providerInfo.hiddenDrive;
            this.home = providerInfo.home;
            this.helpFile = providerInfo.helpFile;
            this.pssnapin = providerInfo.pssnapin;
            this.sessionState = providerInfo.sessionState;
        }

        internal ProviderInfo(SessionState sessionState, Type implementingType, string name, string helpFile, PSSnapInInfo psSnapIn) : this(sessionState, implementingType, name, string.Empty, string.Empty, helpFile, psSnapIn)
        {
        }

        internal ProviderInfo(SessionState sessionState, Type implementingType, string name, string description, string home, string helpFile, PSSnapInInfo psSnapIn)
        {
            this.helpFile = "";
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            if (implementingType == null)
            {
                throw PSTraceSource.NewArgumentNullException("implementingType");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            this.sessionState = sessionState;
            this.name = name;
            this.description = description;
            this.home = home;
            this.implementingType = implementingType;
            this.helpFile = helpFile;
            this.pssnapin = psSnapIn;
            this.hiddenDrive = new PSDriveInfo(this.FullName, this, "", "", null);
            this.hiddenDrive.Hidden = true;
        }

        internal CmdletProvider CreateInstance()
        {
            object obj2 = null;
            Exception innerException = null;
            try
            {
                obj2 = Activator.CreateInstance(this.ImplementingType);
            }
            catch (TargetInvocationException exception2)
            {
                innerException = exception2.InnerException;
            }
            catch (MissingMethodException)
            {
            }
            catch (MemberAccessException)
            {
            }
            catch (ArgumentException)
            {
            }
            if (obj2 == null)
            {
                ProviderNotFoundException exception3 = null;
                if (innerException != null)
                {
                    exception3 = new ProviderNotFoundException(this.Name, SessionStateCategory.CmdletProvider, "ProviderCtorException", SessionStateStrings.ProviderCtorException, new object[] { innerException.Message });
                }
                else
                {
                    exception3 = new ProviderNotFoundException(this.Name, SessionStateCategory.CmdletProvider, "ProviderNotFoundInAssembly", SessionStateStrings.ProviderNotFoundInAssembly, new object[0]);
                }
                throw exception3;
            }
            CmdletProvider provider = obj2 as CmdletProvider;
            provider.SetProviderInformation(this);
            return provider;
        }

        internal PSNoteProperty GetNotePropertyForProviderCmdlets(string name)
        {
            if (this._noteProperty == null)
            {
                Interlocked.CompareExchange<PSNoteProperty>(ref this._noteProperty, new PSNoteProperty(name, this), null);
            }
            return this._noteProperty;
        }

        internal void GetOutputTypes(string cmdletname, List<PSTypeName> listToAppend)
        {
            if (this.providerOutputType == null)
            {
                this.providerOutputType = new Dictionary<string, List<PSTypeName>>();
                foreach (Attribute attribute in this.implementingType.GetCustomAttributes(typeof(OutputTypeAttribute), false))
                {
                    OutputTypeAttribute attribute2 = (OutputTypeAttribute) attribute;
                    if (!string.IsNullOrEmpty(attribute2.ProviderCmdlet))
                    {
                        List<PSTypeName> list;
                        if (!this.providerOutputType.TryGetValue(attribute2.ProviderCmdlet, out list))
                        {
                            list = new List<PSTypeName>();
                            this.providerOutputType[attribute2.ProviderCmdlet] = list;
                        }
                        list.AddRange(attribute2.Type);
                    }
                }
            }
            List<PSTypeName> list2 = null;
            if (this.providerOutputType.TryGetValue(cmdletname, out list2))
            {
                listToAppend.AddRange(list2);
            }
        }

        internal bool IsMatch(string providerName)
        {
            PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(providerName);
            WildcardPattern namePattern = null;
            if ((instance != null) && WildcardPattern.ContainsWildcardCharacters(instance.ShortName))
            {
                namePattern = new WildcardPattern(instance.ShortName, WildcardOptions.IgnoreCase);
            }
            return this.IsMatch(namePattern, instance);
        }

        internal bool IsMatch(WildcardPattern namePattern, PSSnapinQualifiedName psSnapinQualifiedName)
        {
            bool flag = false;
            if (psSnapinQualifiedName == null)
            {
                return true;
            }
            if (namePattern == null)
            {
                if (string.Equals(this.Name, psSnapinQualifiedName.ShortName, StringComparison.OrdinalIgnoreCase) && this.IsPSSnapinNameMatch(psSnapinQualifiedName))
                {
                    flag = true;
                }
                return flag;
            }
            if (namePattern.IsMatch(this.Name) && this.IsPSSnapinNameMatch(psSnapinQualifiedName))
            {
                flag = true;
            }
            return flag;
        }

        private bool IsPSSnapinNameMatch(PSSnapinQualifiedName psSnapinQualifiedName)
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(psSnapinQualifiedName.PSSnapInName) && !string.Equals(psSnapinQualifiedName.PSSnapInName, this.PSSnapInName, StringComparison.OrdinalIgnoreCase))
            {
                return flag;
            }
            return true;
        }

        internal bool NameEquals(string providerName)
        {
            PSSnapinQualifiedName instance = PSSnapinQualifiedName.GetInstance(providerName);
            bool flag = false;
            if (instance != null)
            {
                if ((!string.IsNullOrEmpty(instance.PSSnapInName) && !string.Equals(instance.PSSnapInName, this.PSSnapInName, StringComparison.OrdinalIgnoreCase)) && !string.Equals(instance.PSSnapInName, this.ModuleName, StringComparison.OrdinalIgnoreCase))
                {
                    return flag;
                }
                return string.Equals(instance.ShortName, this.Name, StringComparison.OrdinalIgnoreCase);
            }
            return string.Equals(providerName, this.Name, StringComparison.OrdinalIgnoreCase);
        }

        internal void SetModule(PSModuleInfo module)
        {
            this._module = module;
        }

        public override string ToString()
        {
            return this.FullName;
        }

        internal string ApplicationBase
        {
            get
            {
                try
                {
                    return Utils.GetApplicationBase(Utils.DefaultPowerShellShellID);
                }
                catch (SecurityException)
                {
                    return null;
                }
            }
        }

        public ProviderCapabilities Capabilities
        {
            get
            {
                if (!this.capabilitiesRead)
                {
                    try
                    {
                        object[] customAttributes = this.ImplementingType.GetCustomAttributes(typeof(CmdletProviderAttribute), false);
                        if ((customAttributes != null) && (customAttributes.Length == 1))
                        {
                            CmdletProviderAttribute attribute = (CmdletProviderAttribute) customAttributes[0];
                            this.capabilities = attribute.ProviderCapabilities;
                            this.capabilitiesRead = true;
                        }
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                    }
                }
                return this.capabilities;
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

        public Collection<PSDriveInfo> Drives
        {
            get
            {
                return this.sessionState.Drive.GetAllForProvider(this.FullName);
            }
        }

        internal string FullName
        {
            get
            {
                string name = this.Name;
                if (!string.IsNullOrEmpty(this.PSSnapInName))
                {
                    return string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", new object[] { this.PSSnapInName, this.Name });
                }
                if (!string.IsNullOrEmpty(this.ModuleName))
                {
                    name = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", new object[] { this.ModuleName, this.Name });
                }
                return name;
            }
        }

        public string HelpFile
        {
            get
            {
                return this.helpFile;
            }
        }

        internal PSDriveInfo HiddenDrive
        {
            get
            {
                return this.hiddenDrive;
            }
        }

        public string Home
        {
            get
            {
                return this.home;
            }
            set
            {
                this.home = value;
            }
        }

        public Type ImplementingType
        {
            get
            {
                return this.implementingType;
            }
        }

        public PSModuleInfo Module
        {
            get
            {
                return this._module;
            }
        }

        public string ModuleName
        {
            get
            {
                if (this.pssnapin != null)
                {
                    return this.pssnapin.Name;
                }
                if (this._module != null)
                {
                    return this._module.Name;
                }
                return string.Empty;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public PSSnapInInfo PSSnapIn
        {
            get
            {
                return this.pssnapin;
            }
        }

        internal string PSSnapInName
        {
            get
            {
                string name = null;
                if (this.pssnapin != null)
                {
                    name = this.pssnapin.Name;
                }
                return name;
            }
        }
    }
}

