namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;

    public class VariableBreakpoint : Breakpoint
    {
        internal VariableBreakpoint(string script, string variable, VariableAccessMode accessMode, ScriptBlock action) : base(script, action)
        {
            this.Variable = variable;
            this.AccessMode = accessMode;
        }

        internal override void RemoveSelf(Debugger debugger)
        {
            debugger.RemoveVariableBreakpoint(this);
        }

        public override string ToString()
        {
            if (!base.IsScriptBreakpoint)
            {
                return StringUtil.Format(DebuggerStrings.VariableBreakpointString, this.Variable, this.AccessMode);
            }
            return StringUtil.Format(DebuggerStrings.VariableScriptBreakpointString, new object[] { base.Script, this.Variable, this.AccessMode });
        }

        internal bool Trigger(string currentScriptFile, bool read)
        {
            if (!base.Enabled)
            {
                return false;
            }
            if ((this.AccessMode != VariableAccessMode.ReadWrite) && (this.AccessMode != (read ? VariableAccessMode.Read : VariableAccessMode.Write)))
            {
                return false;
            }
            if ((base.Script != null) && !base.Script.Equals(currentScriptFile, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return (base.Trigger() == Breakpoint.BreakpointAction.Break);
        }

        public VariableAccessMode AccessMode { get; private set; }

        public string Variable { get; private set; }
    }
}

