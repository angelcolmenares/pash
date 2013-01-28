using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Resolve", "Path", DefaultParameterSetName="Path", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113384")]
	public class ResolvePathCommand : CoreCommandWithCredentialsBase
	{
		private SwitchParameter relative;

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

		[Parameter]
		public SwitchParameter Relative
		{
			get
			{
				return this.relative;
			}
			set
			{
				this.relative = value;
			}
		}

		public ResolvePathCommand()
		{
		}

		protected override void ProcessRecord()
		{
			Collection<PathInfo> resolvedPSPathFromPSPath;
			string[] path = this.Path;
			for (int i = 0; i < (int)path.Length; i++)
			{
				string str = path[i];
				resolvedPSPathFromPSPath = null;
				try
				{
					resolvedPSPathFromPSPath = base.SessionState.Path.GetResolvedPSPathFromPSPath(str, this.CmdletProviderContext);
					if (this.relative)
					{
						foreach (PathInfo pathInfo in resolvedPSPathFromPSPath)
						{
							string str1 = base.SessionState.Path.NormalizeRelativePath(pathInfo.Path, base.SessionState.Path.CurrentLocation.ProviderPath);
							if (!str1.StartsWith(".", StringComparison.OrdinalIgnoreCase))
							{
								str1 = base.SessionState.Path.Combine(".", str1);
							}
							base.WriteObject(str1, false);
						}
					}
					goto Label0;
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
			return;
		Label0:
			if (!this.relative)
			{
				base.WriteObject(resolvedPSPathFromPSPath, true);
                return;
			}
			else
			{
                return;
			}
		}
	}
}