namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class TypeTableLoadException : RuntimeException
    {
        private Collection<string> errors;

        public TypeTableLoadException()
        {
            this.SetDefaultErrorRecord();
        }

        internal TypeTableLoadException(Collection<string> loadErrors) : base(TypesXmlStrings.TypeTableLoadErrors)
        {
            this.errors = loadErrors;
            this.SetDefaultErrorRecord();
        }

        public TypeTableLoadException(string message) : base(message)
        {
            this.SetDefaultErrorRecord();
        }

        protected TypeTableLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
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

        public TypeTableLoadException(string message, Exception innerException) : base(message, innerException)
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
            base.SetErrorId(typeof(TypeTableLoadException).FullName);
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

