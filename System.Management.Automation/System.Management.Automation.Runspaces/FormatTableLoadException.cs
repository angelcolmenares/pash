namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class FormatTableLoadException : RuntimeException
    {
        private Collection<string> errors;

        public FormatTableLoadException()
        {
            this.SetDefaultErrorRecord();
        }

        internal FormatTableLoadException(Collection<string> loadErrors) : base(StringUtil.Format(FormatAndOutXmlLoadingStrings.FormatTableLoadErrors, new object[0]))
        {
            this.errors = loadErrors;
            this.SetDefaultErrorRecord();
        }

        public FormatTableLoadException(string message) : base(message)
        {
            this.SetDefaultErrorRecord();
        }

        protected FormatTableLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            int num = info.GetInt32("ErrorCount");
            if (num > 0)
            {
                this.errors = new Collection<string>();
                for (int i = 0; i < num; i++)
                {
                    string name = string.Format(CultureInfo.InvariantCulture, "Error{0}", new object[] { i });
                    this.errors.Add(info.GetString(name));
                }
            }
        }

        public FormatTableLoadException(string message, Exception innerException) : base(message, innerException)
        {
            this.SetDefaultErrorRecord();
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            if (this.errors != null)
            {
                int count = this.errors.Count;
                info.AddValue("ErrorCount", count);
                for (int i = 0; i < count; i++)
                {
                    string name = string.Format(CultureInfo.InvariantCulture, "Error{0}", new object[] { i });
                    info.AddValue(name, this.errors[i]);
                }
            }
        }

        protected void SetDefaultErrorRecord()
        {
            base.SetErrorCategory(ErrorCategory.InvalidData);
            base.SetErrorId(typeof(FormatTableLoadException).FullName);
        }

        public Collection<string> Errors
        {
            get
            {
                return this.errors;
            }
        }
    }
}

