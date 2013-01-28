namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("Out", "LineOutput")]
    public class OutLineOutputCommand : FrontEndCommandBase
    {
        private object lineOutput;

        public OutLineOutputCommand()
        {
            base.implementation = new OutCommandInner();
        }

        protected override void BeginProcessing()
        {
            if (this.lineOutput == null)
            {
                this.ProcessNullLineOutput();
            }
            Microsoft.PowerShell.Commands.Internal.Format.LineOutput lineOutput = this.lineOutput as Microsoft.PowerShell.Commands.Internal.Format.LineOutput;
            if (lineOutput == null)
            {
                this.ProcessWrongTypeLineOutput(this.lineOutput);
            }
            ((OutCommandInner) base.implementation).LineOutput = lineOutput;
            base.BeginProcessing();
        }

        private void ProcessNullLineOutput()
        {
            string message = StringUtil.Format(FormatAndOut_out_xxx.OutLineOutput_NullLineOutputParameter, new object[0]);
            ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewArgumentNullException("LineOutput"), "OutLineOutputNullLineOutputParameter", ErrorCategory.InvalidArgument, null) {
                ErrorDetails = new ErrorDetails(message)
            };
            base.ThrowTerminatingError(errorRecord);
        }

        private void ProcessWrongTypeLineOutput(object obj)
        {
            string message = StringUtil.Format(FormatAndOut_out_xxx.OutLineOutput_InvalidLineOutputParameterType, obj.GetType().FullName, typeof(Microsoft.PowerShell.Commands.Internal.Format.LineOutput).FullName);
            ErrorRecord errorRecord = new ErrorRecord(new InvalidCastException(), "OutLineOutputInvalidLineOutputParameterType", ErrorCategory.InvalidArgument, null) {
                ErrorDetails = new ErrorDetails(message)
            };
            base.ThrowTerminatingError(errorRecord);
        }

        [Parameter(Mandatory=true, Position=0)]
        public object LineOutput
        {
            get
            {
                return this.lineOutput;
            }
            set
            {
                this.lineOutput = value;
            }
        }
    }
}

