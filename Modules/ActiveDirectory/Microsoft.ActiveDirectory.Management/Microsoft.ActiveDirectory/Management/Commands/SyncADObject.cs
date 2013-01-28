using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Sync", "ADObject", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216426")]
	public class SyncADObject : ADCmdletBase<SyncADObjectParameterSet>, IADErrorTarget
	{
		private const string _debugCategory = "SyncADObject";

		private ADObjectFactory<ADObject> _adObjectFactory;

		private string currentADDriveServer;

		private string sourceServer;

		public SyncADObject()
		{
			this._adObjectFactory = new ADObjectFactory<ADObject>();
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(this.SyncADObjectBeginCSRoutine));
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.SyncADObjectProcessCSRoutine));
		}

		internal override ADSessionInfo GetSessionInfo()
		{
			ADSessionInfo destination = base.GetSessionInfo().Copy();
			destination.Server = this._cmdletParameters.Destination;
			return destination;
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			if (this._cmdletParameters.Contains("Object"))
			{
				return this._cmdletParameters["Object"];
			}
			else
			{
				return null;
			}
		}

		private bool SyncADObjectBeginCSRoutine()
		{
			if (ProviderUtils.IsCurrentDriveAD(base.SessionState))
			{
				this.currentADDriveServer = ((ADDriveInfo)base.SessionState.Drive.Current).Server;
			}
			if (this._cmdletParameters.Contains("Source"))
			{
				this.sourceServer = this._cmdletParameters["Source"] as string;
				if (base.DoesServerNameRepresentDomainName(this.sourceServer))
				{
					object[] objArray = new object[1];
					objArray[0] = this.sourceServer;
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SourceDoesNotTargetDirectoryServer, objArray));
				}
			}
			return true;
		}

		private bool SyncADObjectProcessCSRoutine()
		{
			CmdletSessionInfo cmdletSessionInfo;
			ADRootDSE rootDSE;
			ADObject directoryObjectFromIdentity;
			bool flag;
			string empty;
			this.ValidateParameters();
			ADObject obj = this._cmdletParameters.Object;
			string destination = this._cmdletParameters.Destination;
			if (this.sourceServer == null)
			{
				if (!obj.IsSearchResult)
				{
					if (this.currentADDriveServer == null)
					{
						object[] objArray = new object[1];
						objArray[0] = "Source";
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
					}
					else
					{
						this.sourceServer = this.currentADDriveServer;
					}
				}
				else
				{
					this.sourceServer = obj.SessionInfo.Server;
				}
			}
			try
			{
				cmdletSessionInfo = this.GetCmdletSessionInfo();
			}
			catch (ADServerDownException aDServerDownException1)
			{
				ADServerDownException aDServerDownException = aDServerDownException1;
				object[] objArray1 = new object[1];
				objArray1[0] = destination;
				throw new ADServerDownException(string.Format(CultureInfo.CurrentCulture, StringResources.DestinationServerDown, objArray1), aDServerDownException.InnerException, destination);
			}
			if (!cmdletSessionInfo.ADRootDSE.IsWritable() || !this._cmdletParameters.GetSwitchParameterBooleanValue("PasswordOnly"))
			{
				string dSServiceName = null;
				ADSessionInfo aDSessionInfo = cmdletSessionInfo.ADSessionInfo.Copy();
				aDSessionInfo.Server = this.sourceServer;
				string distinguishedName = null;
				using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(aDSessionInfo))
				{
					try
					{
						rootDSE = aDObjectSearcher.GetRootDSE();
						dSServiceName = rootDSE.DSServiceName;
					}
					catch (ADIdentityNotFoundException aDIdentityNotFoundException)
					{
						object[] objArray2 = new object[1];
						objArray2[0] = this.sourceServer;
						throw new ADServerDownException(string.Format(CultureInfo.CurrentCulture, StringResources.SourceServerDown, objArray2), this.sourceServer);
					}
					if (!obj.IsSearchResult)
					{
						ADCmdletCache aDCmdletCache = new ADCmdletCache();
						aDSessionInfo.ServerType = Utils.ADServerTypeFromRootDSE(rootDSE);
						CmdletSessionInfo cmdletSessionInfo1 = new CmdletSessionInfo(aDSessionInfo, rootDSE, rootDSE.DefaultNamingContext, rootDSE.DefaultNamingContext, rootDSE.DefaultNamingContext, aDSessionInfo.ServerType, aDCmdletCache, this, null, this._cmdletParameters);
						this._adObjectFactory.SetCmdletSessionInfo(cmdletSessionInfo1);
						try
						{
							directoryObjectFromIdentity = this._adObjectFactory.GetDirectoryObjectFromIdentity(obj, cmdletSessionInfo1.DefaultPartitionPath);
							distinguishedName = directoryObjectFromIdentity.DistinguishedName;
						}
						catch (ADIdentityNotFoundException aDIdentityNotFoundException2)
						{
							this._adObjectFactory.SetCmdletSessionInfo(cmdletSessionInfo);
							try
							{
								directoryObjectFromIdentity = this._adObjectFactory.GetDirectoryObjectFromIdentity(obj, cmdletSessionInfo.DefaultPartitionPath);
								Guid? objectGuid = directoryObjectFromIdentity.ObjectGuid;
								distinguishedName = string.Concat("<GUID=", objectGuid.ToString(), ">");
							}
							catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
							{
								object[] str = new object[1];
								str[0] = obj.ToString();
								throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ObjectToReplicateNotFoundOnSource, str));
							}
						}
						aDCmdletCache.Clear();
					}
					else
					{
						distinguishedName = obj.DistinguishedName;
					}
				}
				ADObject aDObject = new ADObject();
				aDObject.DistinguishedName = "";
				string str1 = string.Concat(dSServiceName, ":", distinguishedName);
				if (this._cmdletParameters.GetSwitchParameterBooleanValue("PasswordOnly"))
				{
					str1 = string.Concat(str1, ":SECRETS_ONLY");
				}
				aDObject.Add("replicateSingleObject", str1);
				aDObject.TrackChanges = false;
				using (ADActiveObject aDActiveObject = new ADActiveObject(cmdletSessionInfo.ADSessionInfo, aDObject))
				{
					if (base.ShouldProcessOverride(obj.IdentifyingString, "Sync"))
					{
						try
						{
							aDActiveObject.Update();
						}
						catch (ADIdentityNotFoundException aDIdentityNotFoundException3)
						{
							object[] objArray3 = new object[2];
							objArray3[0] = this.sourceServer;
							objArray3[1] = destination;
							throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.SourceServerObjNotFoundOrObjToReplicateNotFound, objArray3));
						}
						catch (ArgumentException argumentException1)
						{
							ArgumentException argumentException = argumentException1;
							Win32Exception win32Exception = new Win32Exception(0x200a);
							if (string.Compare(win32Exception.Message, 0, argumentException.Message, 0, win32Exception.Message.Length, StringComparison.OrdinalIgnoreCase) != 0 || string.Compare("replicateSingleObject", argumentException.ParamName, StringComparison.OrdinalIgnoreCase) != 0)
							{
								throw argumentException;
							}
							else
							{
								object[] objArray4 = new object[1];
								objArray4[0] = destination;
								throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.DestinationServerDoesNotSupportSynchronizingObject, objArray4));
							}
						}
						if (this._cmdletParameters.GetSwitchParameterBooleanValue("PassThru"))
						{
							this._adObjectFactory.SetCmdletSessionInfo(cmdletSessionInfo);
							ADObject extendedObjectFromDN = this._adObjectFactory.GetExtendedObjectFromDN(distinguishedName);
							base.WriteObject(extendedObjectFromDN);
						}
						return false;
					}
					else
					{
						flag = false;
					}
				}
				return flag;
			}
			else
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				string passwordOnlySwitchAllowedOnlyOnRODC = StringResources.PasswordOnlySwitchAllowedOnlyOnRODC;
				object[] objArray5 = new object[1];
				object[] objArray6 = objArray5;
				int num = 0;
				string dNSHostName = cmdletSessionInfo.ADRootDSE.DNSHostName;
				int? portLDAP = cmdletSessionInfo.ADRootDSE.PortLDAP;
				if (!portLDAP.HasValue)
				{
					empty = string.Empty;
				}
				else
				{
					int? nullable = cmdletSessionInfo.ADRootDSE.PortLDAP;
					empty = string.Concat(":", nullable.ToString());
				}
				objArray6[num] = string.Concat(dNSHostName, empty);
				throw new ArgumentException(string.Format(currentCulture, passwordOnlySwitchAllowedOnlyOnRODC, objArray5));
			}
		}

		protected internal void ValidateParameters()
		{
			string item = this._cmdletParameters["Destination"] as string;
			if (!base.DoesServerNameRepresentDomainName(item))
			{
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = item;
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.DestinationDoesNotTargetDirectoryServer, objArray));
			}
		}
	}
}