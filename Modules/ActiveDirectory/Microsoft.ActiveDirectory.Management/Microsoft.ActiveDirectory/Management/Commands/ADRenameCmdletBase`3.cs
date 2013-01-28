using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADRenameCmdletBase<P, F, O> : ADCmdletBase<P>, IDynamicParameters, IADErrorTarget
	where P : ADParameterSet, new()
	where F : ADFactory<O>, new()
	where O : ADObject, new()
	{
		private F _factory;

		public ADRenameCmdletBase()
		{
			this._factory = Activator.CreateInstance<F>();
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWritableDCRequiredCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADRenameCmdletBaseProcessCSRoutine));
		}

		private bool ADRenameCmdletBaseProcessCSRoutine()
		{
			bool flag;
			O item = (O)this._cmdletParameters["Identity"];
			this.SetPipelinedSessionInfo(item.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			this.ValidateParameters();
			string defaultPartitionPath = this._cmdletParameters["Partition"] as string;
			if (defaultPartitionPath == null)
			{
				defaultPartitionPath = this.GetDefaultPartitionPath();
				if (defaultPartitionPath == null && !item.IsSearchResult)
				{
					object[] objArray = new object[1];
					objArray[0] = "Partition";
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
				}
			}
			ADObject directoryObjectFromIdentity = this._factory.GetDirectoryObjectFromIdentity(item, defaultPartitionPath);
			using (ADActiveObject aDActiveObject = new ADActiveObject(cmdletSessionInfo.ADSessionInfo, directoryObjectFromIdentity))
			{
				string str = this.GenerateNewRDN(this._factory, this._cmdletParameters, this._cmdletParameters, directoryObjectFromIdentity.DistinguishedName);
				if (base.ShouldProcessOverride(directoryObjectFromIdentity.DistinguishedName, "Rename"))
				{
					O o = default(O);
					if (this._factory.PreCommitProcesing(ADFactory<O>.DirectoryOperation.Rename, o, this._cmdletParameters, directoryObjectFromIdentity))
					{
						aDActiveObject.Update();
					}
					aDActiveObject.Rename(str);
					O o1 = default(O);
					this._factory.PostCommitProcesing(ADFactory<O>.DirectoryOperation.Rename, o1, this._cmdletParameters, directoryObjectFromIdentity);
					if (this._cmdletParameters.GetSwitchParameterBooleanValue("PassThru"))
					{
						string value = directoryObjectFromIdentity["distinguishedName"].Value as string;
						string parentPath = ADPathModule.GetParentPath(value, null, ADPathFormat.X500);
						string str1 = ADPathModule.MakePath(parentPath, str, ADPathFormat.X500);
						O extendedObjectFromDN = this._factory.GetExtendedObjectFromDN(str1);
						base.WriteObject(extendedObjectFromDN);
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

		protected internal virtual string GenerateNewRDN(F factory, ADParameterSet cmdletParameters, P dynamicParameters, string oldDN)
		{
			string str = this.GenerateRDNPrefix(factory, cmdletParameters, dynamicParameters, oldDN);
			string item = cmdletParameters["NewName"] as string;
			if (item != null)
			{
				item = Utils.EscapeDNComponent(item);
			}
			return string.Concat(str, "=", item);
		}

		protected internal virtual string GenerateRDNPrefix(F factory, ADParameterSet cmdletParameters, P dynamicParameters, string oldDN)
		{
			return factory.RDNPrefix;
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