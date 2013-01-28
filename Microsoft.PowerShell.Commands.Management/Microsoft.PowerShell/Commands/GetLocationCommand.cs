using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "Location", DefaultParameterSetName="Location", SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113321")]
	[OutputType(new Type[] { typeof(PathInfo) }, ParameterSetName=new string[] { "locationSet" })]
	[OutputType(new Type[] { typeof(PathInfoStack) }, ParameterSetName=new string[] { "Stack" })]
	public class GetLocationCommand : DriveMatchingCoreCommandBase
	{
		private const string locationSet = "Location";

		private const string stackSet = "Stack";

		private bool stackSwitch;

		private string[] provider;

		private string[] drives;

		private string[] stackNames;

		[Parameter(ParameterSetName="Location", ValueFromPipelineByPropertyName=true)]
		public string[] PSDrive
		{
			get
			{
				return this.drives;
			}
			set
			{
				this.drives = value;
			}
		}

		[Parameter(ParameterSetName="Location", ValueFromPipelineByPropertyName=true)]
		public string[] PSProvider
		{
			get
			{
				return this.provider;
			}
			set
			{
				if (value != null)
				{
					this.provider = value;
					return;
				}
				else
				{
					this.provider = new string[0];
					return;
				}
			}
		}

		[Parameter(ParameterSetName="Stack")]
		public SwitchParameter Stack
		{
			get
			{
				return this.stackSwitch;
			}
			set
			{
				this.stackSwitch = value;
			}
		}

		[Parameter(ParameterSetName="Stack", ValueFromPipelineByPropertyName=true)]
		public string[] StackName
		{
			get
			{
				return this.stackNames;
			}
			set
			{
				this.stackNames = value;
			}
		}

		public GetLocationCommand()
		{
			this.provider = new string[0];
		}

		protected override void ProcessRecord()
		{
			PathInfo pathInfo;
			List<PSDriveInfo> matchingDrives;
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "Location")
				{
					pathInfo = null;
					if (this.PSDrive == null || (int)this.PSDrive.Length <= 0)
					{
						if ((this.PSDrive == null || (int)this.PSDrive.Length == 0) && this.PSProvider != null && (int)this.PSProvider.Length > 0)
						{
							string[] pSProvider = this.PSProvider;
							for (int i = 0; i < (int)pSProvider.Length; i++)
							{
								string str1 = pSProvider[i];
								bool flag = WildcardPattern.ContainsWildcardCharacters(str1);
								if (!flag)
								{
									try
									{
										base.SessionState.Provider.GetOne(str1);
									}
									catch (ProviderNotFoundException providerNotFoundException1)
									{
										ProviderNotFoundException providerNotFoundException = providerNotFoundException1;
										ErrorRecord errorRecord = new ErrorRecord(providerNotFoundException, "GetLocationNoMatchingProvider", ErrorCategory.ObjectNotFound, str1);
										base.WriteError(errorRecord);
										goto Label0;
									}
								}
								foreach (ProviderInfo all in base.SessionState.Provider.GetAll())
								{
									if (!all.IsMatch(str1))
									{
										continue;
									}
									try
									{
										base.WriteObject(base.SessionState.Path.CurrentProviderLocation(all.FullName));
									}
									catch (ProviderNotFoundException providerNotFoundException3)
									{
										ProviderNotFoundException providerNotFoundException2 = providerNotFoundException3;
										base.WriteError(new ErrorRecord(providerNotFoundException2.ErrorRecord, providerNotFoundException2));
									}
									catch (DriveNotFoundException driveNotFoundException1)
									{
										DriveNotFoundException driveNotFoundException = driveNotFoundException1;
										if (!flag)
										{
											base.WriteError(new ErrorRecord(driveNotFoundException.ErrorRecord, driveNotFoundException));
										}
									}
								}
                            Label0:
                                continue;
							}
							return;
						}
						else
						{
							base.WriteObject(base.SessionState.Path.CurrentLocation);
							return;
						}
					}
					else
					{
						string[] pSDrive = this.PSDrive;
						for (int j = 0; j < (int)pSDrive.Length; j++)
						{
							string str2 = pSDrive[j];
							matchingDrives = null;
							try
							{
								matchingDrives = base.GetMatchingDrives(str2, this.PSProvider, null);
								goto Label1;
							}
							catch (DriveNotFoundException driveNotFoundException3)
							{
								DriveNotFoundException driveNotFoundException2 = driveNotFoundException3;
								ErrorRecord errorRecord1 = new ErrorRecord(driveNotFoundException2, "GetLocationNoMatchingDrive", ErrorCategory.ObjectNotFound, str2);
								base.WriteError(errorRecord1);
							}
							catch (ProviderNotFoundException providerNotFoundException5)
							{
								ProviderNotFoundException providerNotFoundException4 = providerNotFoundException5;
								ErrorRecord errorRecord2 = new ErrorRecord(providerNotFoundException4, "GetLocationNoMatchingProvider", ErrorCategory.ObjectNotFound, this.PSProvider);
								base.WriteError(errorRecord2);
							}
							catch (ArgumentException argumentException1)
							{
								ArgumentException argumentException = argumentException1;
								ErrorRecord errorRecord3 = new ErrorRecord(argumentException, "GetLocationNoMatchingDrive", ErrorCategory.ObjectNotFound, str2);
								base.WriteError(errorRecord3);
							}
						}
						return;
					}
				}
				else
				{
					if (str == "Stack")
					{
						if (this.stackNames == null)
						{
							try
							{
								base.WriteObject(base.SessionState.Path.LocationStack(null), false);
							}
							catch (PSArgumentException pSArgumentException1)
							{
								PSArgumentException pSArgumentException = pSArgumentException1;
								base.WriteError(new ErrorRecord(pSArgumentException.ErrorRecord, pSArgumentException));
							}
						}
						else
						{
							string[] strArrays = this.stackNames;
							for (int k = 0; k < (int)strArrays.Length; k++)
							{
								string str3 = strArrays[k];
								try
								{
									base.WriteObject(base.SessionState.Path.LocationStack(str3), false);
								}
								catch (PSArgumentException pSArgumentException3)
								{
									PSArgumentException pSArgumentException2 = pSArgumentException3;
									base.WriteError(new ErrorRecord(pSArgumentException2.ErrorRecord, pSArgumentException2));
								}
							}
							return;
						}
					}
					else
					{
						return;
					}
				}
			}
			return;
		Label1:
			List<PSDriveInfo>.Enumerator enumerator = matchingDrives.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					PSDriveInfo current = enumerator.Current;
					try
					{
						string driveQualifiedPath = LocationGlobber.GetDriveQualifiedPath(current.CurrentLocation, current);
						pathInfo = new PathInfo(current, current.Provider, driveQualifiedPath, base.SessionState);
						base.WriteObject(pathInfo);
					}
					catch (ProviderNotFoundException providerNotFoundException7)
					{
						ProviderNotFoundException providerNotFoundException6 = providerNotFoundException7;
						base.WriteError(new ErrorRecord(providerNotFoundException6.ErrorRecord, providerNotFoundException6));
					}
				}
                return;
			}
			finally
			{
				enumerator.Dispose();
			}
		}
	}
}