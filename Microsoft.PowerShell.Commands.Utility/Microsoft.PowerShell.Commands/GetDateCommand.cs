namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Text;
    using System.Threading;

    [Cmdlet("Get", "Date", DefaultParameterSetName="net", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113313"), OutputType(new Type[] { typeof(string) }, ParameterSetName=new string[] { "UFormat", "net" }), OutputType(new Type[] { typeof(DateTime) }, ParameterSetName=new string[] { "net" })]
    public sealed class GetDateCommand : Cmdlet
    {
        private DateTime date;
        private bool dateSpecified;
        private int day;
        private bool daySpecified;
        private DisplayHintType displayHint = DisplayHintType.DateTime;
        private string format;
        private int hour;
        private bool hourSpecified;
        private int millisecond;
        private bool millisecondSpecified;
        private int minute;
        private bool minuteSpecified;
        private int month;
        private bool monthSpecified;
        private int second;
        private bool secondSpecified;
        private string uFormat;
        private int year;
        private bool yearSpecified;

        protected override void ProcessRecord()
        {
            int num;
            DateTime now = DateTime.Now;
            if (this.dateSpecified)
            {
                now = this.Date;
            }
            if (this.yearSpecified)
            {
                num = this.Year - now.Year;
                now = now.AddYears(num);
            }
            if (this.monthSpecified)
            {
                num = this.Month - now.Month;
                now = now.AddMonths(num);
            }
            if (this.daySpecified)
            {
                num = this.Day - now.Day;
                now = now.AddDays((double) num);
            }
            if (this.hourSpecified)
            {
                num = this.Hour - now.Hour;
                now = now.AddHours((double) num);
            }
            if (this.minuteSpecified)
            {
                num = this.Minute - now.Minute;
                now = now.AddMinutes((double) num);
            }
            if (this.secondSpecified)
            {
                num = this.Second - now.Second;
                now = now.AddSeconds((double) num);
            }
            if (this.millisecondSpecified)
            {
                num = this.Millisecond - now.Millisecond;
                now = now.AddMilliseconds((double) num);
                now = now.Subtract(TimeSpan.FromTicks(now.Ticks % 0x2710L));
            }
            if (this.UFormat != null)
            {
                base.WriteObject(this.UFormatDateString(now));
            }
            else if (this.Format != null)
            {
                base.WriteObject(now.ToString(this.Format, Thread.CurrentThread.CurrentCulture));
            }
            else
            {
                PSObject sendToPipeline = new PSObject(now);
                PSNoteProperty member = new PSNoteProperty("DisplayHint", this.DisplayHint);
                sendToPipeline.Properties.Add(member);
                base.WriteObject(sendToPipeline);
            }
        }

        private string UFormatDateString(DateTime dateTime)
        {
            DateTime time = DateTime.Parse("January 1, 1970", CultureInfo.InvariantCulture);
            int num = 0;
            StringBuilder builder = new StringBuilder();
            if (this.UFormat[0] == '+')
            {
                num++;
            }
            for (int i = num; i < this.UFormat.Length; i++)
            {
                if (this.UFormat[i] == '%')
                {
                    i++;
                    switch (this.UFormat[i])
                    {
                        case 'A':
                        {
                            builder.Append("{0:dddd}");
                            continue;
                        }
                        case 'B':
                        {
                            builder.Append("{0:MMMM}");
                            continue;
                        }
                        case 'C':
                        {
                            builder.Append((int) (dateTime.Year / 100));
                            continue;
                        }
                        case 'D':
                        {
                            builder.Append("{0:MM/dd/yy}");
                            continue;
                        }
                        case 'G':
                        {
                            builder.Append("{0:yyyy}");
                            continue;
                        }
                        case 'H':
                        {
                            builder.Append("{0:HH}");
                            continue;
                        }
                        case 'I':
                        {
                            builder.Append("{0:hh}");
                            continue;
                        }
                        case 'M':
                        {
                            builder.Append("{0:mm}");
                            continue;
                        }
                        case 'R':
                        {
                            builder.Append("{0:HH:mm}");
                            continue;
                        }
                        case 'S':
                        {
                            builder.Append("{0:ss}");
                            continue;
                        }
                        case 'T':
                        {
                            builder.Append("{0:HH:mm:ss}");
                            continue;
                        }
                        case 'U':
                        {
                            builder.Append((int) (dateTime.DayOfYear / 7));
                            continue;
                        }
                        case 'V':
                        {
                            builder.Append((int) ((dateTime.DayOfYear / 7) + 1));
                            continue;
                        }
                        case 'W':
                        {
                            builder.Append((int) (dateTime.DayOfYear / 7));
                            continue;
                        }
                        case 'X':
                        {
                            builder.Append("{0:HH:mm:ss}");
                            continue;
                        }
                        case 'Y':
                        {
                            builder.Append("{0:yyyy}");
                            continue;
                        }
                        case 'Z':
                        {
                            builder.Append("{0:zz}");
                            continue;
                        }
                        case 'a':
                        {
                            builder.Append("{0:ddd}");
                            continue;
                        }
                        case 'b':
                        {
                            builder.Append("{0:MMM}");
                            continue;
                        }
                        case 'c':
                        {
                            builder.Append("{0:ddd} {0:MMM} ");
                            builder.Append(StringUtil.Format("{0,2} ", dateTime.Day));
                            builder.Append("{0:HH}:{0:mm}:{0:ss} {0:yyyy}");
                            continue;
                        }
                        case 'd':
                        {
                            builder.Append("{0:dd}");
                            continue;
                        }
                        case 'e':
                        {
                            builder.Append(StringUtil.Format("{0,2}", dateTime.Day));
                            continue;
                        }
                        case 'g':
                        {
                            builder.Append("{0:yy}");
                            continue;
                        }
                        case 'h':
                        {
                            builder.Append("{0:MMM}");
                            continue;
                        }
                        case 'j':
                        {
                            builder.Append(dateTime.DayOfYear);
                            continue;
                        }
                        case 'k':
                        {
                            builder.Append("{0:HH}");
                            continue;
                        }
                        case 'l':
                        {
                            builder.Append("{0:hh}");
                            continue;
                        }
                        case 'm':
                        {
                            builder.Append("{0:MM}");
                            continue;
                        }
                        case 'n':
                        {
                            builder.Append("\n");
                            continue;
                        }
                        case 'p':
                        {
                            builder.Append("{0:tt}");
                            continue;
                        }
                        case 'r':
                        {
                            builder.Append("{0:hh:mm:ss tt}");
                            continue;
                        }
                        case 's':
                        {
                            builder.Append(dateTime.Subtract(time).TotalSeconds);
                            continue;
                        }
                        case 't':
                        {
                            builder.Append("\t");
                            continue;
                        }
                        case 'u':
                        {
                            builder.Append((int) dateTime.DayOfWeek);
                            continue;
                        }
                        case 'w':
                        {
                            builder.Append((int) dateTime.DayOfWeek);
                            continue;
                        }
                        case 'x':
                        {
                            builder.Append("{0:MM/dd/yy}");
                            continue;
                        }
                        case 'y':
                        {
                            builder.Append("{0:yy}");
                            continue;
                        }
                    }
                    builder.Append(this.UFormat[i]);
                    continue;
                }
                builder.Append(this.UFormat[i]);
            }
            return StringUtil.Format(builder.ToString(), dateTime);
        }

        [Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true), Alias(new string[] { "LastWriteTime" })]
        public DateTime Date
        {
            get
            {
                return this.date;
            }
            set
            {
                this.date = value;
                this.dateSpecified = true;
            }
        }

        [Parameter, ValidateRange(1, 0x1f)]
        public int Day
        {
            get
            {
                return this.day;
            }
            set
            {
                this.day = value;
                this.daySpecified = true;
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

        [Parameter(ParameterSetName="net")]
        public string Format
        {
            get
            {
                return this.format;
            }
            set
            {
                this.format = value;
            }
        }

        [ValidateRange(0, 0x17), Parameter]
        public int Hour
        {
            get
            {
                return this.hour;
            }
            set
            {
                this.hour = value;
                this.hourSpecified = true;
            }
        }

        [Parameter, ValidateRange(0, 0x3e7)]
        public int Millisecond
        {
            get
            {
                return this.millisecond;
            }
            set
            {
                this.millisecond = value;
                this.millisecondSpecified = true;
            }
        }

        [Parameter, ValidateRange(0, 0x3b)]
        public int Minute
        {
            get
            {
                return this.minute;
            }
            set
            {
                this.minute = value;
                this.minuteSpecified = true;
            }
        }

        [Parameter, ValidateRange(1, 12)]
        public int Month
        {
            get
            {
                return this.month;
            }
            set
            {
                this.month = value;
                this.monthSpecified = true;
            }
        }

        [ValidateRange(0, 0x3b), Parameter]
        public int Second
        {
            get
            {
                return this.second;
            }
            set
            {
                this.second = value;
                this.secondSpecified = true;
            }
        }

        [Parameter(ParameterSetName="UFormat")]
        public string UFormat
        {
            get
            {
                return this.uFormat;
            }
            set
            {
                this.uFormat = value;
            }
        }

        [Parameter, ValidateRange(1, 0x270f)]
        public int Year
        {
            get
            {
                return this.year;
            }
            set
            {
                this.year = value;
                this.yearSpecified = true;
            }
        }
    }
}

