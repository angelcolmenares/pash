namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Management.Automation;

    [Cmdlet("Write", "Host", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113426", RemotingCapability=RemotingCapability.None)]
    public sealed class WriteHostCommand : ConsoleColorCmdlet
    {
        private bool notAppendNewline;
        private object objectToEcho;
        private object separator = " ";

        private void PrintObject(object o)
        {
            if (o != null)
            {
                string str = o as string;
                IEnumerable enumerable = null;
                if (str != null)
                {
                    if (str.Length > 0)
                    {
                        base.Host.UI.Write(base.ForegroundColor, base.BackgroundColor, str);
                    }
                }
                else
                {
                    enumerable = o as IEnumerable;
                    if (enumerable != null)
                    {
                        bool flag = false;
                        foreach (object obj2 in enumerable)
                        {
                            if (flag && (this.Separator != null))
                            {
                                base.Host.UI.Write(base.ForegroundColor, base.BackgroundColor, this.Separator.ToString());
                            }
                            this.PrintObject(obj2);
                            flag = true;
                        }
                    }
                    else
                    {
                        str = o.ToString();
                        if (str.Length > 0)
                        {
                            base.Host.UI.Write(base.ForegroundColor, base.BackgroundColor, str);
                        }
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            if (this.Object != null)
            {
                this.PrintObject(this.Object);
            }
            if (this.NoNewline == 0)
            {
                base.Host.UI.WriteLine(base.ForegroundColor, base.BackgroundColor, "");
            }
        }

        [Parameter]
        public SwitchParameter NoNewline
        {
            get
            {
                return this.notAppendNewline;
            }
            set
            {
                this.notAppendNewline = (bool) value;
            }
        }

        [Parameter(Position=0, ValueFromRemainingArguments=true, ValueFromPipeline=true)]
        public object Object
        {
            get
            {
                return this.objectToEcho;
            }
            set
            {
                this.objectToEcho = value;
            }
        }

        [Parameter]
        public object Separator
        {
            get
            {
                return this.separator;
            }
            set
            {
                this.separator = value;
            }
        }
    }
}

