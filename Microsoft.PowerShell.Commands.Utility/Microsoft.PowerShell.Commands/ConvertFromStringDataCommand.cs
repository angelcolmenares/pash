namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Management.Automation;
    using System.Text.RegularExpressions;

    [Cmdlet("ConvertFrom", "StringData", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113288", RemotingCapability=RemotingCapability.None), OutputType(new Type[] { typeof(Hashtable) })]
    public sealed class ConvertFromStringDataCommand : PSCmdlet
    {
        private string _stringData;

        protected override void ProcessRecord()
        {
            Hashtable sendToPipeline = new Hashtable(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(this._stringData))
            {
                base.WriteObject(sendToPipeline);
            }
            else
            {
                foreach (string str in this._stringData.Split(new char[] { '\n' }))
                {
                    string str2 = str.Trim();
                    if (!string.IsNullOrEmpty(str2) && (str2[0] != '#'))
                    {
                        int index = str2.IndexOf('=');
                        if (index <= 0)
                        {
                            throw PSTraceSource.NewInvalidOperationException("ConvertFromStringData", "InvalidDataLine", new object[] { str });
                        }
                        string key = str2.Substring(0, index).Trim();
                        if (sendToPipeline.ContainsKey(key))
                        {
                            throw PSTraceSource.NewInvalidOperationException("ConvertFromStringData", "DataItemAlreadyDefined", new object[] { str, key });
                        }
                        string str4 = Regex.Unescape(str2.Substring(index + 1).Trim());
                        sendToPipeline.Add(key, str4);
                    }
                }
                base.WriteObject(sendToPipeline);
            }
        }

        [Parameter(Mandatory=true, Position=0, ValueFromPipeline=true), AllowEmptyString]
        public string StringData
        {
            get
            {
                return this._stringData;
            }
            set
            {
                this._stringData = value;
            }
        }
    }
}

