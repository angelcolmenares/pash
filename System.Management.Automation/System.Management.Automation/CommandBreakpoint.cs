namespace System.Management.Automation
{
    using System;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;

    public class CommandBreakpoint : Breakpoint
    {
        internal CommandBreakpoint(string script, WildcardPattern command, string commandString, ScriptBlock action) : base(script, action)
        {
            this.CommandPattern = command;
            this.Command = commandString;
        }

        private bool CommandInfoMatches(CommandInfo commandInfo)
        {
            if (commandInfo != null)
            {
                if (this.CommandPattern.IsMatch(commandInfo.Name))
                {
                    return true;
                }
                if ((!string.IsNullOrEmpty(commandInfo.ModuleName) && (this.Command.IndexOf('\\') != -1)) && this.CommandPattern.IsMatch(commandInfo.ModuleName + @"\" + commandInfo.Name))
                {
                    return true;
                }
                ExternalScriptInfo info = commandInfo as ExternalScriptInfo;
                if (info != null)
                {
                    if (info.Path.Equals(this.Command, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (this.CommandPattern.IsMatch(Path.GetFileNameWithoutExtension(info.Path)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal override void RemoveSelf(Debugger debugger)
        {
            debugger.RemoveCommandBreakpoint(this);
        }

        public override string ToString()
        {
            if (!base.IsScriptBreakpoint)
            {
                return StringUtil.Format(DebuggerStrings.CommandBreakpointString, this.Command);
            }
            return StringUtil.Format(DebuggerStrings.CommandScriptBreakpointString, base.Script, this.Command);
        }

        internal bool Trigger(InvocationInfo invocationInfo)
        {
            if (!this.CommandPattern.IsMatch(invocationInfo.InvocationName) && !this.CommandInfoMatches(invocationInfo.MyCommand))
            {
                return false;
            }
            if (base.Script != null)
            {
                return base.Script.Equals(invocationInfo.ScriptName, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        public string Command { get; private set; }

        internal WildcardPattern CommandPattern { get; private set; }
    }
}

