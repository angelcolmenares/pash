namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;

    [Cmdlet("Invoke", "Expression", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113343")]
    public sealed class InvokeExpressionCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            ScriptBlock block = base.InvokeCommand.NewScriptBlock(this.Command);
            if (base.Context.HasRunspaceEverUsedConstrainedLanguageMode)
            {
                block.LanguageMode = (PSLanguageMode)3;
            }
            block.InvokeUsingCmdlet(this, false, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, AutomationNull.Value, new object[0], AutomationNull.Value, new object[0]);
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true)]
        public string Command { get; set; }
    }
}

