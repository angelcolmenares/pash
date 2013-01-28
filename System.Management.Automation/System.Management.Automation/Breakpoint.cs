namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class Breakpoint
    {
        private static int _lastID;

        internal Breakpoint(string script, ScriptBlock action)
        {
            this.Enabled = true;
            this.Script = script;
            this.Id = _lastID++;
            this.Action = action;
            this.HitCount = 0;
        }

        internal virtual void RemoveSelf(Debugger debugger)
        {
        }

        internal void SetEnabled(bool value)
        {
            this.Enabled = value;
        }

        internal BreakpointAction Trigger()
        {
            this.HitCount++;
            if (this.Action == null)
            {
                return BreakpointAction.Break;
            }
            try
            {
                this.Action.DoInvoke(this, null, new object[0]);
            }
            catch (BreakException)
            {
                return BreakpointAction.Break;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            return BreakpointAction.Continue;
        }

        public ScriptBlock Action { get; private set; }

        public bool Enabled { get; private set; }

        public int HitCount { get; private set; }

        public int Id { get; private set; }

        internal bool IsScriptBreakpoint
        {
            get
            {
                return (this.Script != null);
            }
        }

        public string Script { get; private set; }

        internal enum BreakpointAction
        {
            Continue,
            Break
        }
    }
}

