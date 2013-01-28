using Microsoft.PowerShell.Commands.Management;
using System;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Set", "Content", DefaultParameterSetName="Path", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113392")]
	public class SetContentCommand : WriteContentCommandBase
	{
		public SetContentCommand()
		{
		}

		internal override void BeforeOpenStreams(string[] paths)
		{
			if (paths == null || paths != null && (int)paths.Length == 0)
			{
				throw PSTraceSource.NewArgumentNullException("paths");
			}
			else
			{
				CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(base.GetCurrentContext());
				string[] strArrays = paths;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					try
					{
						base.InvokeProvider.Content.Clear(str, cmdletProviderContext);
						cmdletProviderContext.ThrowFirstErrorOrDoNothing(true);
					}
					catch (PSNotSupportedException pSNotSupportedException)
					{
					}
					catch (DriveNotFoundException driveNotFoundException1)
					{
						DriveNotFoundException driveNotFoundException = driveNotFoundException1;
						base.WriteError(new ErrorRecord(driveNotFoundException.ErrorRecord, driveNotFoundException));
					}
					catch (ProviderNotFoundException providerNotFoundException1)
					{
						ProviderNotFoundException providerNotFoundException = providerNotFoundException1;
						base.WriteError(new ErrorRecord(providerNotFoundException.ErrorRecord, providerNotFoundException));
					}
					catch (ItemNotFoundException itemNotFoundException)
					{
					}
				}
				return;
			}
		}

		internal override bool CallShouldProcess(string path)
		{
			string setContentAction = NavigationResources.SetContentAction;
			string str = StringUtil.Format(NavigationResources.SetContentTarget, path);
			return base.ShouldProcess(str, setContentAction);
		}
	}
}