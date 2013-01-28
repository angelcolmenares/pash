namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Text;

    [Cmdlet("ForEach", "Object", SupportsShouldProcess=true, DefaultParameterSetName="ScriptBlockSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113300", RemotingCapability=RemotingCapability.None)]
    public sealed class ForEachObjectCommand : PSCmdlet
    {
        private object[] _arguments;
        private PSObject _inputObject = AutomationNull.Value;
        private string _propertyOrMethodName;
        private int end;
        private ScriptBlock endScript;
        private List<ScriptBlock> scripts = new List<ScriptBlock>();
        private bool setEndScript;
        private int start;
        private string targetString;

        protected override void BeginProcessing()
        {
            if (base.ParameterSetName == "ScriptBlockSet")
            {
                Dictionary<string, object> boundParameters = base.MyInvocation.BoundParameters;
                if (boundParameters != null)
                {
                    SwitchParameter parameter = false;
                    SwitchParameter parameter2 = false;
                    if (boundParameters.ContainsKey("whatif"))
                    {
                        parameter = (SwitchParameter) boundParameters["whatif"];
                    }
                    if (boundParameters.ContainsKey("confirm"))
                    {
                        parameter2 = (SwitchParameter) boundParameters["confirm"];
                    }
                    if ((parameter != false) || (parameter2 != false))
                    {
                        ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(InternalCommandStrings.NoShouldProcessForScriptBlockSet), "NoShouldProcessForScriptBlockSet", ErrorCategory.InvalidOperation, null);
                        base.ThrowTerminatingError(errorRecord);
                    }
                }
                this.end = this.scripts.Count;
                this.start = (this.scripts.Count > 1) ? 1 : 0;
                if (!this.setEndScript && (this.scripts.Count > 2))
                {
                    this.end = this.scripts.Count - 1;
                    this.endScript = this.scripts[this.end];
                }
                if ((this.end >= 2) && (this.scripts[0] != null))
                {
                    this.scripts[0].InvokeUsingCmdlet(this, false, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[0]);
                }
            }
        }

        private bool BlockMethodInLanguageMode(object inputObject)
        {
            if (base.Context.LanguageMode.Equals(PSLanguageMode.RestrictedLanguage))
            {
                PSInvalidOperationException exception = new PSInvalidOperationException(InternalCommandStrings.NoMethodInvocationInRestrictedLanguageMode);
                base.WriteError(new ErrorRecord(exception, "NoMethodInvocationInRestrictedLanguageMode", ErrorCategory.InvalidOperation, null));
                return true;
            }
            if (base.Context.LanguageMode.Equals(PSLanguageMode.ConstrainedLanguage) && !CoreTypes.Contains(PSObject.Base(inputObject).GetType()))
            {
                PSInvalidOperationException exception2 = new PSInvalidOperationException(ParserStrings.InvokeMethodConstrainedLanguage);
                base.WriteError(new ErrorRecord(exception2, "MethodInvocationNotSupportedInConstrainedLanguage", ErrorCategory.InvalidOperation, null));
                return true;
            }
            return false;
        }

        protected override void EndProcessing()
        {
            if ((base.ParameterSetName == "ScriptBlockSet") && (this.endScript != null))
            {
                this.endScript.InvokeUsingCmdlet(this, false, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[0]);
            }
        }

        internal static ErrorRecord GenerateNameParameterError(string paraName, string resourceString, string errorId, object target, params object[] args)
        {
            string str;
            if ((args == null) || (args.Length == 0))
            {
                str = resourceString;
            }
            else
            {
                str = StringUtil.Format(resourceString, args);
            }
            string.IsNullOrEmpty(str);
            return new ErrorRecord(new PSArgumentException(str, paraName), errorId, ErrorCategory.InvalidArgument, target);
        }

        private static string GetStringRepresentation(object obj)
        {
            string str;
            try
            {
                str = LanguagePrimitives.IsNull(obj) ? "null" : obj.ToString();
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                str = null;
            }
            if (string.IsNullOrEmpty(str))
            {
                PSObject obj2 = obj as PSObject;
                str = (obj2 != null) ? obj2.BaseObject.GetType().FullName : obj.GetType().FullName;
            }
            return str;
        }

        private bool GetValueFromIDictionaryInput()
        {
            IDictionary dictionary = PSObject.Base(this._inputObject) as IDictionary;
            try
            {
                if ((dictionary != null) && dictionary.Contains(this._propertyOrMethodName))
                {
                    string action = string.Format(CultureInfo.InvariantCulture, InternalCommandStrings.ForEachObjectKeyAction, new object[] { this._propertyOrMethodName });
                    if (base.ShouldProcess(this.targetString, action))
                    {
                        object obj3 = dictionary[this._propertyOrMethodName];
                        this.WriteToPipelineWithUnrolling(obj3);
                    }
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
            }
            return false;
        }

        private void MethodCallWithArguments()
        {
            ReadOnlyPSMemberInfoCollection<PSMemberInfo> infos = this._inputObject.Members.Match(this._propertyOrMethodName, PSMemberTypes.ParameterizedProperty | PSMemberTypes.Methods);
            if (infos.Count > 1)
            {
                StringBuilder builder = new StringBuilder();
                foreach (PSMemberInfo info in infos)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, " {0}", new object[] { info.Name });
                }
                base.WriteError(GenerateNameParameterError("Name", InternalCommandStrings.AmbiguousMethodName, "AmbiguousMethodName", this._inputObject, new object[] { this._propertyOrMethodName, builder }));
            }
            else if ((infos.Count == 0) || !(infos[0] is PSMethodInfo))
            {
                base.WriteError(GenerateNameParameterError("Name", InternalCommandStrings.MethodNotFound, "MethodNotFound", this._inputObject, new object[] { this._propertyOrMethodName }));
            }
            else
            {
                PSMethodInfo info2 = infos[0] as PSMethodInfo;
                StringBuilder builder2 = new StringBuilder(GetStringRepresentation(this._arguments[0]));
                for (int i = 1; i < this._arguments.Length; i++)
                {
                    builder2.AppendFormat(CultureInfo.InvariantCulture, ", {0}", new object[] { GetStringRepresentation(this._arguments[i]) });
                }
                string action = string.Format(CultureInfo.InvariantCulture, InternalCommandStrings.ForEachObjectMethodActionWithArguments, new object[] { info2.Name, builder2 });
                try
                {
                    if (base.ShouldProcess(this.targetString, action) && !this.BlockMethodInLanguageMode(this.InputObject))
                    {
                        object obj2 = info2.Invoke(this._arguments);
                        this.WriteToPipelineWithUnrolling(obj2);
                    }
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    base.WriteError(new ErrorRecord(exception, "MethodInvocationError", ErrorCategory.InvalidOperation, this._inputObject));
                }
            }
        }

        protected override void ProcessRecord()
        {
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName == null)
            {
                return;
            }
            if (!(parameterSetName == "ScriptBlockSet"))
            {
                if (!(parameterSetName == "PropertyAndMethodSet"))
                {
                    return;
                }
            }
            else
            {
                for (int i = this.start; i < this.end; i++)
                {
                    if (this.scripts[i] != null)
                    {
                        this.scripts[i].InvokeUsingCmdlet(this, false, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, this.InputObject, new object[] { this.InputObject }, AutomationNull.Value, new object[0]);
                    }
                }
                return;
            }
            this.targetString = string.Format(CultureInfo.InvariantCulture, InternalCommandStrings.ForEachObjectTarget, new object[] { GetStringRepresentation(this.InputObject) });
            if (LanguagePrimitives.IsNull(this.InputObject))
            {
                if ((this._arguments != null) && (this._arguments.Length > 0))
                {
                    base.WriteError(GenerateNameParameterError("InputObject", ParserStrings.InvokeMethodOnNull, "InvokeMethodOnNull", this._inputObject, new object[0]));
                    return;
                }
                string action = string.Format(CultureInfo.InvariantCulture, InternalCommandStrings.ForEachObjectPropertyAction, new object[] { this._propertyOrMethodName });
                if (base.ShouldProcess(this.targetString, action))
                {
                    if (base.Context.IsStrictVersion(2))
                    {
                        base.WriteError(GenerateNameParameterError("InputObject", InternalCommandStrings.InputObjectIsNull, "InputObjectIsNull", this._inputObject, new object[0]));
                        return;
                    }
                    base.WriteObject(null);
                }
                return;
            }
            ErrorRecord errorRecord = null;
            if ((this._arguments != null) && (this._arguments.Length > 0))
            {
                this.MethodCallWithArguments();
            }
            else
            {
                if (this.GetValueFromIDictionaryInput())
                {
                    return;
                }
                PSMemberInfo info = null;
                if (WildcardPattern.ContainsWildcardCharacters(this._propertyOrMethodName))
                {
                    ReadOnlyPSMemberInfoCollection<PSMemberInfo> infos = this._inputObject.Members.Match(this._propertyOrMethodName, PSMemberTypes.All);
                    if (infos.Count > 1)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (PSMemberInfo info2 in infos)
                        {
                            builder.AppendFormat(CultureInfo.InvariantCulture, " {0}", new object[] { info2.Name });
                        }
                        base.WriteError(GenerateNameParameterError("Name", InternalCommandStrings.AmbiguousPropertyOrMethodName, "AmbiguousPropertyOrMethodName", this._inputObject, new object[] { this._propertyOrMethodName, builder }));
                        return;
                    }
                    if (infos.Count == 1)
                    {
                        info = infos[0];
                    }
                }
                else
                {
                    info = this._inputObject.Members[this._propertyOrMethodName];
                }
                if (info == null)
                {
                    errorRecord = GenerateNameParameterError("Name", InternalCommandStrings.PropertyOrMethodNotFound, "PropertyOrMethodNotFound", this._inputObject, new object[] { this._propertyOrMethodName });
                }
                else
                {
                    if (info is PSMethodInfo)
                    {
                        PSParameterizedProperty property = info as PSParameterizedProperty;
                        if (property != null)
                        {
                            string str2 = string.Format(CultureInfo.InvariantCulture, InternalCommandStrings.ForEachObjectPropertyAction, new object[] { property.Name });
                            if (base.ShouldProcess(this.targetString, str2))
                            {
                                base.WriteObject(info.Value);
                            }
                            return;
                        }
                        PSMethodInfo info3 = info as PSMethodInfo;
                        try
                        {
                            string str3 = string.Format(CultureInfo.InvariantCulture, InternalCommandStrings.ForEachObjectMethodActionWithoutArguments, new object[] { info3.Name });
                            if (base.ShouldProcess(this.targetString, str3) && !this.BlockMethodInLanguageMode(this.InputObject))
                            {
                                object obj2 = info3.Invoke(new object[0]);
                                this.WriteToPipelineWithUnrolling(obj2);
                            }
                            goto Label_0451;
                        }
                        catch (PipelineStoppedException)
                        {
                            throw;
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                            MethodException exception2 = exception as MethodException;
                            if (((exception2 != null) && (exception2.ErrorRecord != null)) && (exception2.ErrorRecord.FullyQualifiedErrorId == "MethodCountCouldNotFindBest"))
                            {
                                base.WriteObject(info3.Value);
                            }
                            else
                            {
                                base.WriteError(new ErrorRecord(exception, "MethodInvocationError", ErrorCategory.InvalidOperation, this._inputObject));
                            }
                            goto Label_0451;
                        }
                    }
                    string str4 = string.Format(CultureInfo.InvariantCulture, InternalCommandStrings.ForEachObjectPropertyAction, new object[] { info.Name });
                    if (base.ShouldProcess(this.targetString, str4))
                    {
                        try
                        {
                            this.WriteToPipelineWithUnrolling(info.Value);
                        }
                        catch (TerminateException)
                        {
                            throw;
                        }
                        catch (MethodException)
                        {
                            throw;
                        }
                        catch (PipelineStoppedException)
                        {
                            throw;
                        }
                        catch (Exception exception3)
                        {
                            CommandProcessorBase.CheckForSevereException(exception3);
                            base.WriteObject(null);
                        }
                    }
                }
            }
        Label_0451:
            if (errorRecord != null)
            {
                string str5 = string.Format(CultureInfo.InvariantCulture, InternalCommandStrings.ForEachObjectPropertyAction, new object[] { this._propertyOrMethodName });
                if (base.ShouldProcess(this.targetString, str5))
                {
                    if (base.Context.IsStrictVersion(2))
                    {
                        base.WriteError(errorRecord);
                        return;
                    }
                    base.WriteObject(null);
                }
            }
        }

        private void WriteOutIEnumerator(IEnumerator list)
        {
            if (list != null)
            {
                while (ParserOps.MoveNext(base.Context, null, list))
                {
                    object sendToPipeline = ParserOps.Current(null, list);
                    if (sendToPipeline != AutomationNull.Value)
                    {
                        base.WriteObject(sendToPipeline);
                    }
                }
            }
        }

        private void WriteToPipelineWithUnrolling(object obj)
        {
            IEnumerator list = LanguagePrimitives.GetEnumerator(obj);
            if (list != null)
            {
                this.WriteOutIEnumerator(list);
            }
            else
            {
                base.WriteObject(obj, true);
            }
        }

        [Parameter(ParameterSetName="PropertyAndMethodSet", ValueFromRemainingArguments=true), Alias(new string[] { "Args" })]
        public object[] ArgumentList
        {
            get
            {
                return this._arguments;
            }
            set
            {
                this._arguments = value;
            }
        }

        [Parameter(ParameterSetName="ScriptBlockSet")]
        public ScriptBlock Begin
        {
            get
            {
                return null;
            }
            set
            {
                this.scripts.Insert(0, value);
            }
        }

        [Parameter(ParameterSetName="ScriptBlockSet")]
        public ScriptBlock End
        {
            get
            {
                return this.endScript;
            }
            set
            {
                this.endScript = value;
                this.setEndScript = true;
            }
        }

        [Parameter(ValueFromPipeline=true, ParameterSetName="PropertyAndMethodSet"), Parameter(ValueFromPipeline=true, ParameterSetName="ScriptBlockSet")]
        public PSObject InputObject
        {
            get
            {
                return this._inputObject;
            }
            set
            {
                this._inputObject = value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="PropertyAndMethodSet"), ValidateNotNullOrEmpty]
        public string MemberName
        {
            get
            {
                return this._propertyOrMethodName;
            }
            set
            {
                this._propertyOrMethodName = value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="ScriptBlockSet"), AllowNull, AllowEmptyCollection]
        public ScriptBlock[] Process
        {
            get
            {
                return null;
            }
            set
            {
                if (value == null)
                {
                    this.scripts.Add(null);
                }
                else
                {
                    this.scripts.AddRange(value);
                }
            }
        }

        [Parameter(ParameterSetName="ScriptBlockSet", ValueFromRemainingArguments=true), AllowEmptyCollection, AllowNull]
        public ScriptBlock[] RemainingScripts
        {
            get
            {
                return null;
            }
            set
            {
                if (value == null)
                {
                    this.scripts.Add(null);
                }
                else
                {
                    this.scripts.AddRange(value);
                }
            }
        }
    }
}

