using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Join", "Path", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113347")]
	[OutputType(new Type[] { typeof(string) })]
	public class JoinPathCommand : CoreCommandWithCredentialsBase
	{
		private string[] paths;

		private string childPath;

		private bool resolve;

		[AllowEmptyString]
		[AllowNull]
		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string ChildPath
		{
			get
			{
				return this.childPath;
			}
			set
			{
				this.childPath = value;
			}
		}

		[Alias(new string[] { "PSPath" })]
		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
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
		public SwitchParameter Resolve
		{
			get
			{
				return this.resolve;
			}
			set
			{
				this.resolve = value;
			}
		}

		public JoinPathCommand()
		{
			this.childPath = string.Empty;
		}

		protected override void ProcessRecord()
		{
			string[] path = this.Path;
			for (int i = 0; i < (int)path.Length; i++)
			{
				string str = path[i];
				string str1 = null;
				try
				{
					str1 = base.SessionState.Path.Combine(str, this.ChildPath, this.CmdletProviderContext);
				}
				catch (PSNotSupportedException pSNotSupportedException1)
				{
					PSNotSupportedException pSNotSupportedException = pSNotSupportedException1;
					base.WriteError(new ErrorRecord(pSNotSupportedException.ErrorRecord, pSNotSupportedException));
					goto Label0;
				}
				catch (DriveNotFoundException driveNotFoundException1)
				{
					DriveNotFoundException driveNotFoundException = driveNotFoundException1;
					base.WriteError(new ErrorRecord(driveNotFoundException.ErrorRecord, driveNotFoundException));
					goto Label0;
				}
				catch (ProviderNotFoundException providerNotFoundException1)
				{
					ProviderNotFoundException providerNotFoundException = providerNotFoundException1;
					base.WriteError(new ErrorRecord(providerNotFoundException.ErrorRecord, providerNotFoundException));
					goto Label0;
				}
				catch (ItemNotFoundException itemNotFoundException1)
				{
					ItemNotFoundException itemNotFoundException = itemNotFoundException1;
					base.WriteError(new ErrorRecord(itemNotFoundException.ErrorRecord, itemNotFoundException));
					goto Label0;
				}
				if (!this.Resolve)
				{
					if (str1 != null)
					{
						base.WriteObject(str1);
					}
				}
				else
				{
					Collection<PathInfo> resolvedPSPathFromPSPath = null;
					try
					{
						resolvedPSPathFromPSPath = base.SessionState.Path.GetResolvedPSPathFromPSPath(str1, this.CmdletProviderContext);
					}
					catch (PSNotSupportedException pSNotSupportedException3)
					{
						PSNotSupportedException pSNotSupportedException2 = pSNotSupportedException3;
						base.WriteError(new ErrorRecord(pSNotSupportedException2.ErrorRecord, pSNotSupportedException2));
						goto Label0;
					}
					catch (DriveNotFoundException driveNotFoundException3)
					{
						DriveNotFoundException driveNotFoundException2 = driveNotFoundException3;
						base.WriteError(new ErrorRecord(driveNotFoundException2.ErrorRecord, driveNotFoundException2));
						goto Label0;
					}
					catch (ProviderNotFoundException providerNotFoundException3)
					{
						ProviderNotFoundException providerNotFoundException2 = providerNotFoundException3;
						base.WriteError(new ErrorRecord(providerNotFoundException2.ErrorRecord, providerNotFoundException2));
						goto Label0;
					}
					catch (ItemNotFoundException itemNotFoundException3)
					{
						ItemNotFoundException itemNotFoundException2 = itemNotFoundException3;
						base.WriteError(new ErrorRecord(itemNotFoundException2.ErrorRecord, itemNotFoundException2));
						goto Label0;
					}
					for (int j = 0; j < resolvedPSPathFromPSPath.Count; j++)
					{
						try
						{
							if (resolvedPSPathFromPSPath[j] != null)
							{
								base.WriteObject(resolvedPSPathFromPSPath[j].Path);
							}
						}
						catch (PSNotSupportedException pSNotSupportedException5)
						{
							PSNotSupportedException pSNotSupportedException4 = pSNotSupportedException5;
							base.WriteError(new ErrorRecord(pSNotSupportedException4.ErrorRecord, pSNotSupportedException4));
						}
						catch (DriveNotFoundException driveNotFoundException5)
						{
							DriveNotFoundException driveNotFoundException4 = driveNotFoundException5;
							base.WriteError(new ErrorRecord(driveNotFoundException4.ErrorRecord, driveNotFoundException4));
						}
						catch (ProviderNotFoundException providerNotFoundException5)
						{
							ProviderNotFoundException providerNotFoundException4 = providerNotFoundException5;
							base.WriteError(new ErrorRecord(providerNotFoundException4.ErrorRecord, providerNotFoundException4));
						}
						catch (ItemNotFoundException itemNotFoundException5)
						{
							ItemNotFoundException itemNotFoundException4 = itemNotFoundException5;
							base.WriteError(new ErrorRecord(itemNotFoundException4.ErrorRecord, itemNotFoundException4));
						}
					}
				}
            Label0:
                continue;
			}
		}
	}
}