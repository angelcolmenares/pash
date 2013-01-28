using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADMoveCmdletBase<P, F, O> : ADCmdletBase<P>, IDynamicParameters, IADErrorTarget
	where P : ADParameterSet, new()
	where F : ADFactory<O>, new()
	where O : ADObject, new()
	{
		private F _factory;

		public ADMoveCmdletBase()
		{
			this._factory = Activator.CreateInstance<F>();
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWritableDCRequiredCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADMoveCmdletBaseProcessCSRoutine));
		}

		private bool ADMoveCmdletBaseProcessCSRoutine()
		{
			bool flag;
			O item = (O)this._cmdletParameters["Identity"];
			this.SetPipelinedSessionInfo(item.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			this.ValidateParameters();
			string defaultPartitionPath = this.GetDefaultPartitionPath();
			ADObject directoryObjectFromIdentity = this._factory.GetDirectoryObjectFromIdentity(item, defaultPartitionPath);
			using (ADActiveObject aDActiveObject = new ADActiveObject(cmdletSessionInfo.ADSessionInfo, directoryObjectFromIdentity))
			{
				if (base.ShouldProcessOverride(directoryObjectFromIdentity.DistinguishedName, "Move"))
				{
					O o = default(O);
					if (this._factory.PreCommitProcesing(ADFactory<O>.DirectoryOperation.Move, o, this._cmdletParameters, directoryObjectFromIdentity))
					{
						aDActiveObject.Update();
					}
					string str = this._cmdletParameters["TargetPath"] as string;
					string childName = ADPathModule.GetChildName(directoryObjectFromIdentity.DistinguishedName, ADPathFormat.X500);
					string item1 = null;
					if (!this._cmdletParameters.Contains("TargetServer"))
					{
						aDActiveObject.Move(str, childName);
					}
					else
					{
						item1 = this._cmdletParameters["TargetServer"] as string;
						aDActiveObject.CrossDomainMove(str, childName, item1);
					}
					O o1 = default(O);
					this._factory.PostCommitProcesing(ADFactory<O>.DirectoryOperation.Move, o1, this._cmdletParameters, directoryObjectFromIdentity);
					if (this._cmdletParameters.GetSwitchParameterBooleanValue("PassThru"))
					{
						ADSessionInfo aDSessionInfo = cmdletSessionInfo.ADSessionInfo;
						if (this._cmdletParameters.Contains("TargetServer"))
						{
							aDSessionInfo.Server = item1;
						}
						string str1 = ADPathModule.MakePath(str, childName, ADPathFormat.X500);
						F f = Activator.CreateInstance<F>();
						using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(aDSessionInfo))
						{
							ADRootDSE rootDSE = aDObjectSearcher.GetRootDSE();
							ADCmdletCache aDCmdletCache = new ADCmdletCache();
							aDSessionInfo.ServerType = Utils.ADServerTypeFromRootDSE(rootDSE);
							CmdletSessionInfo cmdletSessionInfo1 = new CmdletSessionInfo(aDSessionInfo, rootDSE, rootDSE.DefaultNamingContext, rootDSE.DefaultNamingContext, rootDSE.DefaultNamingContext, aDSessionInfo.ServerType, aDCmdletCache, this, this, this._cmdletParameters);
							f.SetCmdletSessionInfo(cmdletSessionInfo1);
							O extendedObjectFromDN = this._factory.GetExtendedObjectFromDN(str1);
							base.WriteObject(extendedObjectFromDN);
							aDCmdletCache.Clear();
						}
					}
					return true;
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			if (this._cmdletParameters.Contains("Identity"))
			{
				return this._cmdletParameters["Identity"];
			}
			else
			{
				return null;
			}
		}

		protected internal virtual void ValidateParameters()
		{
			this.GetCmdletSessionInfo();
			if (!this._cmdletParameters.Contains("Identity") || !string.IsNullOrEmpty(this.GetDefaultPartitionPath()))
			{
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "Partition";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
			}
		}
	}
}