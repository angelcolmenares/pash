using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Convert", "Path", DefaultParameterSetName="Path", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113289", RemotingCapability=RemotingCapability.None)]
	[OutputType(new Type[] { typeof(string) })]
	public class ConvertPathCommand : CoreCommandBase
	{
		private string[] paths;

		[Alias(new string[] { "PSPath" })]
		[Parameter(ParameterSetName="LiteralPath", Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
		public string[] LiteralPath
		{
			get
			{
				return this.paths;
			}
			set
			{
				base.SuppressWildcardExpansion = true;
				this.paths = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="Path", Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		public string[] Path
		{
			get
			{
				return this.paths;
			}
			set
			{
				this.paths = value;
			}
		}

		public ConvertPathCommand()
		{
		}

		protected override void ProcessRecord()
		{
			ProviderInfo providerInfo = null;
			string[] path = this.Path;
			for (int i = 0; i < (int)path.Length; i++)
			{
				string str = path[i];
				try
				{
					Collection<string> resolvedProviderPathFromPSPath = base.SessionState.Path.GetResolvedProviderPathFromPSPath(str, this.CmdletProviderContext, out providerInfo);
					base.WriteObject(resolvedProviderPathFromPSPath, true);
				}
				catch (PSNotSupportedException pSNotSupportedException1)
				{
					PSNotSupportedException pSNotSupportedException = pSNotSupportedException1;
					base.WriteError(new ErrorRecord(pSNotSupportedException.ErrorRecord, pSNotSupportedException));
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
				catch (ItemNotFoundException itemNotFoundException1)
				{
					ItemNotFoundException itemNotFoundException = itemNotFoundException1;
					base.WriteError(new ErrorRecord(itemNotFoundException.ErrorRecord, itemNotFoundException));
				}
			}
		}
	}
}