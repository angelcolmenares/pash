namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.ComponentModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;

    [Cmdlet("Set", "Date", DefaultParameterSetName="Date", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113393"), OutputType(new Type[] { typeof(DateTime) })]
    public sealed class SetDateCommand : PSCmdlet
    {
        private TimeSpan adjust;
        private DisplayHintType displayHint = DisplayHintType.DateTime;
        private DateTime to;

        [ArchitectureSensitive]
        protected override void ProcessRecord()
        {
            DateTime date;
            string str;
            if ((((str = base.ParameterSetName) == null) || (str == "Date")) || (str != "Adjust"))
            {
                date = this.Date;
            }
            else
            {
                date = DateTime.Now.Add(this.Adjust);
            }
            NativeMethods.SystemTime systime = new NativeMethods.SystemTime {
                Year = (ushort) date.Year,
                Month = (ushort) date.Month,
                Day = (ushort) date.Day,
                Hour = (ushort) date.Hour,
                Minute = (ushort) date.Minute,
                Second = (ushort) date.Second,
                Milliseconds = (ushort) date.Millisecond
            };
            if (base.ShouldProcess(date.ToString()))
            {
                if (!NativeMethods.SetLocalTime(ref systime))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                if (!NativeMethods.SetLocalTime(ref systime))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            PSObject sendToPipeline = new PSObject(date);
            PSNoteProperty member = new PSNoteProperty("DisplayHint", this.DisplayHint);
            sendToPipeline.Properties.Add(member);
            base.WriteObject(sendToPipeline);
        }

        [AllowNull, Parameter(Position=0, Mandatory=true, ParameterSetName="Adjust", ValueFromPipelineByPropertyName=true)]
        public TimeSpan Adjust
        {
            get
            {
                return this.adjust;
            }
            set
            {
                this.adjust = value;
            }
        }

        [Parameter(Position=0, Mandatory=true, ParameterSetName="Date", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
        public DateTime Date
        {
            get
            {
                return this.to;
            }
            set
            {
                this.to = value;
            }
        }

        [Parameter]
        public DisplayHintType DisplayHint
        {
            get
            {
                return this.displayHint;
            }
            set
            {
                this.displayHint = value;
            }
        }

        internal static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError=true)]
            public static extern bool SetLocalTime(ref SystemTime systime);

            [StructLayout(LayoutKind.Sequential)]
            public struct SystemTime
            {
                public ushort Year;
                public ushort Month;
                public ushort DayOfWeek;
                public ushort Day;
                public ushort Hour;
                public ushort Minute;
                public ushort Second;
                public ushort Milliseconds;
            }
        }
    }
}

