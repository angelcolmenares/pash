using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Push", "Location", DefaultParameterSetName="Path", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113370")]
	public class PushLocationCommand : CoreCommandBase
	{
		private string path;

		private bool passThrough;

		private string stackName;

		[Alias(new string[] { "PSPath" })]
		[Parameter(ParameterSetName="LiteralPath", ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
		public string LiteralPath
		{
			get
			{
				return this.path;
			}
			set
			{
				base.SuppressWildcardExpansion = true;
				this.path = value;
			}
		}

		[Parameter]
		public SwitchParameter PassThru
		{
			get
			{
				return this.passThrough;
			}
			set
			{
				this.passThrough = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="Path", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		public string Path
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string StackName
		{
			get
			{
				return this.stackName;
			}
			set
			{
				this.stackName = value;
			}
		}

		public PushLocationCommand()
		{
			this.path = string.Empty;
		}

		protected override void ProcessRecord()
		{
			base.SessionState.Path.PushCurrentLocation(this.stackName);
			if (this.Path != null)
			{
				try
				{
					PathInfo pathInfo = base.SessionState.Path.SetLocation(this.Path, this.CmdletProviderContext);
					if (this.PassThru)
					{
						base.WriteObject(pathInfo);
					}
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
				catch (PSArgumentException pSArgumentException1)
				{
					PSArgumentException pSArgumentException = pSArgumentException1;
					base.WriteError(new ErrorRecord(pSArgumentException.ErrorRecord, pSArgumentException));
				}
			}
		}
	}
}