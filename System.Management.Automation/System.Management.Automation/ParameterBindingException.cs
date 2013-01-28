namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class ParameterBindingException : RuntimeException
    {
        private object[] args;
        private string commandName;
        private string errorId;
        private InvocationInfo invocationInfo;
        private long line;
        private string message;
        private long offset;
        private string parameterName;
        private Type parameterType;
        private string resourceBaseName;
        private string resourceId;
        private Type typeSpecified;

        public ParameterBindingException()
        {
            this.parameterName = string.Empty;
            this.line = -9223372036854775808L;
            this.offset = -9223372036854775808L;
            this.args = new object[0];
        }

        public ParameterBindingException(string message) : base(message)
        {
            this.parameterName = string.Empty;
            this.line = -9223372036854775808L;
            this.offset = -9223372036854775808L;
            this.args = new object[0];
            this.message = message;
        }

        protected ParameterBindingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.parameterName = string.Empty;
            this.line = -9223372036854775808L;
            this.offset = -9223372036854775808L;
            this.args = new object[0];
            this.message = info.GetString("ParameterBindingException_Message");
            this.parameterName = info.GetString("ParameterName");
            this.line = info.GetInt64("Line");
            this.offset = info.GetInt64("Offset");
        }

        public ParameterBindingException(string message, Exception innerException) : base(message, innerException)
        {
            this.parameterName = string.Empty;
            this.line = -9223372036854775808L;
            this.offset = -9223372036854775808L;
            this.args = new object[0];
            this.message = message;
        }

        internal ParameterBindingException(Exception innerException, ParameterBindingException pbex, string resourceBaseName, string resourceId, params object[] args) : base(string.Empty, innerException)
        {
            this.parameterName = string.Empty;
            this.line = -9223372036854775808L;
            this.offset = -9223372036854775808L;
            this.args = new object[0];
            if (pbex == null)
            {
                throw PSTraceSource.NewArgumentNullException("pbex");
            }
            if (string.IsNullOrEmpty(resourceBaseName))
            {
                throw PSTraceSource.NewArgumentException("resourceBaseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw PSTraceSource.NewArgumentException("resourceId");
            }
            this.invocationInfo = pbex.CommandInvocation;
            if (this.invocationInfo != null)
            {
                this.commandName = this.invocationInfo.MyCommand.Name;
            }
            IScriptExtent scriptPosition = null;
            if (this.invocationInfo != null)
            {
                scriptPosition = this.invocationInfo.ScriptPosition;
            }
            this.line = pbex.Line;
            this.offset = pbex.Offset;
            this.parameterName = pbex.ParameterName;
            this.parameterType = pbex.ParameterType;
            this.typeSpecified = pbex.TypeSpecified;
            this.errorId = pbex.ErrorId;
            this.resourceBaseName = resourceBaseName;
            this.resourceId = resourceId;
            if (args != null)
            {
                this.args = args;
            }
            base.SetErrorCategory(pbex.ErrorRecord._category);
            base.SetErrorId(this.errorId);
            if (this.invocationInfo != null)
            {
                base.ErrorRecord.SetInvocationInfo(new InvocationInfo(this.invocationInfo.MyCommand, scriptPosition));
            }
        }

        internal ParameterBindingException(ErrorCategory errorCategory, InvocationInfo invocationInfo, IScriptExtent errorPosition, string parameterName, Type parameterType, Type typeSpecified, string resourceBaseName, string errorIdAndResourceId, params object[] args) : base(errorCategory, invocationInfo, errorPosition, errorIdAndResourceId, null, null)
        {
            this.parameterName = string.Empty;
            this.line = -9223372036854775808L;
            this.offset = -9223372036854775808L;
            this.args = new object[0];
            if (string.IsNullOrEmpty(resourceBaseName))
            {
                throw PSTraceSource.NewArgumentException("resourceBaseName");
            }
            if (string.IsNullOrEmpty(errorIdAndResourceId))
            {
                throw PSTraceSource.NewArgumentException("errorIdAndResourceId");
            }
            this.invocationInfo = invocationInfo;
            if (this.invocationInfo != null)
            {
                this.commandName = invocationInfo.MyCommand.Name;
            }
            this.parameterName = parameterName;
            this.parameterType = parameterType;
            this.typeSpecified = typeSpecified;
            if ((errorPosition == null) && (this.invocationInfo != null))
            {
                errorPosition = invocationInfo.ScriptPosition;
            }
            if (errorPosition != null)
            {
                this.line = errorPosition.StartLineNumber;
                this.offset = errorPosition.StartColumnNumber;
            }
            this.resourceBaseName = resourceBaseName;
            this.resourceId = errorIdAndResourceId;
            this.errorId = errorIdAndResourceId;
            if (args != null)
            {
                this.args = args;
            }
        }

        internal ParameterBindingException(Exception innerException, ErrorCategory errorCategory, InvocationInfo invocationInfo, IScriptExtent errorPosition, string parameterName, Type parameterType, Type typeSpecified, string resourceBaseName, string errorIdAndResourceId, params object[] args) : base(errorCategory, invocationInfo, errorPosition, errorIdAndResourceId, null, innerException)
        {
            this.parameterName = string.Empty;
            this.line = -9223372036854775808L;
            this.offset = -9223372036854775808L;
            this.args = new object[0];
            if (invocationInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("invocationInfo");
            }
            if (string.IsNullOrEmpty(resourceBaseName))
            {
                throw PSTraceSource.NewArgumentException("resourceBaseName");
            }
            if (string.IsNullOrEmpty(errorIdAndResourceId))
            {
                throw PSTraceSource.NewArgumentException("errorIdAndResourceId");
            }
            this.invocationInfo = invocationInfo;
            this.commandName = invocationInfo.MyCommand.Name;
            this.parameterName = parameterName;
            this.parameterType = parameterType;
            this.typeSpecified = typeSpecified;
            if (errorPosition == null)
            {
                errorPosition = invocationInfo.ScriptPosition;
            }
            if (errorPosition != null)
            {
                this.line = errorPosition.StartLineNumber;
                this.offset = errorPosition.StartColumnNumber;
            }
            this.resourceBaseName = resourceBaseName;
            this.resourceId = errorIdAndResourceId;
            this.errorId = errorIdAndResourceId;
            if (args != null)
            {
                this.args = args;
            }
        }

        private string BuildMessage()
        {
            try
            {
                object[] array = new object[0];
                if (this.args != null)
                {
                    array = new object[this.args.Length + 6];
                    array[0] = this.commandName;
                    array[1] = this.parameterName;
                    array[2] = this.parameterType;
                    array[3] = this.typeSpecified;
                    array[4] = this.line;
                    array[5] = this.offset;
                    this.args.CopyTo(array, 6);
                }
                string str = string.Empty;
                if (!string.IsNullOrEmpty(this.resourceBaseName) && !string.IsNullOrEmpty(this.resourceId))
                {
                    str = ResourceManagerCache.FormatResourceString(this.resourceBaseName, this.resourceId, array);
                }
                return str;
            }
            catch (MissingManifestResourceException exception)
            {
                return ResourceManagerCache.FormatResourceString("ParameterBinderStrings", "ResourceStringLoadError", new object[] { this.args[0], this.resourceBaseName, this.resourceId, exception.Message });
            }
            catch (FormatException exception2)
            {
                return ResourceManagerCache.FormatResourceString("ParameterBinderStrings", "ResourceStringFormatError", new object[] { this.args[0], this.resourceBaseName, this.resourceId, exception2.Message });
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new PSArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("ParameterBindingException_Message", this.Message);
            info.AddValue("ParameterName", this.parameterName);
            info.AddValue("Line", this.line);
            info.AddValue("Offset", this.offset);
        }

        public InvocationInfo CommandInvocation
        {
            get
            {
                return this.invocationInfo;
            }
        }

        public string ErrorId
        {
            get
            {
                return this.errorId;
            }
        }

        public long Line
        {
            get
            {
                return this.line;
            }
        }

        public override string Message
        {
            get
            {
                if (this.message == null)
                {
                    this.message = this.BuildMessage();
                }
                return this.message;
            }
        }

        public long Offset
        {
            get
            {
                return this.offset;
            }
        }

        public string ParameterName
        {
            get
            {
                return this.parameterName;
            }
        }

        public Type ParameterType
        {
            get
            {
                return this.parameterType;
            }
        }

        public Type TypeSpecified
        {
            get
            {
                return this.typeSpecified;
            }
        }
    }
}

