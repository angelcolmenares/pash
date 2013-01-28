namespace System.Management.Automation.Internal.Host
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Security;
    using System.Text;

    internal class InternalHostUserInterface : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
    {
        private PSHostUserInterface externalUI;
        private PSInformationalBuffers informationalBuffers;
        private InternalHostRawUserInterface internalRawUI;
        private InternalHost parent;
        private const string PromptEmptyDescriptionsErrorResource = "PromptEmptyDescriptionsError";
        private const string resStringsBaseName = "InternalHostUserInterfaceStrings";
        private const string UnsupportedPreferenceErrorResource = "UnsupportedPreferenceError";

        internal InternalHostUserInterface(PSHostUserInterface externalUI, InternalHost parentHost)
        {
            this.externalUI = externalUI;
            if (parentHost == null)
            {
                throw PSTraceSource.NewArgumentNullException("parentHost");
            }
            this.parent = parentHost;
            PSHostRawUserInterface externalRawUI = null;
            if (externalUI != null)
            {
                externalRawUI = externalUI.RawUI;
            }
            this.internalRawUI = new InternalHostRawUserInterface(externalRawUI, this.parent);
        }

        private bool DebugShouldContinue(string message, ref ActionPreference actionPreference)
        {
            bool flag = false;
            Collection<ChoiceDescription> choices = new Collection<ChoiceDescription> {
                new ChoiceDescription(InternalHostUserInterfaceStrings.ShouldContinueYesLabel, InternalHostUserInterfaceStrings.ShouldContinueYesHelp),
                new ChoiceDescription(InternalHostUserInterfaceStrings.ShouldContinueYesToAllLabel, InternalHostUserInterfaceStrings.ShouldContinueYesToAllHelp),
                new ChoiceDescription(InternalHostUserInterfaceStrings.ShouldContinueNoLabel, InternalHostUserInterfaceStrings.ShouldContinueNoHelp),
                new ChoiceDescription(InternalHostUserInterfaceStrings.ShouldContinueNoToAllLabel, InternalHostUserInterfaceStrings.ShouldContinueNoToAllHelp),
                new ChoiceDescription(InternalHostUserInterfaceStrings.ShouldContinueSuspendLabel, InternalHostUserInterfaceStrings.ShouldContinueSuspendHelp)
            };
            bool flag2 = true;
            do
            {
                flag2 = true;
                switch (this.PromptForChoice(InternalHostUserInterfaceStrings.ShouldContinuePromptMessage, message, choices, 0))
                {
                    case 0:
                        flag = true;
                        break;

                    case 1:
                        actionPreference = ActionPreference.Continue;
                        flag = true;
                        break;

                    case 2:
                        flag = false;
                        break;

                    case 3:
                        actionPreference = ActionPreference.Stop;
                        flag = false;
                        break;

                    case 4:
                        this.parent.EnterNestedPrompt();
                        flag2 = false;
                        break;
                }
            }
            while (!flag2);
            return flag;
        }

        private Collection<int> EmulatePromptForMultipleChoice(string caption, string message, Collection<ChoiceDescription> choices, IEnumerable<int> defaultChoices)
        {
            if (choices == null)
            {
                throw PSTraceSource.NewArgumentNullException("choices");
            }
            if (choices.Count == 0)
            {
                throw PSTraceSource.NewArgumentException("choices", "InternalHostUserInterfaceStrings", "EmptyChoicesError", new object[] { "choices" });
            }
            Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
            if (defaultChoices != null)
            {
                foreach (int num in defaultChoices)
                {
                    if ((num < 0) || (num >= choices.Count))
                    {
                        throw PSTraceSource.NewArgumentOutOfRangeException("defaultChoice", num, "InternalHostUserInterfaceStrings", "InvalidDefaultChoiceForMultipleSelection", new object[] { "defaultChoice", "choices", num });
                    }
                    if (!dictionary.ContainsKey(num))
                    {
                        dictionary.Add(num, true);
                    }
                }
            }
            StringBuilder builder = new StringBuilder();
            char ch = '\n';
            if (!string.IsNullOrEmpty(caption))
            {
                builder.Append(caption);
                builder.Append(ch);
            }
            if (!string.IsNullOrEmpty(message))
            {
                builder.Append(message);
                builder.Append(ch);
            }
            string[,] hotkeysAndPlainLabels = null;
            HostUIHelperMethods.BuildHotkeysAndPlainLabels(choices, out hotkeysAndPlainLabels);
            string format = "[{0}] {1}  ";
            for (int i = 0; i < hotkeysAndPlainLabels.GetLength(1); i++)
            {
                string str2 = string.Format(CultureInfo.InvariantCulture, format, new object[] { hotkeysAndPlainLabels[0, i], hotkeysAndPlainLabels[1, i] });
                builder.Append(str2);
                builder.Append(ch);
            }
            string str3 = "";
            if (dictionary.Count > 0)
            {
                string str4 = "";
                StringBuilder builder2 = new StringBuilder();
                foreach (int num3 in dictionary.Keys)
                {
                    string str5 = hotkeysAndPlainLabels[0, num3];
                    if (string.IsNullOrEmpty(str5))
                    {
                        str5 = hotkeysAndPlainLabels[1, num3];
                    }
                    builder2.Append(string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { str4, str5 }));
                    str4 = ",";
                }
                string str6 = builder2.ToString();
                if (dictionary.Count == 1)
                {
                    str3 = StringUtil.Format(InternalHostUserInterfaceStrings.DefaultChoice, str6);
                }
                else
                {
                    str3 = StringUtil.Format(InternalHostUserInterfaceStrings.DefaultChoicesForMultipleChoices, str6);
                }
            }
            string str7 = builder.ToString() + str3 + ch;
            Collection<int> collection = new Collection<int>();
            int o = 0;
            while (true)
            {
                string str8 = StringUtil.Format(InternalHostUserInterfaceStrings.ChoiceMessage, o);
                str7 = str7 + str8;
                this.externalUI.WriteLine(str7);
                string str9 = this.externalUI.ReadLine();
                if (str9.Length == 0)
                {
                    if ((collection.Count == 0) && (dictionary.Keys.Count >= 0))
                    {
                        foreach (int num5 in dictionary.Keys)
                        {
                            collection.Add(num5);
                        }
                    }
                    return collection;
                }
                int item = HostUIHelperMethods.DetermineChoicePicked(str9.Trim(), choices, hotkeysAndPlainLabels);
                if (item >= 0)
                {
                    collection.Add(item);
                    o++;
                }
                str7 = "";
            }
        }

        internal static Type GetFieldType(FieldDescription field)
        {
            Exception exception;
            Type type = null;
            if ((type == null) && !string.IsNullOrEmpty(field.ParameterAssemblyFullName))
            {
                type = LanguagePrimitives.ConvertStringToType(field.ParameterAssemblyFullName, out exception);
            }
            if ((type == null) && !string.IsNullOrEmpty(field.ParameterTypeFullName))
            {
                type = LanguagePrimitives.ConvertStringToType(field.ParameterTypeFullName, out exception);
            }
            return type;
        }

        internal PSInformationalBuffers GetInformationalMessageBuffers()
        {
            return this.informationalBuffers;
        }

        internal static bool IsSecuritySensitiveType(string typeName)
        {
            return (typeName.Equals(typeof(PSCredential).Name, StringComparison.OrdinalIgnoreCase) || typeName.Equals(typeof(SecureString).Name, StringComparison.OrdinalIgnoreCase));
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            if (descriptions == null)
            {
                throw PSTraceSource.NewArgumentNullException("descriptions");
            }
            if (descriptions.Count < 1)
            {
                throw PSTraceSource.NewArgumentException("descriptions", "InternalHostUserInterfaceStrings", "PromptEmptyDescriptionsError", new object[] { "descriptions" });
            }
            if (this.externalUI == null)
            {
                this.ThrowPromptNotInteractive(message);
            }
            Dictionary<string, PSObject> dictionary = null;
            try
            {
                dictionary = this.externalUI.Prompt(caption, message, descriptions);
            }
            catch (PipelineStoppedException)
            {
                LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((RunspaceBase) this.parent.Context.CurrentRunspace).GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline == null)
                {
                    throw;
                }
                currentlyRunningPipeline.Stopper.Stop();
            }
            return dictionary;
        }

        public Collection<int> PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, IEnumerable<int> defaultChoices)
        {
            if (this.externalUI == null)
            {
                this.ThrowPromptNotInteractive(message);
            }
            IHostUISupportsMultipleChoiceSelection externalUI = this.externalUI as IHostUISupportsMultipleChoiceSelection;
            Collection<int> collection = null;
            try
            {
                if (externalUI == null)
                {
                    return this.EmulatePromptForMultipleChoice(caption, message, choices, defaultChoices);
                }
                collection = externalUI.PromptForChoice(caption, message, choices, defaultChoices);
            }
            catch (PipelineStoppedException)
            {
                LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((RunspaceBase) this.parent.Context.CurrentRunspace).GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline == null)
                {
                    throw;
                }
                currentlyRunningPipeline.Stopper.Stop();
            }
            return collection;
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            if (this.externalUI == null)
            {
                this.ThrowPromptNotInteractive(message);
            }
            int num = -1;
            try
            {
                num = this.externalUI.PromptForChoice(caption, message, choices, defaultChoice);
            }
            catch (PipelineStoppedException)
            {
                LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((RunspaceBase) this.parent.Context.CurrentRunspace).GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline == null)
                {
                    throw;
                }
                currentlyRunningPipeline.Stopper.Stop();
            }
            return num;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            return this.PromptForCredential(caption, message, userName, targetName, PSCredentialTypes.Default, PSCredentialUIOptions.Default);
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            if (this.externalUI == null)
            {
                this.ThrowPromptNotInteractive(message);
            }
            PSCredential credential = null;
            try
            {
                credential = this.externalUI.PromptForCredential(caption, message, userName, targetName, allowedCredentialTypes, options);
            }
            catch (PipelineStoppedException)
            {
                LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((RunspaceBase) this.parent.Context.CurrentRunspace).GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline == null)
                {
                    throw;
                }
                currentlyRunningPipeline.Stopper.Stop();
            }
            return credential;
        }

        public override string ReadLine()
        {
            if (this.externalUI == null)
            {
                this.ThrowNotInteractive();
            }
            string str = null;
            try
            {
                str = this.externalUI.ReadLine();
            }
            catch (PipelineStoppedException)
            {
                LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((RunspaceBase) this.parent.Context.CurrentRunspace).GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline == null)
                {
                    throw;
                }
                currentlyRunningPipeline.Stopper.Stop();
            }
            return str;
        }

        public override SecureString ReadLineAsSecureString()
        {
            if (this.externalUI == null)
            {
                this.ThrowNotInteractive();
            }
            SecureString str = null;
            try
            {
                str = this.externalUI.ReadLineAsSecureString();
            }
            catch (PipelineStoppedException)
            {
                LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((RunspaceBase) this.parent.Context.CurrentRunspace).GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline == null)
                {
                    throw;
                }
                currentlyRunningPipeline.Stopper.Stop();
            }
            return str;
        }

        internal void SetInformationalMessageBuffers(PSInformationalBuffers informationalBuffers)
        {
            this.informationalBuffers = informationalBuffers;
        }

        private void ThrowNotInteractive()
        {
            this.internalRawUI.ThrowNotInteractive();
        }

        private void ThrowPromptNotInteractive(string promptMessage)
        {
            HostException exception = new HostException(StringUtil.Format(HostInterfaceExceptionsStrings.HostFunctionPromptNotImplemented, promptMessage), null, "HostFunctionNotImplemented", ErrorCategory.NotImplemented);
            throw exception;
        }

        public override void Write(string value)
        {
            if ((value != null) && (this.externalUI != null))
            {
                this.externalUI.Write(value);
            }
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if ((value != null) && (this.externalUI != null))
            {
                this.externalUI.Write(foregroundColor, backgroundColor, value);
            }
        }

        internal void WriteDebugInfoBuffers(DebugRecord record)
        {
            if (this.informationalBuffers != null)
            {
                this.informationalBuffers.AddDebug(record);
            }
        }

        public override void WriteDebugLine(string message)
        {
            this.WriteDebugLineHelper(message);
        }

        internal void WriteDebugLine(string message, ref ActionPreference preference)
        {
            ErrorRecord error = null;
            switch (preference)
            {
                case ActionPreference.SilentlyContinue:
                case ActionPreference.Ignore:
                    return;

                case ActionPreference.Stop:
                {
                    this.WriteDebugLineHelper(message);
                    error = new ErrorRecord(new ParentContainsErrorRecordException(InternalHostUserInterfaceStrings.WriteDebugLineStoppedError), "ActionPreferenceStop", ErrorCategory.OperationStopped, null);
                    ActionPreferenceStopException exception2 = new ActionPreferenceStopException(error);
                    throw exception2;
                }
                case ActionPreference.Continue:
                    this.WriteDebugLineHelper(message);
                    return;

                case ActionPreference.Inquire:
                    if (!this.DebugShouldContinue(message, ref preference))
                    {
                        error = new ErrorRecord(new ParentContainsErrorRecordException(InternalHostUserInterfaceStrings.WriteDebugLineStoppedError), "UserStopRequest", ErrorCategory.OperationStopped, null);
                        ActionPreferenceStopException exception = new ActionPreferenceStopException(error);
                        throw exception;
                    }
                    this.WriteDebugLineHelper(message);
                    return;
            }
            throw PSTraceSource.NewArgumentException("preference", "InternalHostUserInterfaceStrings", "UnsupportedPreferenceError", new object[] { (ActionPreference) preference });
        }

        private void WriteDebugLineHelper(string message)
        {
            if (message != null)
            {
                this.WriteDebugRecord(new DebugRecord(message));
            }
        }

        internal void WriteDebugRecord(DebugRecord record)
        {
            this.WriteDebugInfoBuffers(record);
            if (this.externalUI != null)
            {
                this.externalUI.WriteDebugLine(record.Message);
            }
        }

        public override void WriteErrorLine(string value)
        {
            if ((value != null) && (this.externalUI != null))
            {
                this.externalUI.WriteErrorLine(value);
            }
        }

        public override void WriteLine()
        {
            if (this.externalUI != null)
            {
                this.externalUI.WriteLine();
            }
        }

        public override void WriteLine(string value)
        {
            if ((value != null) && (this.externalUI != null))
            {
                this.externalUI.WriteLine(value);
            }
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            if ((value != null) && (this.externalUI != null))
            {
                this.externalUI.WriteLine(foregroundColor, backgroundColor, value);
            }
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            if (record == null)
            {
                throw PSTraceSource.NewArgumentNullException("record");
            }
            if (this.informationalBuffers != null)
            {
                this.informationalBuffers.AddProgress(record);
            }
            if (this.externalUI != null)
            {
                this.externalUI.WriteProgress(sourceId, record);
            }
        }

        internal void WriteVerboseInfoBuffers(VerboseRecord record)
        {
            if (this.informationalBuffers != null)
            {
                this.informationalBuffers.AddVerbose(record);
            }
        }

        public override void WriteVerboseLine(string message)
        {
            if (message != null)
            {
                this.WriteVerboseRecord(new VerboseRecord(message));
            }
        }

        internal void WriteVerboseRecord(VerboseRecord record)
        {
            this.WriteVerboseInfoBuffers(record);
            if (this.externalUI != null)
            {
                this.externalUI.WriteVerboseLine(record.Message);
            }
        }

        internal void WriteWarningInfoBuffers(WarningRecord record)
        {
            if (this.informationalBuffers != null)
            {
                this.informationalBuffers.AddWarning(record);
            }
        }

        public override void WriteWarningLine(string message)
        {
            if (message != null)
            {
                this.WriteWarningRecord(new WarningRecord(message));
            }
        }

        internal void WriteWarningRecord(WarningRecord record)
        {
            this.WriteWarningInfoBuffers(record);
            if (this.externalUI != null)
            {
                this.externalUI.WriteWarningLine(record.Message);
            }
        }

        public override PSHostRawUserInterface RawUI
        {
            get
            {
                return this.internalRawUI;
            }
        }
    }
}

