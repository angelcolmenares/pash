namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    [Cmdlet("Remove", "PSSnapin", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113378"), OutputType(new Type[] { typeof(PSSnapInInfo) })]
    public sealed class RemovePSSnapinCommand : PSSnapInCommandBase
    {
        private bool _passThru;
        private string[] _pssnapins;

        protected override void ProcessRecord()
        {
            foreach (string str in this._pssnapins)
            {
                Collection<PSSnapInInfo> snapIns = base.GetSnapIns(str);
                if (snapIns.Count == 0)
                {
                    base.WriteNonTerminatingError(str, "NoPSSnapInsFound", PSTraceSource.NewArgumentException(str, "MshSnapInCmdletResources", "NoPSSnapInsFound", new object[] { str }), ErrorCategory.InvalidArgument);
                }
                else
                {
                    foreach (PSSnapInInfo info in snapIns)
                    {
                        if (base.ShouldProcess(info.Name))
                        {
                            Exception innerException = null;
                            if ((base.Runspace == null) && (base.Context.InitialSessionState != null))
                            {
                                try
                                {
                                    PSSnapInException exception2;
                                    PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(info.Name);
                                    if (MshConsoleInfo.IsDefaultPSSnapIn(info.Name, base.Context.InitialSessionState.defaultSnapins))
                                    {
                                        throw PSTraceSource.NewArgumentException(info.Name, "ConsoleInfoErrorStrings", "CannotRemoveDefault", new object[] { info.Name });
                                    }
                                    InitialSessionState state = InitialSessionState.Create();
                                    state.ImportPSSnapIn(info, out exception2);
                                    state.Unbind(base.Context);
                                    base.Context.InitialSessionState.ImportedSnapins.Remove(info.Name);
                                }
                                catch (PSArgumentException exception3)
                                {
                                    innerException = exception3;
                                }
                                if (innerException != null)
                                {
                                    base.WriteNonTerminatingError(str, "RemovePSSnapIn", innerException, ErrorCategory.InvalidArgument);
                                }
                            }
                            else
                            {
                                try
                                {
                                    PSSnapInException warning = null;
                                    PSSnapInInfo sendToPipeline = base.Runspace.RemovePSSnapIn(info.Name, out warning);
                                    if (warning != null)
                                    {
                                        base.WriteNonTerminatingError(info.Name, "RemovePSSnapInRead", warning, ErrorCategory.InvalidData);
                                    }
                                    if (this._passThru)
                                    {
                                        sendToPipeline.LoadIndirectResources(base.ResourceReader);
                                        base.WriteObject(sendToPipeline);
                                    }
                                }
                                catch (PSArgumentException exception5)
                                {
                                    innerException = exception5;
                                }
                                if (innerException != null)
                                {
                                    base.WriteNonTerminatingError(str, "RemovePSSnapIn", innerException, ErrorCategory.InvalidArgument);
                                }
                            }
                        }
                    }
                }
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true)]
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

        [Parameter]
        public SwitchParameter PassThru
        {
            get
            {
                return this._passThru;
            }
            set
            {
                this._passThru = (bool) value;
            }
        }
    }
}

