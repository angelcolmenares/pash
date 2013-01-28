using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Move", "Item", DefaultParameterSetName="Path", SupportsShouldProcess=true, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113350")]
	public class MoveItemCommand : CoreCommandWithCredentialsBase
	{
		private string[] paths;

		private string destination;

		private bool passThrough;

		[Parameter(Position=1, ValueFromPipelineByPropertyName=true)]
		public string Destination
		{
			get
			{
				return this.destination;
			}
			set
			{
				this.destination = value;
			}
		}

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

		public MoveItemCommand()
		{
			this.destination = ".";
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			if (this.Path == null || (int)this.Path.Length <= 0)
			{
				return base.InvokeProvider.Item.MoveItemDynamicParameters(".", this.Destination, context);
			}
			else
			{
				return base.InvokeProvider.Item.MoveItemDynamicParameters(this.Path[0], this.Destination, context);
			}
		}

		private Collection<PathInfo> GetResolvedPaths(string path)
		{
			Collection<PathInfo> pathInfos = new Collection<PathInfo>();
			try
			{
				pathInfos = base.SessionState.Path.GetResolvedPSPathFromPSPath(path);
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
			return pathInfos;
		}

		private void MoveItem(string path)
		{
			CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
			try
			{
				string str = path;
				if (!base.SuppressWildcardExpansion)
				{
					str = WildcardPattern.Escape(path);
				}
				if (!base.InvokeProvider.Item.Exists(str, cmdletProviderContext))
				{
					object[] objArray = new object[1];
					objArray[0] = path;
					PSInvalidOperationException pSInvalidOperationException = PSTraceSource.NewInvalidOperationException("NavigationResources", "MoveItemDoesntExist", objArray);
					base.WriteError(new ErrorRecord(pSInvalidOperationException.ErrorRecord, pSInvalidOperationException));
					return;
				}
			}
			catch (PSNotSupportedException pSNotSupportedException1)
			{
				PSNotSupportedException pSNotSupportedException = pSNotSupportedException1;
				base.WriteError(new ErrorRecord(pSNotSupportedException.ErrorRecord, pSNotSupportedException));
				return;
			}
			catch (DriveNotFoundException driveNotFoundException1)
			{
				DriveNotFoundException driveNotFoundException = driveNotFoundException1;
				base.WriteError(new ErrorRecord(driveNotFoundException.ErrorRecord, driveNotFoundException));
				return;
			}
			catch (ProviderNotFoundException providerNotFoundException1)
			{
				ProviderNotFoundException providerNotFoundException = providerNotFoundException1;
				base.WriteError(new ErrorRecord(providerNotFoundException.ErrorRecord, providerNotFoundException));
				return;
			}
			catch (ItemNotFoundException itemNotFoundException1)
			{
				ItemNotFoundException itemNotFoundException = itemNotFoundException1;
				base.WriteError(new ErrorRecord(itemNotFoundException.ErrorRecord, itemNotFoundException));
				return;
			}
			bool flag = false;
			try
			{
				flag = base.SessionState.Path.IsCurrentLocationOrAncestor(path, cmdletProviderContext);
			}
			catch (PSNotSupportedException pSNotSupportedException3)
			{
				PSNotSupportedException pSNotSupportedException2 = pSNotSupportedException3;
				base.WriteError(new ErrorRecord(pSNotSupportedException2.ErrorRecord, pSNotSupportedException2));
				return;
			}
			catch (DriveNotFoundException driveNotFoundException3)
			{
				DriveNotFoundException driveNotFoundException2 = driveNotFoundException3;
				base.WriteError(new ErrorRecord(driveNotFoundException2.ErrorRecord, driveNotFoundException2));
				return;
			}
			catch (ProviderNotFoundException providerNotFoundException3)
			{
				ProviderNotFoundException providerNotFoundException2 = providerNotFoundException3;
				base.WriteError(new ErrorRecord(providerNotFoundException2.ErrorRecord, providerNotFoundException2));
				return;
			}
			catch (ItemNotFoundException itemNotFoundException3)
			{
				ItemNotFoundException itemNotFoundException2 = itemNotFoundException3;
				base.WriteError(new ErrorRecord(itemNotFoundException2.ErrorRecord, itemNotFoundException2));
				return;
			}
			if (!flag)
			{
				CmdletProviderContext passThru = cmdletProviderContext;
				passThru.PassThru = this.PassThru;
				object[] destination = new object[2];
				destination[0] = path;
				destination[1] = this.Destination;
				CoreCommandBase.tracer.WriteLine("Moving {0} to {1}", destination);
				try
				{
					string str1 = path;
					if (!base.SuppressWildcardExpansion)
					{
						str1 = WildcardPattern.Escape(path);
					}
					base.InvokeProvider.Item.Move(str1, this.Destination, passThru);
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
			else
			{
				object[] objArray1 = new object[1];
				objArray1[0] = path;
				PSInvalidOperationException pSInvalidOperationException1 = PSTraceSource.NewInvalidOperationException("NavigationResources", "MoveItemInUse", objArray1);
				base.WriteError(new ErrorRecord(pSInvalidOperationException1.ErrorRecord, pSInvalidOperationException1));
				return;
			}
		}

		protected override void ProcessRecord()
		{
			string[] path = this.Path;
			for (int i = 0; i < (int)path.Length; i++)
			{
				string str = path[i];
				if (!base.SuppressWildcardExpansion)
				{
					Collection<PathInfo> resolvedPaths = this.GetResolvedPaths(str);
					foreach (PathInfo resolvedPath in resolvedPaths)
					{
						string path1 = resolvedPath.Path;
						this.MoveItem(path1);
					}
				}
				else
				{
					this.MoveItem(str);
				}
			}
		}
	}
}