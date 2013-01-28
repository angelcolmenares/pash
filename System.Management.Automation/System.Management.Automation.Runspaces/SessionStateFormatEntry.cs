namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class SessionStateFormatEntry : InitialSessionStateEntry
    {
        private string _fileName;
        private FormatTable _formattable;
        private ExtendedTypeDefinition _typeDefinition;

        public SessionStateFormatEntry(ExtendedTypeDefinition typeDefinition) : base("*")
        {
            if (typeDefinition == null)
            {
                throw PSTraceSource.NewArgumentNullException("typeDefinition");
            }
            this._typeDefinition = typeDefinition;
        }

        public SessionStateFormatEntry(FormatTable formattable) : base("*")
        {
            if (formattable == null)
            {
                throw PSTraceSource.NewArgumentNullException("formattable");
            }
            this._formattable = formattable;
        }

        public SessionStateFormatEntry(string fileName) : base("*")
        {
            if (string.IsNullOrEmpty(fileName) || (fileName.Trim().Length == 0))
            {
                throw PSTraceSource.NewArgumentException("fileName");
            }
            this._fileName = fileName.Trim();
        }

        public override InitialSessionStateEntry Clone()
        {
            SessionStateFormatEntry entry;
            if (this._fileName != null)
            {
                entry = new SessionStateFormatEntry(this._fileName);
            }
            else if (this._formattable != null)
            {
                entry = new SessionStateFormatEntry(this._formattable);
            }
            else
            {
                entry = new SessionStateFormatEntry(this._typeDefinition);
            }
            entry.SetPSSnapIn(base.PSSnapIn);
            entry.SetModule(base.Module);
            return entry;
        }

        public string FileName
        {
            get
            {
                return this._fileName;
            }
        }

        public ExtendedTypeDefinition FormatData
        {
            get
            {
                return this._typeDefinition;
            }
        }

        public FormatTable Formattable
        {
            get
            {
                return this._formattable;
            }
        }
    }
}

