using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Test", "Path", DefaultParameterSetName="Path", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113418")]
	[OutputType(new Type[] { typeof(bool) })]
	public class TestPathCommand : CoreCommandWithCredentialsBase
	{
		private string[] paths;

		private TestPathType type;

		private SwitchParameter isValid;

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

		[Parameter]
		public SwitchParameter IsValid
		{
			get
			{
				return this.isValid;
			}
			set
			{
				this.isValid = value;
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

		[Alias(new string[] { "Type" })]
		[Parameter]
		public TestPathType PathType
		{
			get
			{
				return this.type;
			}
			set
			{
				this.type = value;
			}
		}

		public TestPathCommand()
		{
			this.isValid = new SwitchParameter();
		}

		internal override object GetDynamicParameters(CmdletProviderContext context)
		{
			object obj = null;
			if (this.PathType == TestPathType.Any && !this.IsValid)
			{
				if (this.Path == null || (int)this.Path.Length <= 0)
				{
					obj = base.InvokeProvider.Item.ItemExistsDynamicParameters(".", context);
				}
				else
				{
					obj = base.InvokeProvider.Item.ItemExistsDynamicParameters(this.Path[0], context);
				}
			}
			return obj;
		}

		protected override void ProcessRecord()
		{
			bool flag;
			CmdletProviderContext cmdletProviderContext = this.CmdletProviderContext;
			string[] strArrays = this.paths;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				bool flag1 = false;
				try
				{
					if (!this.IsValid)
					{
						if (this.PathType != TestPathType.Container)
						{
							if (this.PathType != TestPathType.Leaf)
							{
								flag1 = base.InvokeProvider.Item.Exists(str, cmdletProviderContext);
							}
							else
							{
								if (!base.InvokeProvider.Item.Exists(str, cmdletProviderContext))
								{
									flag = false;
								}
								else
								{
									flag = !base.InvokeProvider.Item.IsContainer(str, cmdletProviderContext);
								}
								flag1 = flag;
							}
						}
						else
						{
							flag1 = base.InvokeProvider.Item.IsContainer(str, cmdletProviderContext);
						}
					}
					else
					{
						flag1 = base.SessionState.Path.IsValid(str, cmdletProviderContext);
					}
				}
				catch (PSNotSupportedException pSNotSupportedException)
				{
				}
				catch (DriveNotFoundException driveNotFoundException)
				{
				}
				catch (ProviderNotFoundException providerNotFoundException)
				{
				}
				catch (ItemNotFoundException itemNotFoundException)
				{
				}
				base.WriteObject(flag1);
			}
		}
	}
}