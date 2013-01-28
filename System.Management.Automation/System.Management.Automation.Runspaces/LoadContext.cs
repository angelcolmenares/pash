namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct LoadContext
    {
        private const string FileLineError = "FileLineError";
        private const string FileLineTypeError = "FileLineTypeError";
        internal const string FileError = "FileError";
        internal int lineNumber;
        internal XmlTextReader reader;
        internal Collection<string> errors;
        internal string fileName;
        internal string PSSnapinName;
        internal bool isFullyTrusted;
        internal LoadContext(string PSSnapinName, string fileName, Collection<string> errors)
        {
            this.reader = null;
            this.fileName = fileName;
            this.errors = errors;
            this.PSSnapinName = PSSnapinName;
            this.lineNumber = 0;
            this.isFullyTrusted = false;
        }

        internal bool Read()
        {
            this.lineNumber = this.reader.LineNumber;
            return this.reader.Read();
        }

        internal bool IsFullyTrusted
        {
            get
            {
                return this.isFullyTrusted;
            }
            set
            {
                this.isFullyTrusted = value;
            }
        }
        internal void AddError(string resourceString, params object[] formatArguments)
        {
            string str = StringUtil.Format(resourceString, formatArguments);
            string item = StringUtil.Format(TypesXmlStrings.FileError, new object[] { this.PSSnapinName, this.fileName, str });
            this.errors.Add(item);
        }

        internal void AddError(int errorLineNumber, string resourceString, params object[] formatArguments)
        {
            string str = StringUtil.Format(resourceString, formatArguments);
            string item = StringUtil.Format(TypesXmlStrings.FileLineError, new object[] { this.PSSnapinName, this.fileName, errorLineNumber, str });
            this.errors.Add(item);
        }

        internal void AddError(string typeName, int errorLineNumber, string resourceString, params object[] formatArguments)
        {
            string str = StringUtil.Format(resourceString, formatArguments);
            string item = StringUtil.Format(TypesXmlStrings.FileLineTypeError, new object[] { this.PSSnapinName, this.fileName, errorLineNumber, typeName, str });
            this.errors.Add(item);
        }
    }
}

