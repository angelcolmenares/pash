using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADRemoveCmdletBase<P, F, O> : ADCmdletBase<P>, IDynamicParameters, IADErrorTarget
	where P : ADParameterSet, new()
	where F : ADFactory<O>, new()
	where O : ADObject, new()
	{
		private F _factory;

		private bool _showDeleted;

		public ADRemoveCmdletBase()
		{
			this._factory = Activator.CreateInstance<F>();
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWritableDCRequiredCSRoutine));
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.ADRemoveCmdletBaseBeginCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADRemoveCmdletBaseProcessCSRoutine));
		}

		private bool ADRemoveCmdletBaseBeginCSRoutine()
		{
			this._showDeleted = this._cmdletParameters.GetSwitchParameterBooleanValue("IncludeDeletedObjects");
			return true;
		}

		private bool ADRemoveCmdletBaseProcessCSRoutine()
		{
			bool flag;
			bool hasValue;
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
			if (this._showDeleted)
			{
				hasValue = true;
			}
			else
			{
				if (!item.Contains("Deleted"))
				{
					hasValue = false;
				}
				else
				{
					bool? value = (bool?)(item.GetValue("Deleted") as bool?);
					if (!value.GetValueOrDefault())
					{
						hasValue = false;
					}
					else
					{
						hasValue = value.HasValue;
					}
				}
			}
			bool flag1 = hasValue;
			ADObject directoryObjectFromIdentity = this._factory.GetDirectoryObjectFromIdentity(item, defaultPartitionPath, flag1);
			using (ADActiveObject aDActiveObject = new ADActiveObject(cmdletSessionInfo.ADSessionInfo, directoryObjectFromIdentity))
			{
				bool flag2 = this._cmdletParameters.Contains("Recursive");
				if (!flag2)
				{
					if (!base.ShouldProcessOverride(directoryObjectFromIdentity.DistinguishedName, "Remove"))
					{
						flag = false;
						return flag;
					}
				}
				else
				{
					object[] distinguishedName = new object[1];
					distinguishedName[0] = directoryObjectFromIdentity.DistinguishedName;
					string str = string.Format(CultureInfo.CurrentCulture, StringResources.PerformingRecursiveRemove, distinguishedName);
					if (!base.ShouldProcessOverride(str, str, StringResources.PromptForRecursiveRemove))
					{
						flag = false;
						return flag;
					}
				}
				O o = default(O);
				if (this._factory.PreCommitProcesing(ADFactory<O>.DirectoryOperation.Delete, o, this._cmdletParameters, directoryObjectFromIdentity))
				{
					aDActiveObject.Update();
				}
				if (!flag2)
				{
					aDActiveObject.Delete(flag1);
				}
				else
				{
					aDActiveObject.DeleteTree(flag1);
				}
				O o1 = default(O);
				this._factory.PostCommitProcesing(ADFactory<O>.DirectoryOperation.Delete, o1, this._cmdletParameters, directoryObjectFromIdentity);
				return true;
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