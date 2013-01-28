namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;

    public class ApplicationInfo : CommandInfo
    {
        private ReadOnlyCollection<PSTypeName> _outputType;
        private ExecutionContext context;
        private string extension;
        private string path;

        internal ApplicationInfo(string name, string path, ExecutionContext context) : base(name, CommandTypes.Application)
        {
            this.path = string.Empty;
            this.extension = string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            if (context == null)
            {
                throw PSTraceSource.NewArgumentNullException("context");
            }
            this.path = path;
            this.extension = System.IO.Path.GetExtension(path);
            this.context = context;
        }

        public override string Definition
        {
            get
            {
                return this.Path;
            }
        }

        public string Extension
        {
            get
            {
                return this.extension;
            }
        }

        public override ReadOnlyCollection<PSTypeName> OutputType
        {
            get
            {
                if (this._outputType == null)
                {
                    List<PSTypeName> list = new List<PSTypeName> {
                        new PSTypeName(typeof(string))
                    };
                    this._outputType = new ReadOnlyCollection<PSTypeName>(list);
                }
                return this._outputType;
            }
        }

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public override SessionStateEntryVisibility Visibility
        {
            get
            {
                return this.context.EngineSessionState.CheckApplicationVisibility(this.path);
            }
            set
            {
                throw PSTraceSource.NewNotImplementedException();
            }
        }
    }
}

