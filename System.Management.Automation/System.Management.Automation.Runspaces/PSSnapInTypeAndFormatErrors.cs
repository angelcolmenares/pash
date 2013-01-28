namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    internal class PSSnapInTypeAndFormatErrors
    {
        private Collection<string> errors;
        internal bool FailToLoadFile;
        private System.Management.Automation.Runspaces.FormatTable formatTable;
        private string fullPath;
        private bool isRemove;
        public string psSnapinName;
        private System.Management.Automation.Runspaces.TypeData typeData;
        private ExtendedTypeDefinition typeDefinition;

        internal PSSnapInTypeAndFormatErrors(string psSnapinName, ExtendedTypeDefinition typeDefinition)
        {
            this.psSnapinName = psSnapinName;
            this.typeDefinition = typeDefinition;
            this.errors = new Collection<string>();
        }

        internal PSSnapInTypeAndFormatErrors(string psSnapinName, System.Management.Automation.Runspaces.FormatTable formatTable)
        {
            this.psSnapinName = psSnapinName;
            this.formatTable = formatTable;
            this.errors = new Collection<string>();
        }

        internal PSSnapInTypeAndFormatErrors(string psSnapinName, string fullPath)
        {
            this.psSnapinName = psSnapinName;
            this.fullPath = fullPath;
            this.errors = new Collection<string>();
        }

        internal PSSnapInTypeAndFormatErrors(string psSnapinName, System.Management.Automation.Runspaces.TypeData typeData, bool isRemove)
        {
            this.psSnapinName = psSnapinName;
            this.typeData = typeData;
            this.isRemove = isRemove;
            this.errors = new Collection<string>();
        }

        internal Collection<string> Errors
        {
            get
            {
                return this.errors;
            }
            set
            {
                this.errors = value;
            }
        }

        internal ExtendedTypeDefinition FormatData
        {
            get
            {
                return this.typeDefinition;
            }
        }

        internal System.Management.Automation.Runspaces.FormatTable FormatTable
        {
            get
            {
                return this.formatTable;
            }
        }

        internal string FullPath
        {
            get
            {
                return this.fullPath;
            }
        }

        internal bool IsRemove
        {
            get
            {
                return this.isRemove;
            }
        }

        internal string PSSnapinName
        {
            get
            {
                return this.psSnapinName;
            }
        }

        internal System.Management.Automation.Runspaces.TypeData TypeData
        {
            get
            {
                return this.typeData;
            }
        }
    }
}

