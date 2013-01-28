namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Security;

    [OutputType(new Type[] { typeof(PSSnapInInfo) }), Cmdlet("Add", "PSSnapin", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113281")]
    public sealed class AddPSSnapinCommand : PSSnapInCommandBase
    {
        private bool _passThru;
        private string[] _pssnapins;

        private void AddPSSnapIns(Collection<string> snapInList)
        {
            if (snapInList != null)
            {
                if (base.Context.RunspaceConfiguration == null)
                {
                    Collection<PSSnapInInfo> snapIns = base.GetSnapIns(null);
                    InitialSessionState state = InitialSessionState.Create();
                    bool flag = false;
                    foreach (string str in snapInList)
                    {
                        if (InitialSessionState.IsEngineModule(str))
                        {
                            base.WriteNonTerminatingError(str, "LoadSystemSnapinAsModule", PSTraceSource.NewArgumentException(str, "MshSnapInCmdletResources", "LoadSystemSnapinAsModule", new object[] { str }), ErrorCategory.InvalidArgument);
                        }
                        else
                        {
                            try
                            {
                                PSSnapInInfo psSnapInInfo = PSSnapInReader.Read(Utils.GetCurrentMajorVersion(), str);
                                PSSnapInInfo info2 = PSSnapInCommandBase.IsSnapInLoaded(snapIns, psSnapInInfo);
                                if (info2 == null)
                                {
                                    PSSnapInException exception;
                                    info2 = state.ImportPSSnapIn(str, out exception);
                                    flag = true;
                                    base.Context.InitialSessionState.ImportedSnapins.Add(info2.Name, info2);
                                }
                                if (this._passThru)
                                {
                                    info2.LoadIndirectResources(base.ResourceReader);
                                    base.WriteObject(info2);
                                }
                            }
                            catch (PSSnapInException exception2)
                            {
                                base.WriteNonTerminatingError(str, "AddPSSnapInRead", exception2, ErrorCategory.InvalidData);
                            }
                        }
                    }
                    if (flag)
                    {
                        state.Bind(base.Context, true);
                    }
                }
                else
                {
                    foreach (string str2 in snapInList)
                    {
                        Exception innerException = null;
                        try
                        {
                            PSSnapInException warning = null;
                            PSSnapInInfo sendToPipeline = base.Runspace.AddPSSnapIn(str2, out warning);
                            if (warning != null)
                            {
                                base.WriteNonTerminatingError(str2, "AddPSSnapInRead", warning, ErrorCategory.InvalidData);
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
                        catch (PSSnapInException exception6)
                        {
                            innerException = exception6;
                        }
                        catch (SecurityException exception7)
                        {
                            innerException = exception7;
                        }
                        if (innerException != null)
                        {
                            base.WriteNonTerminatingError(str2, "AddPSSnapInRead", innerException, ErrorCategory.InvalidArgument);
                        }
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            Collection<PSSnapInInfo> searchList = null;
            foreach (string str in this._pssnapins)
            {
                Exception innerException = null;
                Collection<string> snapInList = new Collection<string>();
                try
                {
                    if (WildcardPattern.ContainsWildcardCharacters(str))
                    {
                        if (searchList == null)
                        {
                            searchList = PSSnapInReader.ReadAll(PSVersionInfo.RegistryVersion1Key);
                        }
                        snapInList = base.SearchListForPattern(searchList, str);
                        if (snapInList.Count != 0)
                        {
                            goto Label_0088;
                        }
                        if (this._passThru)
                        {
                            base.WriteNonTerminatingError(str, "NoPSSnapInsFound", PSTraceSource.NewArgumentException(str, "MshSnapInCmdletResources", "NoPSSnapInsFound", new object[] { str }), ErrorCategory.InvalidArgument);
                        }
                        continue;
                    }
                    snapInList.Add(str);
                Label_0088:
                    this.AddPSSnapIns(snapInList);
                }
                catch (PSArgumentException exception2)
                {
                    innerException = exception2;
                }
                catch (SecurityException exception3)
                {
                    innerException = exception3;
                }
                if (innerException != null)
                {
                    base.WriteNonTerminatingError(str, "AddPSSnapInRead", innerException, ErrorCategory.InvalidArgument);
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

