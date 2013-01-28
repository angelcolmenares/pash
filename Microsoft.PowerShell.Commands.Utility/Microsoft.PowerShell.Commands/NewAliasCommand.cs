namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    [OutputType(new Type[] { typeof(AliasInfo) }), Cmdlet("New", "Alias", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113352")]
    public class NewAliasCommand : WriteAliasCommandBase
    {
        protected override void ProcessRecord()
        {
            if (base.Force == 0)
            {
                AliasInfo valueToCheck = null;
                if (string.IsNullOrEmpty(base.Scope))
                {
                    valueToCheck = base.SessionState.Internal.GetAlias(base.Name);
                }
                else
                {
                    valueToCheck = base.SessionState.Internal.GetAliasAtScope(base.Name, base.Scope);
                }
                if (valueToCheck != null)
                {
                    SessionState.ThrowIfNotVisible(base.CommandOrigin, valueToCheck);
                    SessionStateException replaceParentContainsErrorRecordException = new SessionStateException(base.Name, SessionStateCategory.Alias, "AliasAlreadyExists", SessionStateStrings.AliasAlreadyExists, ErrorCategory.ResourceExists, new object[0]);
                    base.WriteError(new ErrorRecord(replaceParentContainsErrorRecordException.ErrorRecord, replaceParentContainsErrorRecordException));
                    return;
                }
            }
            AliasInfo alias = new AliasInfo(base.Name, base.Value, base.Context, base.Option) {
                Description = base.Description
            };
            string newAliasAction = AliasCommandStrings.NewAliasAction;
            string target = StringUtil.Format(AliasCommandStrings.NewAliasTarget, base.Name, base.Value);
            if (base.ShouldProcess(target, newAliasAction))
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
                catch (SessionStateException exception2)
                {
                    base.WriteError(new ErrorRecord(exception2.ErrorRecord, exception2));
                    return;
                }
                catch (PSArgumentOutOfRangeException exception3)
                {
                    base.WriteError(new ErrorRecord(exception3.ErrorRecord, exception3));
                    return;
                }
                catch (PSArgumentException exception4)
                {
                    base.WriteError(new ErrorRecord(exception4.ErrorRecord, exception4));
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

