namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Linq;
    using System.Management.Automation;

    [Cmdlet("Get", "TraceSource", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113333"), OutputType(new Type[] { typeof(PSTraceSource) })]
    public class GetTraceSourceCommand : TraceCommandBase
    {
        private string[] names = new string[] { "*" };

        protected override void ProcessRecord()
        {
            IOrderedEnumerable<PSTraceSource> sendToPipeline = from source in base.GetMatchingTraceSource(this.names, true)
                orderby source.Name
                select source;
            base.WriteObject(sendToPipeline, true);
        }

        [Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
        public string[] Name
        {
            get
            {
                return this.names;
            }
            set
            {
                if ((value == null) || (value.Length == 0))
                {
                    value = new string[] { "*" };
                }
                this.names = value;
            }
        }
    }
}

