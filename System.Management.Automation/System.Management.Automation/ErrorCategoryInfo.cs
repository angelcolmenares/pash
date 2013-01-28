namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Threading;

    public class ErrorCategoryInfo
    {
        private ErrorRecord _errorRecord;
        private bool _reasonIsExceptionType;

        internal ErrorCategoryInfo(ErrorRecord errorRecord)
        {
            if (errorRecord == null)
            {
                throw new ArgumentNullException("errorRecord");
            }
            this._errorRecord = errorRecord;
        }

        internal static string Ellipsize(CultureInfo uiCultureInfo, string original)
        {
            if (40 >= original.Length)
            {
                return original;
            }
            string str = original.Substring(0, 15);
            string str2 = original.Substring(original.Length - 15, 15);
            return string.Format(uiCultureInfo, ErrorPackage.Ellipsize, new object[] { str, str2 });
        }

        public string GetMessage()
        {
            return this.GetMessage(Thread.CurrentThread.CurrentUICulture);
        }

        public string GetMessage(CultureInfo uiCultureInfo)
        {
            string str = this.Category.ToString();
            if (string.IsNullOrEmpty(str))
            {
                str = ErrorCategory.NotSpecified.ToString();
            }
            string notSpecified = ErrorCategoryStrings.ResourceManager.GetString(str, uiCultureInfo);
            if (string.IsNullOrEmpty(notSpecified))
            {
                notSpecified = ErrorCategoryStrings.NotSpecified;
            }
            string str3 = Ellipsize(uiCultureInfo, this.Activity);
            string str4 = Ellipsize(uiCultureInfo, this.TargetName);
            string str5 = Ellipsize(uiCultureInfo, this.TargetType);
            string reason = this.Reason;
            reason = this._reasonIsExceptionType ? reason : Ellipsize(uiCultureInfo, reason);
            try
            {
                return string.Format(uiCultureInfo, notSpecified, new object[] { str3, str4, str5, reason, str });
            }
            catch (FormatException)
            {
                notSpecified = ErrorCategoryStrings.InvalidErrorCategory;
                return string.Format(uiCultureInfo, notSpecified, new object[] { str3, str4, str5, reason, str });
            }
        }

        public override string ToString()
        {
            return this.GetMessage(Thread.CurrentThread.CurrentUICulture);
        }

        public string Activity
        {
            get
            {
                if (!string.IsNullOrEmpty(this._errorRecord._activityOverride))
                {
                    return this._errorRecord._activityOverride;
                }
                if (((this._errorRecord.InvocationInfo != null) && ((this._errorRecord.InvocationInfo.MyCommand is CmdletInfo) || (this._errorRecord.InvocationInfo.MyCommand is IScriptCommandInfo))) && !string.IsNullOrEmpty(this._errorRecord.InvocationInfo.MyCommand.Name))
                {
                    return this._errorRecord.InvocationInfo.MyCommand.Name;
                }
                return "";
            }
            set
            {
                this._errorRecord._activityOverride = value;
            }
        }

        public ErrorCategory Category
        {
            get
            {
                return this._errorRecord._category;
            }
        }

        public string Reason
        {
            get
            {
                this._reasonIsExceptionType = false;
                if (!string.IsNullOrEmpty(this._errorRecord._reasonOverride))
                {
                    return this._errorRecord._reasonOverride;
                }
                if (this._errorRecord.Exception != null)
                {
                    this._reasonIsExceptionType = true;
                    return this._errorRecord.Exception.GetType().Name;
                }
                return "";
            }
            set
            {
                this._errorRecord._reasonOverride = value;
            }
        }

        public string TargetName
        {
            get
            {
                string str;
                if (!string.IsNullOrEmpty(this._errorRecord._targetNameOverride))
                {
                    return this._errorRecord._targetNameOverride;
                }
                if (this._errorRecord.TargetObject == null)
                {
                    return "";
                }
                try
                {
                    str = this._errorRecord.TargetObject.ToString();
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    str = null;
                }
                return ErrorRecord.NotNull(str);
            }
            set
            {
                this._errorRecord._targetNameOverride = value;
            }
        }

        public string TargetType
        {
            get
            {
                if (!string.IsNullOrEmpty(this._errorRecord._targetTypeOverride))
                {
                    return this._errorRecord._targetTypeOverride;
                }
                if (this._errorRecord.TargetObject != null)
                {
                    return this._errorRecord.TargetObject.GetType().Name;
                }
                return "";
            }
            set
            {
                this._errorRecord._targetTypeOverride = value;
            }
        }
    }
}

