namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Internal;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public class ScriptRequiresException : RuntimeException
    {
        private string _commandName;
        private ReadOnlyCollection<string> _missingPSSnapIns;
        private Version _requiresPSVersion;
        private string _requiresShellId;
        private string _requiresShellPath;

        public ScriptRequiresException()
        {
            this._commandName = string.Empty;
            this._missingPSSnapIns = new ReadOnlyCollection<string>(new string[0]);
        }

        public ScriptRequiresException(string message) : base(message)
        {
            this._commandName = string.Empty;
            this._missingPSSnapIns = new ReadOnlyCollection<string>(new string[0]);
        }

        protected ScriptRequiresException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._commandName = string.Empty;
            this._missingPSSnapIns = new ReadOnlyCollection<string>(new string[0]);
            this._commandName = info.GetString("CommandName");
            this._requiresPSVersion = (Version) info.GetValue("RequiresPSVersion", typeof(Version));
            this._missingPSSnapIns = (ReadOnlyCollection<string>) info.GetValue("MissingPSSnapIns", typeof(ReadOnlyCollection<string>));
            this._requiresShellId = info.GetString("RequiresShellId");
            this._requiresShellPath = info.GetString("RequiresShellPath");
        }

        public ScriptRequiresException(string message, Exception innerException) : base(message, innerException)
        {
            this._commandName = string.Empty;
            this._missingPSSnapIns = new ReadOnlyCollection<string>(new string[0]);
        }

        internal ScriptRequiresException(string commandName, Collection<string> missingItems, string errorId, bool forSnapins) : this(commandName, missingItems, errorId, forSnapins, null)
        {
        }

        internal ScriptRequiresException(string commandName, string requiresShellId, string requiresShellPath, string errorId) : base(BuildMessage(commandName, requiresShellId, requiresShellPath, true))
        {
            this._commandName = string.Empty;
            this._missingPSSnapIns = new ReadOnlyCollection<string>(new string[0]);
            this._commandName = commandName;
            this._requiresShellId = requiresShellId;
            this._requiresShellPath = requiresShellPath;
            base.SetErrorId(errorId);
            base.SetTargetObject(commandName);
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
        }

        internal ScriptRequiresException(string commandName, Version requiresPSVersion, string currentPSVersion, string errorId) : base(BuildMessage(commandName, requiresPSVersion.ToString(), currentPSVersion, false))
        {
            this._commandName = string.Empty;
            this._missingPSSnapIns = new ReadOnlyCollection<string>(new string[0]);
            this._commandName = commandName;
            this._requiresPSVersion = requiresPSVersion;
            base.SetErrorId(errorId);
            base.SetTargetObject(commandName);
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
        }

        internal ScriptRequiresException(string commandName, Collection<string> missingItems, string errorId, bool forSnapins, ErrorRecord errorRecord) : base(BuildMessage(commandName, missingItems, forSnapins), null, errorRecord)
        {
            this._commandName = string.Empty;
            this._missingPSSnapIns = new ReadOnlyCollection<string>(new string[0]);
            this._commandName = commandName;
            this._missingPSSnapIns = new ReadOnlyCollection<string>(missingItems);
            base.SetErrorId(errorId);
            base.SetTargetObject(commandName);
            base.SetErrorCategory(ErrorCategory.ResourceUnavailable);
        }

        private static string BuildMessage(string commandName, Collection<string> missingItems, bool forSnapins)
        {
            string str = null;
            if (forSnapins)
            {
                str = "RequiresMissingPSSnapIns";
            }
            else
            {
                str = "RequiresMissingModules";
            }
            StringBuilder builder = new StringBuilder();
            if (missingItems == null)
            {
                throw PSTraceSource.NewArgumentNullException("missingItems");
            }
            foreach (string str2 in missingItems)
            {
                builder.Append(str2).Append(", ");
            }
            if (builder.Length > 1)
            {
                builder.Remove(builder.Length - 2, 2);
            }
            try
            {
                if (forSnapins)
                {
                    return StringUtil.Format(DiscoveryExceptions.RequiresMissingPSSnapIns, commandName, builder.ToString());
                }
                return StringUtil.Format(DiscoveryExceptions.RequiresMissingModules, commandName, builder.ToString());
            }
            catch (MissingManifestResourceException exception)
            {
                return StringUtil.Format(SessionStateStrings.ResourceStringLoadError, new object[] { commandName, "DiscoveryExceptions", str, exception.Message });
            }
            catch (FormatException exception2)
            {
                return StringUtil.Format(SessionStateStrings.ResourceStringFormatError, new object[] { commandName, "DiscoveryExceptions", str, exception2.Message });
            }
        }

        private static string BuildMessage(string commandName, string first, string second, bool forShellId)
        {
            string str = null;
            string formatSpec = null;
            if (forShellId)
            {
                if (string.IsNullOrEmpty(first))
                {
                    str = "RequiresShellIDInvalidForSingleShell";
                    formatSpec = DiscoveryExceptions.RequiresShellIDInvalidForSingleShell;
                }
                else
                {
                    str = string.IsNullOrEmpty(second) ? "RequiresInterpreterNotCompatibleNoPath" : "RequiresInterpreterNotCompatible";
                    formatSpec = string.IsNullOrEmpty(second) ? DiscoveryExceptions.RequiresInterpreterNotCompatibleNoPath : DiscoveryExceptions.RequiresInterpreterNotCompatible;
                }
            }
            else
            {
                str = "RequiresPSVersionNotCompatible";
                formatSpec = DiscoveryExceptions.RequiresPSVersionNotCompatible;
            }
            try
            {
                return StringUtil.Format(formatSpec, new object[] { commandName, first, second });
            }
            catch (MissingManifestResourceException exception)
            {
                return StringUtil.Format(SessionStateStrings.ResourceStringLoadError, new object[] { commandName, "DiscoveryExceptions", str, exception.Message });
            }
            catch (FormatException exception2)
            {
                return StringUtil.Format(SessionStateStrings.ResourceStringFormatError, new object[] { commandName, "DiscoveryExceptions", str, exception2.Message });
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
            info.AddValue("CommandName", this._commandName);
            info.AddValue("RequiresPSVersion", this._requiresPSVersion, typeof(Version));
            info.AddValue("MissingPSSnapIns", this._missingPSSnapIns, typeof(ReadOnlyCollection<string>));
            info.AddValue("RequiresShellId", this._requiresShellId);
            info.AddValue("RequiresShellPath", this._requiresShellPath);
        }

        public string CommandName
        {
            get
            {
                return this._commandName;
            }
        }

        public ReadOnlyCollection<string> MissingPSSnapIns
        {
            get
            {
                return this._missingPSSnapIns;
            }
        }

        public Version RequiresPSVersion
        {
            get
            {
                return this._requiresPSVersion;
            }
        }

        public string RequiresShellId
        {
            get
            {
                return this._requiresShellId;
            }
        }

        public string RequiresShellPath
        {
            get
            {
                return this._requiresShellPath;
            }
        }
    }
}

