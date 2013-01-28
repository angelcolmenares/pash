namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [Cmdlet("Set", "Alias", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113390"), OutputType(new Type[] { typeof(AliasInfo) })]
    public class SetAliasCommand : WriteAliasCommandBase
    {
        protected override void ProcessRecord()
        {
            AliasInfo alias = new AliasInfo(base.Name, base.Value, base.Context, base.Option) {
                Description = base.Description
            };
            string setAliasAction = AliasCommandStrings.SetAliasAction;
            string target = StringUtil.Format(AliasCommandStrings.SetAliasTarget, base.Name, base.Value);
            if (base.ShouldProcess(target, setAliasAction))
            {
                AliasInfo sendToPipeline = null;
                try
                {
                    if (string.IsNullOrEmpty(base.Scope))
                    {
                        sendToPipeline = base.SessionState.Internal.SetAliasItem(alias, (bool) base.Force, base.MyInvocation.CommandOrigin);
                    }
                    else
                    {
                        sendToPipeline = base.SessionState.Internal.SetAliasItemAtScope(alias, base.Scope, (bool) base.Force, base.MyInvocation.CommandOrigin);
                    }
                }
                catch (SessionStateException exception)
                {
                    base.WriteError(new ErrorRecord(exception.ErrorRecord, exception));
                    return;
                }
                if ((base.PassThru != 0) && (sendToPipeline != null))
                {
                    base.WriteObject(sendToPipeline);
                }
            }
        }
    }
}

