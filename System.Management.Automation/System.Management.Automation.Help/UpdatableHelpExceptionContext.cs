namespace System.Management.Automation.Help
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal class UpdatableHelpExceptionContext
    {
        private HashSet<string> _cultures;
        private UpdatableHelpSystemException _exception;
        private HashSet<string> _modules;

        internal UpdatableHelpExceptionContext(UpdatableHelpSystemException exception)
        {
            this._exception = exception;
            this._modules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this._cultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        internal ErrorRecord CreateErrorRecord(UpdatableHelpCommandType commandType)
        {
            return new ErrorRecord(new System.Exception(this.GetExceptionMessage(commandType)), this._exception.FullyQualifiedErrorId, this._exception.ErrorCategory, this._exception.TargetObject);
        }

        internal string GetExceptionMessage(UpdatableHelpCommandType commandType)
        {
            string str2 = string.Join(", ", this._modules);
            string str3 = string.Join(", ", this._cultures);
            if (commandType == UpdatableHelpCommandType.UpdateHelpCommand)
            {
                if (this._cultures.Count == 0)
                {
                    return StringUtil.Format(HelpDisplayStrings.FailedToUpdateHelpForModule, str2, this._exception.Message);
                }
                return StringUtil.Format(HelpDisplayStrings.FailedToUpdateHelpForModuleWithCulture, new object[] { str2, str3, this._exception.Message });
            }
            if (this._cultures.Count == 0)
            {
                return StringUtil.Format(HelpDisplayStrings.FailedToSaveHelpForModule, str2, this._exception.Message);
            }
            return StringUtil.Format(HelpDisplayStrings.FailedToSaveHelpForModuleWithCulture, new object[] { str2, str3, this._exception.Message });
        }

        internal HashSet<string> Cultures
        {
            get
            {
                return this._cultures;
            }
            set
            {
                this._cultures = value;
            }
        }

        internal UpdatableHelpSystemException Exception
        {
            get
            {
                return this._exception;
            }
        }

        internal HashSet<string> Modules
        {
            get
            {
                return this._modules;
            }
            set
            {
                this._modules = value;
            }
        }
    }
}

