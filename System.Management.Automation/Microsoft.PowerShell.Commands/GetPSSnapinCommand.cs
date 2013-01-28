namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Security;

    [OutputType(new Type[] { typeof(PSSnapInInfo) }), Cmdlet("Get", "PSSnapin", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113330")]
    public sealed class GetPSSnapinCommand : PSSnapInCommandBase
    {
        private string[] _pssnapins;

        protected override void BeginProcessing()
        {
            if (this._pssnapins != null)
            {
                foreach (string str in this._pssnapins)
                {
                    Exception innerException = null;
                    try
                    {
                        Collection<PSSnapInInfo> snapIns = base.GetSnapIns(str);
                        if (snapIns.Count == 0)
                        {
                            base.WriteNonTerminatingError(str, "NoPSSnapInsFound", PSTraceSource.NewArgumentException(str, "MshSnapInCmdletResources", "NoPSSnapInsFound", new object[] { str }), ErrorCategory.InvalidArgument);
                            continue;
                        }
                        foreach (PSSnapInInfo info in snapIns)
                        {
                            info.LoadIndirectResources(base.ResourceReader);
                            base.WriteObject(info);
                        }
                    }
                    catch (SecurityException exception2)
                    {
                        innerException = exception2;
                    }
                    catch (PSArgumentException exception3)
                    {
                        innerException = exception3;
                    }
                    if (innerException != null)
                    {
                        base.WriteNonTerminatingError(str, "GetPSSnapInRead", innerException, ErrorCategory.InvalidArgument);
                    }
                }
            }
            else if (base.ShouldGetAll)
            {
                Exception exception4 = null;
                try
                {
                    foreach (PSSnapInInfo info2 in PSSnapInReader.ReadAll())
                    {
                        info2.LoadIndirectResources(base.ResourceReader);
                        base.WriteObject(info2);
                    }
                }
                catch (SecurityException exception5)
                {
                    exception4 = exception5;
                }
                catch (PSArgumentException exception6)
                {
                    exception4 = exception6;
                }
                if (exception4 != null)
                {
                    base.WriteNonTerminatingError(this, "GetPSSnapInRead", exception4, ErrorCategory.InvalidArgument);
                }
            }
            else
            {
                foreach (PSSnapInInfo info3 in base.GetSnapIns(null))
                {
                    info3.LoadIndirectResources(base.ResourceReader);
                    base.WriteObject(info3);
                }
            }
        }

        [Parameter(Position=0, Mandatory=false)]
        public string[] Name
        {
            get
            {
                return this._pssnapins;
            }
            set
            {
                this._pssnapins = value;
            }
        }

        [Parameter(Mandatory=false)]
        public SwitchParameter Registered
        {
            get
            {
                return base.ShouldGetAll;
            }
            set
            {
                base.ShouldGetAll = (bool) value;
            }
        }
    }
}

