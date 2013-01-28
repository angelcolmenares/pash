using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "PSDrive", DefaultParameterSetName="Name", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113327")]
	[OutputType(new Type[] { typeof(PSDriveInfo) })]
	public class GetPSDriveCommand : DriveMatchingCoreCommandBase
	{
		private string[] name;

		private string[] provider;

		private string scope;

		[Parameter(Position=0, ParameterSetName="LiteralName", Mandatory=true, ValueFromPipeline=false, ValueFromPipelineByPropertyName=true)]
		public string[] LiteralName
		{
			get
			{
				return this.name;
			}
			set
			{
				base.SuppressWildcardExpansion = true;
				this.name = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="Name", ValueFromPipelineByPropertyName=true)]
		public string[] Name
		{
			get
			{
				return this.name;
			}
			set
			{
				if (value == null)
				{
					string[] strArrays = new string[1];
					strArrays[0] = "*";
					value = strArrays;
				}
				this.name = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string[] PSProvider
		{
			get
			{
				return this.provider;
			}
			set
			{
				if (value == null)
				{
					value = new string[0];
				}
				this.provider = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Scope
		{
			get
			{
				return this.scope;
			}
			set
			{
				this.scope = value;
			}
		}

		public GetPSDriveCommand()
		{
			string[] strArrays = new string[1];
			strArrays[0] = "*";
			this.name = strArrays;
			this.provider = new string[0];
		}

		protected override void BeginProcessing()
		{
			SessionStateInternal.MountDefaultDrive("Cert", base.Context);
			SessionStateInternal.MountDefaultDrive("WSMan", base.Context);
		}

		protected override void ProcessRecord()
		{
			string[] name = this.Name;
			for (int i = 0; i < (int)name.Length; i++)
			{
				string str = name[i];
				try
				{
					List<PSDriveInfo> matchingDrives = base.GetMatchingDrives(str, this.PSProvider, this.Scope);
					if (matchingDrives.Count <= 0)
					{
						if (!WildcardPattern.ContainsWildcardCharacters(str))
						{
							DriveNotFoundException driveNotFoundException = new DriveNotFoundException(str, "DriveNotFound", SessionStateStrings.DriveNotFound);
							base.WriteError(new ErrorRecord(driveNotFoundException, "GetDriveNoMatchingDrive", ErrorCategory.ObjectNotFound, str));
						}
					}
					else
					{
						base.WriteObject(matchingDrives, true);
					}
				}
				catch (DriveNotFoundException driveNotFoundException2)
				{
					DriveNotFoundException driveNotFoundException1 = driveNotFoundException2;
					ErrorRecord errorRecord = new ErrorRecord(driveNotFoundException1, "GetLocationNoMatchingDrive", ErrorCategory.ObjectNotFound, str);
					base.WriteError(errorRecord);
				}
				catch (ProviderNotFoundException providerNotFoundException1)
				{
					ProviderNotFoundException providerNotFoundException = providerNotFoundException1;
					ErrorRecord errorRecord1 = new ErrorRecord(providerNotFoundException, "GetLocationNoMatchingDrive", ErrorCategory.ObjectNotFound, this.PSProvider);
					base.WriteError(errorRecord1);
				}
				catch (PSArgumentOutOfRangeException pSArgumentOutOfRangeException1)
				{
					PSArgumentOutOfRangeException pSArgumentOutOfRangeException = pSArgumentOutOfRangeException1;
					base.WriteError(new ErrorRecord(pSArgumentOutOfRangeException.ErrorRecord, pSArgumentOutOfRangeException));
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