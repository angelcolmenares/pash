namespace System.Management.Automation
{
    using System;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable]
    public class ErrorDetails : ISerializable
    {
        private string _message;
        private string _recommendedAction;
        private Exception _textLookupError;

        internal ErrorDetails(ErrorDetails errorDetails)
        {
            this._message = "";
            this._recommendedAction = "";
            this._message = errorDetails._message;
            this._recommendedAction = errorDetails._recommendedAction;
        }

        public ErrorDetails(string message)
        {
            this._message = "";
            this._recommendedAction = "";
            this._message = message;
        }

        protected ErrorDetails(SerializationInfo info, StreamingContext context)
        {
            this._message = "";
            this._recommendedAction = "";
            this._message = info.GetString("ErrorDetails_Message");
            this._recommendedAction = info.GetString("ErrorDetails_RecommendedAction");
        }

        public ErrorDetails(Cmdlet cmdlet, string baseName, string resourceId, params object[] args)
        {
            this._message = "";
            this._recommendedAction = "";
            this._message = this.BuildMessage(cmdlet, baseName, resourceId, args);
        }

        public ErrorDetails(IResourceSupplier resourceSupplier, string baseName, string resourceId, params object[] args)
        {
            this._message = "";
            this._recommendedAction = "";
            this._message = this.BuildMessage(resourceSupplier, baseName, resourceId, args);
        }

        public ErrorDetails(Assembly assembly, string baseName, string resourceId, params object[] args)
        {
            this._message = "";
            this._recommendedAction = "";
            this._message = this.BuildMessage(assembly, baseName, resourceId, args);
        }

        private string BuildMessage(Cmdlet cmdlet, string baseName, string resourceId, params object[] args)
        {
            if (cmdlet == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentNullException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw PSTraceSource.NewArgumentNullException("resourceId");
            }
            string template = "";
            try
            {
                template = cmdlet.GetResourceString(baseName, resourceId);
            }
            catch (MissingManifestResourceException exception)
            {
                this._textLookupError = exception;
                return "";
            }
            catch (ArgumentException exception2)
            {
                this._textLookupError = exception2;
                return "";
            }
            return this.BuildMessage(template, baseName, resourceId, args);
        }

        private string BuildMessage(IResourceSupplier resourceSupplier, string baseName, string resourceId, params object[] args)
        {
            if (resourceSupplier == null)
            {
                throw PSTraceSource.NewArgumentNullException("resourceSupplier");
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentNullException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw PSTraceSource.NewArgumentNullException("resourceId");
            }
            string template = "";
            try
            {
                template = resourceSupplier.GetResourceString(baseName, resourceId);
            }
            catch (MissingManifestResourceException exception)
            {
                this._textLookupError = exception;
                return "";
            }
            catch (ArgumentException exception2)
            {
                this._textLookupError = exception2;
                return "";
            }
            return this.BuildMessage(template, baseName, resourceId, args);
        }

        private string BuildMessage(Assembly assembly, string baseName, string resourceId, params object[] args)
        {
            if (null == assembly)
            {
                throw PSTraceSource.NewArgumentNullException("assembly");
            }
            if (string.IsNullOrEmpty(baseName))
            {
                throw PSTraceSource.NewArgumentNullException("baseName");
            }
            if (string.IsNullOrEmpty(resourceId))
            {
                throw PSTraceSource.NewArgumentNullException("resourceId");
            }
            string template = "";
            ResourceManager resourceManager = ResourceManagerCache.GetResourceManager(assembly, baseName);
            try
            {
                template = resourceManager.GetString(resourceId, Thread.CurrentThread.CurrentUICulture);
            }
            catch (MissingManifestResourceException exception)
            {
                this._textLookupError = exception;
                return "";
            }
            return this.BuildMessage(template, baseName, resourceId, args);
        }

        private string BuildMessage(string template, string baseName, string resourceId, params object[] args)
        {
            if (string.IsNullOrEmpty(template) || (1 >= template.Trim().Length))
            {
                this._textLookupError = PSTraceSource.NewInvalidOperationException("ErrorPackage", "ErrorDetailsEmptyTemplate", new object[] { baseName, resourceId });
                return "";
            }
            try
            {
                return string.Format(Thread.CurrentThread.CurrentCulture, template, args);
            }
            catch (FormatException exception)
            {
                this._textLookupError = exception;
                return "";
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                info.AddValue("ErrorDetails_Message", this._message);
                info.AddValue("ErrorDetails_RecommendedAction", this._recommendedAction);
            }
        }

        public override string ToString()
        {
            return this.Message;
        }

        public string Message
        {
            get
            {
                return ErrorRecord.NotNull(this._message);
            }
        }

        public string RecommendedAction
        {
            get
            {
                return ErrorRecord.NotNull(this._recommendedAction);
            }
            set
            {
                this._recommendedAction = value;
            }
        }

        internal Exception TextLookupError
        {
            get
            {
                return this._textLookupError;
            }
            set
            {
                this._textLookupError = value;
            }
        }
    }
}

