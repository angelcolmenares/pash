namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    public class ConsoleColorCmdlet : PSCmdlet
    {
        private ConsoleColor bgColor;
        private readonly Type consoleColorEnumType = typeof(ConsoleColor);
        private ConsoleColor fgColor;
        private bool isBgColorSet;
        private bool isFgColorSet;

        private static ErrorRecord BuildOutOfRangeErrorRecord(object val, string errorId)
        {
            string message = StringUtil.Format(HostStrings.InvalidColorErrorTemplate, val.ToString());
            return new ErrorRecord(new ArgumentOutOfRangeException("value", val, message), errorId, ErrorCategory.InvalidArgument, null);
        }

        [Parameter]
        public ConsoleColor BackgroundColor
        {
            get
            {
                if (!this.isBgColorSet)
                {
                    this.bgColor = base.Host.UI.RawUI.BackgroundColor;
                    this.isBgColorSet = true;
                }
                return this.bgColor;
            }
            set
            {
                if ((value >= ConsoleColor.Black) && (value <= ConsoleColor.White))
                {
                    this.bgColor = value;
                    this.isBgColorSet = true;
                }
                else
                {
                    base.ThrowTerminatingError(BuildOutOfRangeErrorRecord(value, "SetInvalidBackgroundColor"));
                }
            }
        }

        [Parameter]
        public ConsoleColor ForegroundColor
        {
            get
            {
                if (!this.isFgColorSet)
                {
                    this.fgColor = base.Host.UI.RawUI.ForegroundColor;
                    this.isFgColorSet = true;
                }
                return this.fgColor;
            }
            set
            {
                if ((value >= ConsoleColor.Black) && (value <= ConsoleColor.White))
                {
                    this.fgColor = value;
                    this.isFgColorSet = true;
                }
                else
                {
                    base.ThrowTerminatingError(BuildOutOfRangeErrorRecord(value, "SetInvalidForegroundColor"));
                }
            }
        }
    }
}

