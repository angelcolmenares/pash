namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Text;

    public class PSScriptProperty : PSPropertyInfo
    {
        private ScriptBlock getterScript;
        private string getterScriptText;
        private PSLanguageMode? languageMode;
        private ScriptBlock setterScript;
        private string setterScriptText;
        private bool shouldCloneOnAccess;

        public PSScriptProperty(string name, ScriptBlock getterScript)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            if (getterScript == null)
            {
                throw PSTraceSource.NewArgumentNullException("getterScript");
            }
            this.getterScript = getterScript;
        }

        public PSScriptProperty(string name, ScriptBlock getterScript, ScriptBlock setterScript)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            if ((getterScript == null) && (setterScript == null))
            {
                throw PSTraceSource.NewArgumentException("getterScript setterScript");
            }
            if (getterScript != null)
            {
                getterScript.DebuggerStepThrough = true;
            }
            if (setterScript != null)
            {
                setterScript.DebuggerStepThrough = true;
            }
            this.getterScript = getterScript;
            this.setterScript = setterScript;
        }

        internal PSScriptProperty(string name, ScriptBlock getterScript, ScriptBlock setterScript, bool shouldCloneOnAccess) : this(name, getterScript, setterScript)
        {
            this.shouldCloneOnAccess = shouldCloneOnAccess;
        }

        internal PSScriptProperty(string name, string getterScript, string setterScript, PSLanguageMode? languageMode)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            if ((getterScript == null) && (setterScript == null))
            {
                throw PSTraceSource.NewArgumentException("getterScript setterScript");
            }
            this.getterScriptText = getterScript;
            this.setterScriptText = setterScript;
            this.languageMode = languageMode;
        }

        internal PSScriptProperty(string name, string getterScript, string setterScript, PSLanguageMode? languageMode, bool shouldCloneOnAccess) : this(name, getterScript, setterScript, languageMode)
        {
            this.shouldCloneOnAccess = shouldCloneOnAccess;
        }

        public override PSMemberInfo Copy()
        {
            PSScriptProperty destiny = new PSScriptProperty(base.name, this.GetterScript, this.SetterScript) {
                shouldCloneOnAccess = this.shouldCloneOnAccess
            };
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        internal object InvokeGetter(object scriptThis)
        {
            object obj2;
            try
            {
                obj2 = this.GetterScript.DoInvokeReturnAsIs(true, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, AutomationNull.Value, scriptThis, new object[0]);
            }
            catch (SessionStateOverflowException exception)
            {
                throw base.NewGetValueException(exception, "ScriptGetValueSessionStateOverflowException");
            }
            catch (RuntimeException exception2)
            {
                throw base.NewGetValueException(exception2, "ScriptGetValueRuntimeException");
            }
            catch (TerminateException)
            {
                throw;
            }
            catch (FlowControlException exception3)
            {
                throw base.NewGetValueException(exception3, "ScriptGetValueFlowControlException");
            }
            catch (PSInvalidOperationException exception4)
            {
                throw base.NewGetValueException(exception4, "ScriptgetValueInvalidOperationException");
            }
            return obj2;
        }

        internal object InvokeSetter(object scriptThis, object value)
        {
            object obj2;
            try
            {
                this.SetterScript.DoInvokeReturnAsIs(true, ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe, AutomationNull.Value, AutomationNull.Value, scriptThis, new object[] { value });
                obj2 = value;
            }
            catch (SessionStateOverflowException exception)
            {
                throw base.NewSetValueException(exception, "ScriptSetValueSessionStateOverflowException");
            }
            catch (RuntimeException exception2)
            {
                throw base.NewSetValueException(exception2, "ScriptSetValueRuntimeException");
            }
            catch (TerminateException)
            {
                throw;
            }
            catch (FlowControlException exception3)
            {
                throw base.NewSetValueException(exception3, "ScriptSetValueFlowControlException");
            }
            catch (PSInvalidOperationException exception4)
            {
                throw base.NewSetValueException(exception4, "ScriptSetValueInvalidOperationException");
            }
            return obj2;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.TypeNameOfValue);
            builder.Append(" ");
            builder.Append(base.Name);
            builder.Append(" {");
            if (this.IsGettable)
            {
                builder.Append("get=");
                builder.Append(this.GetterScript.ToString());
                builder.Append(";");
            }
            if (this.IsSettable)
            {
                builder.Append("set=");
                builder.Append(this.SetterScript.ToString());
                builder.Append(";");
            }
            builder.Append("}");
            return builder.ToString();
        }

        public ScriptBlock GetterScript
        {
            get
            {
                if ((this.getterScript == null) && (this.getterScriptText != null))
                {
                    this.getterScript = ScriptBlock.Create(this.getterScriptText);
                    if (this.languageMode.HasValue)
                    {
                        this.getterScript.LanguageMode = this.languageMode;
                    }
                    this.getterScript.DebuggerStepThrough = true;
                }
                if (this.getterScript == null)
                {
                    return null;
                }
                if (this.shouldCloneOnAccess)
                {
                    ScriptBlock block = this.getterScript.Clone(false);
                    block.LanguageMode = this.getterScript.LanguageMode;
                    return block;
                }
                return this.getterScript;
            }
        }

        public override bool IsGettable
        {
            get
            {
                return (this.GetterScript != null);
            }
        }

        public override bool IsSettable
        {
            get
            {
                return (this.SetterScript != null);
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.ScriptProperty;
            }
        }

        public ScriptBlock SetterScript
        {
            get
            {
                if ((this.setterScript == null) && (this.setterScriptText != null))
                {
                    this.setterScript = ScriptBlock.Create(this.setterScriptText);
                    if (this.languageMode.HasValue)
                    {
                        this.setterScript.LanguageMode = this.languageMode;
                    }
                    this.setterScript.DebuggerStepThrough = true;
                }
                if (this.setterScript == null)
                {
                    return null;
                }
                if (this.shouldCloneOnAccess)
                {
                    ScriptBlock block = this.setterScript.Clone(false);
                    block.LanguageMode = this.setterScript.LanguageMode;
                    return block;
                }
                return this.setterScript;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                if ((this.GetterScript != null) && (this.GetterScript.OutputType.Count > 0))
                {
                    return this.GetterScript.OutputType[0].Name;
                }
                return typeof(object).FullName;
            }
        }

        public override object Value
        {
            get
            {
                if (this.GetterScript == null)
                {
                    throw new GetValueException("GetWithoutGetterFromScriptPropertyValue", null, ExtendedTypeSystem.GetWithoutGetterException, new object[] { base.Name });
                }
                return this.InvokeGetter(base.instance);
            }
            set
            {
                if (this.SetterScript == null)
                {
                    throw new SetValueException("SetWithoutSetterFromScriptProperty", null, ExtendedTypeSystem.SetWithoutSetterException, new object[] { base.Name });
                }
                this.InvokeSetter(base.instance, value);
            }
        }
    }
}

