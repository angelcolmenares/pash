using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Remove", "Item", SupportsShouldProcess=true, DefaultParameterSetName="Path", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113373")]
	public class RemoveItemCommand : CoreCommandWithCredentialsBase
	{
		private string[] paths;

		private bool recurse;

		[Parameter]
		public override string[] Exclude
		{
			get
			{
				return base.Exclude;
			}
			set
			{
				base.Exclude = value;
			}
		}

		[Parameter]
		public override string Filter
		{
			get
			{
				return base.Filter;
			}
			set
			{
				base.Filter = value;
			}
		}

		[Parameter]
		public override SwitchParameter Force
		{
			get
			{
				return base.Force;
			}
			set
			{
				base.Force = value;
			}
		}

		[Parameter]
		public override string[] Include
		{
			get
			{
				return base.Include;
			}
			set
			{
				base.Include = value;
			}
		}

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

		protected override bool ProviderSupportsShouldProcess
		{
			get
			{
				return base.DoesProviderSupportShouldProcess(this.paths);
			}
		}

		[Parameter]
		public SwitchParameter Recurse
		{
			get
			{
				return this.recurse;
			}
			set
			{
				this.recurse = value;
			}
		}

		public RemoveItemCommand()
		{
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			if (this.Path == null || (int)this.Path.Length <= 0)
			{
				return base.InvokeProvider.Item.RemoveItemDynamicParameters(".", this.Recurse, context);
			}
			else
			{
				return base.InvokeProvider.Item.RemoveItemDynamicParameters(this.Path[0], this.Recurse, context);
			}
		}

		protected override void ProcessRecord()
		{
			Collection<PathInfo> resolvedPSPathFromPSPath;
			CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
			bool flag = false;
			bool flag1 = false;
			string[] path = this.Path;
			for (int i = 0; i < (int)path.Length; i++)
			{
				string str = path[i];
				resolvedPSPathFromPSPath = null;
				try
				{
					resolvedPSPathFromPSPath = base.SessionState.Path.GetResolvedPSPathFromPSPath(str, cmdletProviderContext);
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
			IEnumerator<PathInfo> enumerator = resolvedPSPathFromPSPath.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					PathInfo current = enumerator.Current;
					bool flag2 = false;
					try
					{
						flag2 = base.SessionState.Path.IsCurrentLocationOrAncestor(current.Path, cmdletProviderContext);
					}
					catch (PSNotSupportedException pSNotSupportedException3)
					{
						PSNotSupportedException pSNotSupportedException2 = pSNotSupportedException3;
						base.WriteError(new ErrorRecord(pSNotSupportedException2.ErrorRecord, pSNotSupportedException2));
						continue;
					}
					catch (DriveNotFoundException driveNotFoundException3)
					{
						DriveNotFoundException driveNotFoundException2 = driveNotFoundException3;
						base.WriteError(new ErrorRecord(driveNotFoundException2.ErrorRecord, driveNotFoundException2));
						continue;
					}
					catch (ProviderNotFoundException providerNotFoundException3)
					{
						ProviderNotFoundException providerNotFoundException2 = providerNotFoundException3;
						base.WriteError(new ErrorRecord(providerNotFoundException2.ErrorRecord, providerNotFoundException2));
						continue;
					}
					catch (ItemNotFoundException itemNotFoundException3)
					{
						ItemNotFoundException itemNotFoundException2 = itemNotFoundException3;
						base.WriteError(new ErrorRecord(itemNotFoundException2.ErrorRecord, itemNotFoundException2));
						continue;
					}
					if (!flag2)
					{
						bool flag3 = false;
						string unresolvedProviderPathFromPSPath = base.GetUnresolvedProviderPathFromPSPath(current.Path);
						try
						{
							flag3 = base.SessionState.Internal.HasChildItems(current.Provider.Name, unresolvedProviderPathFromPSPath, cmdletProviderContext);
							cmdletProviderContext.ThrowFirstErrorOrDoNothing();
						}
						catch (PSNotSupportedException pSNotSupportedException5)
						{
							PSNotSupportedException pSNotSupportedException4 = pSNotSupportedException5;
							base.WriteError(new ErrorRecord(pSNotSupportedException4.ErrorRecord, pSNotSupportedException4));
							continue;
						}
						catch (DriveNotFoundException driveNotFoundException5)
						{
							DriveNotFoundException driveNotFoundException4 = driveNotFoundException5;
							base.WriteError(new ErrorRecord(driveNotFoundException4.ErrorRecord, driveNotFoundException4));
							continue;
						}
						catch (ProviderNotFoundException providerNotFoundException5)
						{
							ProviderNotFoundException providerNotFoundException4 = providerNotFoundException5;
							base.WriteError(new ErrorRecord(providerNotFoundException4.ErrorRecord, providerNotFoundException4));
							continue;
						}
						catch (ItemNotFoundException itemNotFoundException5)
						{
							ItemNotFoundException itemNotFoundException4 = itemNotFoundException5;
							base.WriteError(new ErrorRecord(itemNotFoundException4.ErrorRecord, itemNotFoundException4));
							continue;
						}
						if (!this.Recurse && flag3)
						{
							string str1 = StringUtil.Format(NavigationResources.RemoveItemWithChildren, current.Path);
							if (!base.ShouldContinue(str1, null, ref flag, ref flag1))
							{
								continue;
							}
						}
						try
						{
							base.SessionState.Internal.RemoveItem(current.Provider.Name, unresolvedProviderPathFromPSPath, this.Recurse, cmdletProviderContext);
						}
						catch (PSNotSupportedException pSNotSupportedException7)
						{
							PSNotSupportedException pSNotSupportedException6 = pSNotSupportedException7;
							base.WriteError(new ErrorRecord(pSNotSupportedException6.ErrorRecord, pSNotSupportedException6));
						}
						catch (DriveNotFoundException driveNotFoundException7)
						{
							DriveNotFoundException driveNotFoundException6 = driveNotFoundException7;
							base.WriteError(new ErrorRecord(driveNotFoundException6.ErrorRecord, driveNotFoundException6));
						}
						catch (ProviderNotFoundException providerNotFoundException7)
						{
							ProviderNotFoundException providerNotFoundException6 = providerNotFoundException7;
							base.WriteError(new ErrorRecord(providerNotFoundException6.ErrorRecord, providerNotFoundException6));
						}
						catch (ItemNotFoundException itemNotFoundException7)
						{
							ItemNotFoundException itemNotFoundException6 = itemNotFoundException7;
							base.WriteError(new ErrorRecord(itemNotFoundException6.ErrorRecord, itemNotFoundException6));
						}
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = current.Path;
						PSInvalidOperationException pSInvalidOperationException = PSTraceSource.NewInvalidOperationException("NavigationResources", "RemoveItemInUse", objArray);
						base.WriteError(new ErrorRecord(pSInvalidOperationException.ErrorRecord, pSInvalidOperationException));
					}
				}
                return;
			}
		}
	}
}