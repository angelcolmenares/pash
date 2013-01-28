using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Set", "Location", DefaultParameterSetName="Path", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113397")]
	[OutputType(new Type[] { typeof(PathInfo), typeof(PathInfoStack) })]
	public class SetLocationCommand : CoreCommandBase
	{
		private const string pathSet = "Path";

		private const string literalPathSet = "LiteralPath";

		private const string stackSet = "Stack";

		private string path;

		private bool passThrough;

		private string stackName;

		[Alias(new string[] { "PSPath" })]
		[Parameter(ParameterSetName="LiteralPath", Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
		public string LiteralPath
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
				base.SuppressWildcardExpansion = true;
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

		[Parameter(ParameterSetName="Stack", ValueFromPipelineByPropertyName=true)]
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

		public SetLocationCommand()
		{
			this.path = string.Empty;
		}

		protected override void ProcessRecord()
		{
			object obj = null;
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "Path" || str == "LiteralPath")
				{
					try
					{
						obj = base.SessionState.Path.SetLocation(this.Path, this.CmdletProviderContext);
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
				else
				{
					if (str == "Stack")
					{
						try
						{
							obj = base.SessionState.Path.SetDefaultLocationStack(this.StackName);
						}
						catch (ItemNotFoundException itemNotFoundException3)
						{
							ItemNotFoundException itemNotFoundException2 = itemNotFoundException3;
							base.WriteError(new ErrorRecord(itemNotFoundException2.ErrorRecord, itemNotFoundException2));
						}
					}
				}
			}
			if (this.passThrough && obj != null)
			{
				base.WriteObject(obj);
			}
		}
	}
}