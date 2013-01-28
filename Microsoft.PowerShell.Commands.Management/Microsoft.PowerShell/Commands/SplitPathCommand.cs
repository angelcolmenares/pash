using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Split", "Path", DefaultParameterSetName="ParentSet", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113404")]
	[OutputType(new Type[] { typeof(string) }, ParameterSetName=new string[] { "LeafSet", "NoQualifierSet", "ParentSet", "QualifierSet", "LiteralPathSet" })]
	[OutputType(new Type[] { typeof(bool) }, ParameterSetName=new string[] { "IsAbsoluteSet" })]
	public class SplitPathCommand : CoreCommandWithCredentialsBase
	{
		private const string parentSet = "ParentSet";

		private const string leafSet = "LeafSet";

		private const string qualifierSet = "QualifierSet";

		private const string noQualifierSet = "NoQualifierSet";

		private const string isAbsoluteSet = "IsAbsoluteSet";

		private const string literalPathSet = "LiteralPathSet";

		private string[] paths;

		private bool qualifier;

		private bool noqualifier;

		private bool parent;

		private bool leaf;

		private bool resolve;

		private bool isAbsolute;

		[Parameter(ParameterSetName="IsAbsoluteSet")]
		public SwitchParameter IsAbsolute
		{
			get
			{
				return this.isAbsolute;
			}
			set
			{
				this.isAbsolute = value;
			}
		}

		[Parameter(ParameterSetName="LeafSet", Mandatory=false, ValueFromPipelineByPropertyName=true)]
		public SwitchParameter Leaf
		{
			get
			{
				return this.leaf;
			}
			set
			{
				this.leaf = value;
			}
		}

		[Alias(new string[] { "PSPath" })]
		[Parameter(ParameterSetName="LiteralPathSet", Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
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

		[Parameter(ParameterSetName="NoQualifierSet", Mandatory=false, ValueFromPipelineByPropertyName=true)]
		public SwitchParameter NoQualifier
		{
			get
			{
				return this.noqualifier;
			}
			set
			{
				this.noqualifier = value;
			}
		}

		[Parameter(ParameterSetName="ParentSet", Mandatory=false, ValueFromPipelineByPropertyName=true)]
		public SwitchParameter Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				this.parent = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="QualifierSet", Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		[Parameter(Position=0, ParameterSetName="LeafSet", Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		[Parameter(Position=0, ParameterSetName="IsAbsoluteSet", Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		[Parameter(Position=0, ParameterSetName="ParentSet", Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		[Parameter(Position=0, ParameterSetName="NoQualifierSet", Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
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

		[Parameter(Position=1, ValueFromPipelineByPropertyName=true, ParameterSetName="QualifierSet", Mandatory=false)]
		public SwitchParameter Qualifier
		{
			get
			{
				return this.qualifier;
			}
			set
			{
				this.qualifier = value;
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

		public SplitPathCommand()
		{
			this.parent = true;
		}

		protected override void ProcessRecord()
		{
			CmdletProviderContext cmdletProviderContext;
			Collection<PathInfo> resolvedPSPathFromPSPath;
			StringCollection stringCollections = new StringCollection();
			if (!this.resolve)
			{
				stringCollections.AddRange(this.Path);
			}
			else
			{
				cmdletProviderContext = this.CmdletProviderContext;
				string[] strArrays = this.paths;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
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
			}
        Label3:
			int num = 0;
			while (num < stringCollections.Count)
			{
				string empty = null;
				string parameterSetName = base.ParameterSetName;
				string str1 = parameterSetName;
				if (parameterSetName != null)
				{
					if (str1 == "IsAbsoluteSet")
					{
						string str2 = null;
						bool flag = base.SessionState.Path.IsPSAbsolute(stringCollections[num], out str2);
						base.WriteObject(flag);
						goto Label1;
					}
					else if (str1 == "QualifierSet")
					{
						int num1 = stringCollections[num].IndexOf(":", StringComparison.CurrentCulture);
						if (num1 >= 0)
						{
							if (!base.SessionState.Path.IsProviderQualified(stringCollections[num]))
							{
								empty = stringCollections[num].Substring(0, num1 + 1);
								goto Label2;
							}
							else
							{
								empty = stringCollections[num].Substring(0, num1 + 2);
								goto Label2;
							}
						}
						else
						{
							FormatException formatException = new FormatException(StringUtil.Format(NavigationResources.ParsePathFormatError, stringCollections[num]));
							base.WriteError(new ErrorRecord(formatException, "ParsePathFormatError", ErrorCategory.InvalidArgument, stringCollections[num]));
							goto Label1;
						}
					}
					else if (str1 == "ParentSet" || str1 == "LiteralPathSet")
					{
						if (!stringCollections[num].StartsWith("\\", StringComparison.CurrentCulture))
						{
							stringCollections[num].StartsWith("/", StringComparison.CurrentCulture);
						}
						try
						{
							empty = base.SessionState.Path.ParseParent(stringCollections[num], string.Empty, this.CmdletProviderContext, true);
							goto Label2;
						}
						catch (PSNotSupportedException pSNotSupportedException2)
						{
							empty = string.Empty;
							goto Label2;
						}
					}
					else if (str1 == "LeafSet")
					{
						try
						{
							empty = base.SessionState.Path.ParseChildName(stringCollections[num], this.CmdletProviderContext, true);
							goto Label2;
						}
						catch (PSNotSupportedException pSNotSupportedException3)
						{
							empty = stringCollections[num];
							goto Label2;
						}
						catch (DriveNotFoundException driveNotFoundException3)
						{
							DriveNotFoundException driveNotFoundException2 = driveNotFoundException3;
							base.WriteError(new ErrorRecord(driveNotFoundException2.ErrorRecord, driveNotFoundException2));
							goto Label1;
						}
						catch (ProviderNotFoundException providerNotFoundException3)
						{
							ProviderNotFoundException providerNotFoundException2 = providerNotFoundException3;
							base.WriteError(new ErrorRecord(providerNotFoundException2.ErrorRecord, providerNotFoundException2));
							goto Label1;
						}
					}
					else if (str1 == "NoQualifierSet")
					{
						empty = this.RemoveQualifier(stringCollections[num]);
						goto Label2;
					}
				}
			Label2:
				if (empty != null)
				{
					base.WriteObject(empty);
				}
			Label1:
				num++;
			}
			return;
		Label0:
			IEnumerator<PathInfo> enumerator = resolvedPSPathFromPSPath.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					PathInfo current = enumerator.Current;
					try
					{
						if (base.InvokeProvider.Item.Exists(current.Path, cmdletProviderContext))
						{
							stringCollections.Add(current.Path);
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
					catch (ItemNotFoundException itemNotFoundException3)
					{
						ItemNotFoundException itemNotFoundException2 = itemNotFoundException3;
						base.WriteError(new ErrorRecord(itemNotFoundException2.ErrorRecord, itemNotFoundException2));
					}
				}
				goto Label3;
			}
		}

		private string RemoveQualifier(string path)
		{
			string str = path;
			if (!base.SessionState.Path.IsProviderQualified(path))
			{
				string empty = string.Empty;
				if (base.SessionState.Path.IsPSAbsolute(path, out empty))
				{
					str = path.Substring(empty.Length + 1);
				}
			}
			else
			{
				int num = path.IndexOf("::", StringComparison.CurrentCulture);
				if (num != -1)
				{
					str = path.Substring(num + 2);
				}
			}
			object[] objArray = new object[1];
			objArray[0] = str;
			CoreCommandBase.tracer.WriteLine("result = {0}", objArray);
			return str;
		}
	}
}