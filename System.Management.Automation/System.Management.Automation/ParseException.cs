namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;
    using System.Runtime.Serialization;
    using System.Text;

    [Serializable]
    public class ParseException : RuntimeException
    {
        private ParseError[] _errors;
        private const string errorIdString = "Parse";

        public ParseException()
        {
            base.SetErrorId("Parse");
            base.SetErrorCategory(ErrorCategory.ParserError);
        }

        public ParseException(ParseError[] errors)
        {
            if ((errors == null) || (errors.Length == 0))
            {
                throw new ArgumentNullException("errors");
            }
            this._errors = errors;
            base.SetErrorId(this._errors[0].ErrorId);
            base.SetErrorCategory(ErrorCategory.ParserError);
            if (errors[0].Extent != null)
            {
                this.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errors[0].Extent));
            }
        }

        public ParseException(string message) : base(message)
        {
            base.SetErrorId("Parse");
            base.SetErrorCategory(ErrorCategory.ParserError);
        }

        protected ParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._errors = (ParseError[]) info.GetValue("Errors", typeof(ParseError[]));
        }

        public ParseException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorId("Parse");
            base.SetErrorCategory(ErrorCategory.ParserError);
        }

        internal ParseException(string message, string errorId) : base(message)
        {
            base.SetErrorId(errorId);
            base.SetErrorCategory(ErrorCategory.ParserError);
        }

        internal ParseException(string message, string errorId, Exception innerException) : base(message, innerException)
        {
            base.SetErrorId(errorId);
            base.SetErrorCategory(ErrorCategory.ParserError);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("Errors", this._errors);
        }

        public ParseError[] Errors
        {
            get
            {
                return this._errors;
            }
        }

        public override string Message
        {
            get
            {
                if (this._errors == null)
                {
                    return base.Message;
                }
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < Math.Min(10, this._errors.Length); i++)
                {
                    builder.Append(this._errors[i].ToString());
                    builder.Append(Environment.NewLine);
                    builder.Append(Environment.NewLine);
                }
                if (this._errors.Length > 10)
                {
                    builder.Append(ParserStrings.TooManyErrors);
                    builder.Append(Environment.NewLine);
                    builder.Append(Environment.NewLine);
                }
                return builder.ToString();
            }
        }
    }
}

