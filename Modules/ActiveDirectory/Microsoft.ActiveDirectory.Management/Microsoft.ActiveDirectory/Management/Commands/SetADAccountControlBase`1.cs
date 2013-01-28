using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class SetADAccountControlBase<P> : ADSetCmdletBase<P, ADAccountFactory<ADAccount>, ADAccount>
	where P : ADParameterSet, new()
	{
		public SetADAccountControlBase()
		{
		}

		internal SetADAccountControlBase(SetADAccountControlAction action)
		{
			SetADAccountControlAction setADAccountControlAction = action;
			switch (setADAccountControlAction)
			{
				case SetADAccountControlAction.Unlock:
				{
					this._cmdletParameters["LockedOut"] = false;
					return;
				}
				case SetADAccountControlAction.Disable:
				{
					this._cmdletParameters["Enabled"] = false;
					return;
				}
				case SetADAccountControlAction.Enable:
				{
					this._cmdletParameters["Enabled"] = true;
					return;
				}
				default:
				{
					return;
				}
			}
		}
	}
}