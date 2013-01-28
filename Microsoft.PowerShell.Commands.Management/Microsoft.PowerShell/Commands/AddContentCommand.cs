using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Add", "Content", DefaultParameterSetName="Path", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113278")]
	public class AddContentCommand : WriteContentCommandBase
	{
		public AddContentCommand()
		{
		}

		internal override bool CallShouldProcess(string path)
		{
			string addContentAction = NavigationResources.AddContentAction;
			string str = StringUtil.Format(NavigationResources.AddContentTarget, path);
			return base.ShouldProcess(str, addContentAction);
		}

		internal override void SeekContentPosition(List<ContentCommandBase.ContentHolder> contentHolders)
		{
			foreach (ContentCommandBase.ContentHolder contentHolder in contentHolders)
			{
				if (contentHolder.Writer == null)
				{
					continue;
				}
				try
				{
					contentHolder.Writer.Seek((long)0, SeekOrigin.End);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandsCommon.CheckForSevereException(this, exception);
					ProviderInvocationException providerInvocationException = new ProviderInvocationException("ProviderSeekError", SessionStateStrings.ProviderSeekError, contentHolder.PathInfo.Provider, contentHolder.PathInfo.Path, exception);
					MshLog.LogProviderHealthEvent(base.Context, contentHolder.PathInfo.Provider.Name, providerInvocationException, Severity.Warning);
					throw providerInvocationException;
				}
			}
		}
	}
}