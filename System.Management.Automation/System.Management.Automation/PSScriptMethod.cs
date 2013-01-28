namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Internal;
    using System.Text;

    public class PSScriptMethod : PSMethodInfo
    {
        private ScriptBlock script;
        private bool shouldCloneOnAccess;

        public PSScriptMethod(string name, ScriptBlock script)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            if (script == null)
            {
                throw PSTraceSource.NewArgumentNullException("script");
            }
            this.script = script;
        }

        internal PSScriptMethod(string name, ScriptBlock script, bool shouldCloneOnAccess) : this(name, script)
        {
            this.shouldCloneOnAccess = shouldCloneOnAccess;
        }

        public override PSMemberInfo Copy()
        {
            PSScriptMethod destiny = new PSScriptMethod(base.name, this.script) {
                shouldCloneOnAccess = this.shouldCloneOnAccess
            };
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        public override object Invoke(params object[] arguments)
        {
            if (arguments == null)
            {
                throw PSTraceSource.NewArgumentNullException("arguments");
            }
            return InvokeScript(base.Name, this.script, base.instance, arguments);
        }

        private static object InvokeScript(string methodName, ScriptBlock script, object @this, object[] arguments)
        {
            object obj2;
            try
            {
                obj2 = script.DoInvokeReturnAsIs(true, ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe, AutomationNull.Value, AutomationNull.Value, @this, arguments);
            }
            catch (SessionStateOverflowException exception)
            {
                throw new MethodInvocationException("ScriptMethodSessionStateOverflowException", exception, ExtendedTypeSystem.MethodInvocationException, new object[] { methodName, arguments.Length, exception.Message });
            }
            catch (RuntimeException exception2)
            {
                throw new MethodInvocationException("ScriptMethodRuntimeException", exception2, ExtendedTypeSystem.MethodInvocationException, new object[] { methodName, arguments.Length, exception2.Message });
            }
            catch (TerminateException)
            {
                throw;
            }
            catch (FlowControlException exception3)
            {
                throw new MethodInvocationException("ScriptMethodFlowControlException", exception3, ExtendedTypeSystem.MethodInvocationException, new object[] { methodName, arguments.Length, exception3.Message });
            }
            catch (PSInvalidOperationException exception4)
            {
                throw new MethodInvocationException("ScriptMethodInvalidOperationException", exception4, ExtendedTypeSystem.MethodInvocationException, new object[] { methodName, arguments.Length, exception4.Message });
            }
            return obj2;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.TypeNameOfValue);
            builder.Append(" ");
            builder.Append(base.Name);
            builder.Append("();");
            return builder.ToString();
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.ScriptMethod;
            }
        }

        public override Collection<string> OverloadDefinitions
        {
            get
            {
                return new Collection<string> { this.ToString() };
            }
        }

        public ScriptBlock Script
        {
            get
            {
                if (this.shouldCloneOnAccess)
                {
                    ScriptBlock block = this.script.Clone(false);
                    block.LanguageMode = this.script.LanguageMode;
                    return block;
                }
                return this.script;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                return typeof(object).FullName;
            }
        }
    }
}

