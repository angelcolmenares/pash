namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class ScriptParameterBinder : ParameterBinderBase
    {
        private readonly CallSite<Func<CallSite, object, object>> _copyMutableValueSite;

        internal ScriptParameterBinder(ScriptBlock script, InvocationInfo invocationInfo, ExecutionContext context, InternalCommand command, SessionStateScope localScope) : base(invocationInfo, context, command)
        {
            this._copyMutableValueSite = CallSite<Func<CallSite, object, object>>.Create(PSVariableAssignmentBinder.Get());
            this.Script = script;
            this.LocalScope = localScope;
        }

        internal override void BindParameter(string name, object value)
        {
            if ((value == AutomationNull.Value) || (value == UnboundParameter.Value))
            {
                value = null;
            }
            VariablePath variablePath = new VariablePath(name, VariablePathFlags.Variable);
            if (((this.LocalScope == null) || !variablePath.IsAnyLocal()) || !this.LocalScope.TrySetLocalParameterValue(variablePath.UnqualifiedPath, this.CopyMutableValues(value)))
            {
                RuntimeDefinedParameter parameter;
                PSVariable newValue = new PSVariable(variablePath.UnqualifiedPath, value, variablePath.IsPrivate ? ScopedItemOptions.Private : ScopedItemOptions.None);
                base.Context.EngineSessionState.SetVariable(variablePath, newValue, false, CommandOrigin.Internal);
                if (this.Script.RuntimeDefinedParameters.TryGetValue(name, out parameter))
                {
                    newValue.AddParameterAttributesNoChecks(parameter.Attributes);
                }
            }
        }

        internal object CopyMutableValues(object o)
        {
            return this._copyMutableValueSite.Target(this._copyMutableValueSite, o);
        }

        internal override object GetDefaultParameterValue(string name)
        {
            RuntimeDefinedParameter parameter;
            if (this.Script.RuntimeDefinedParameters.TryGetValue(name, out parameter))
            {
                return this.GetDefaultScriptParameterValue(parameter, null);
            }
            return null;
        }

        internal object GetDefaultScriptParameterValue(RuntimeDefinedParameter parameter, IList implicitUsingParameters = null)
        {
            object obj2 = parameter.Value;
            Compiler.DefaultValueExpressionWrapper wrapper = obj2 as Compiler.DefaultValueExpressionWrapper;
            if (wrapper != null)
            {
                obj2 = wrapper.GetValue(base.Context, this.Script.SessionStateInternal, implicitUsingParameters);
            }
            return obj2;
        }

        internal SessionStateScope LocalScope { get; set; }

        internal ScriptBlock Script { get; private set; }
    }
}

