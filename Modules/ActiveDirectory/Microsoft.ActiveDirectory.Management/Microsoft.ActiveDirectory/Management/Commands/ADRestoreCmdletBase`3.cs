using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Security.Authentication;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADRestoreCmdletBase<P, F, O> : ADCmdletBase<P>, IDynamicParameters, IADErrorTarget
	where P : ADParameterSet, new()
	where F : ADFactory<O>, new()
	where O : ADObject, new()
	{
		private F _factory;

		public ADRestoreCmdletBase()
		{
			this._factory = Activator.CreateInstance<F>();
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWritableDCRequiredCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADRestoreCmdletBaseProcessCSRoutine));
		}

		private bool ADRestoreCmdletBaseProcessCSRoutine()
		{
			string str;
			string item;
			bool flag;
			string str1;
			string value;
			O o = (O)this._cmdletParameters["Identity"];
			this.SetPipelinedSessionInfo(o.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			string defaultPartitionPath = this.GetDefaultPartitionPath();
			string[] strArrays = new string[3];
			strArrays[0] = "objectClass";
			strArrays[1] = "lastKnownParent";
			strArrays[2] = "msDS-LastKnownRDN";
			string[] strArrays1 = strArrays;
			ADObject extendedObjectFromIdentity = this._factory.GetExtendedObjectFromIdentity(o, defaultPartitionPath, strArrays1, true);
			if (!this._cmdletParameters.Contains("NewName"))
			{
				if (extendedObjectFromIdentity["msDS-LastKnownRDN"] == null)
				{
					str1 = null;
				}
				else
				{
					str1 = this.GenerateNewRDN(this._factory, extendedObjectFromIdentity["msDS-LastKnownRDN"].Value as string, extendedObjectFromIdentity);
				}
				str = str1;
			}
			else
			{
				str = this.GenerateNewRDN(this._factory, this._cmdletParameters["NewName"] as string, extendedObjectFromIdentity);
			}
			if (string.IsNullOrEmpty(str))
			{
				object[] objArray = new object[1];
				objArray[0] = "NewName";
				base.WriteError(new ErrorRecord(new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray)), "0", ErrorCategory.InvalidArgument, extendedObjectFromIdentity));
			}
			if (!this._cmdletParameters.Contains("TargetPath"))
			{
				if (extendedObjectFromIdentity["lastKnownParent"] == null)
				{
					value = null;
				}
				else
				{
					value = extendedObjectFromIdentity["lastKnownParent"].Value as string;
				}
				item = value;
			}
			else
			{
				item = this._cmdletParameters["TargetPath"] as string;
			}
			if (string.IsNullOrEmpty(item))
			{
				object[] objArray1 = new object[1];
				objArray1[0] = "TargetPath";
				base.WriteError(new ErrorRecord(new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray1)), "0", ErrorCategory.InvalidArgument, extendedObjectFromIdentity));
			}
			string str2 = ADPathModule.MakePath(item, str, ADPathFormat.X500);
			using (ADActiveObject aDActiveObject = new ADActiveObject(cmdletSessionInfo.ADSessionInfo, extendedObjectFromIdentity))
			{
				if (base.ShouldProcessOverride(extendedObjectFromIdentity.DistinguishedName, "Restore"))
				{
					O o1 = default(O);
					this._factory.PreCommitProcesing(ADFactory<O>.DirectoryOperation.Restore, o1, this._cmdletParameters, extendedObjectFromIdentity);
					try
					{
						aDActiveObject.Undelete(str2);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						if (exception as ADException != null || exception as ADInvalidOperationException != null || exception as ADIdentityResolutionException != null || exception as UnauthorizedAccessException != null || exception as AuthenticationException != null || exception as ArgumentException != null)
						{
							base.WriteErrorBuffered(new ErrorRecord(exception, "0", ErrorCategory.InvalidOperation, extendedObjectFromIdentity));
							flag = false;
							return flag;
						}
						else
						{
							throw;
						}
					}
					O o2 = default(O);
					if (this._factory.PostCommitProcesing(ADFactory<O>.DirectoryOperation.Restore, o2, this._cmdletParameters, extendedObjectFromIdentity))
					{
						aDActiveObject.Update();
					}
					if (this._cmdletParameters.GetSwitchParameterBooleanValue("PassThru"))
					{
						O extendedObjectFromDN = this._factory.GetExtendedObjectFromDN(str2);
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

		protected internal virtual string GenerateNewRDN(F factory, string newName, ADObject target)
		{
			string empty = string.Empty;
			if (target.Contains("objectClass"))
			{
				ADSessionInfo sessionInfo = this.GetSessionInfo();
				ADSchemaUtil aDSchemaUtil = new ADSchemaUtil(sessionInfo);
				empty = aDSchemaUtil.GetRDNPrefix(target["objectClass"].Value as string);
			}
			if (!string.IsNullOrEmpty(newName))
			{
				newName = Utils.EscapeDNComponent(newName);
			}
			if (!string.IsNullOrEmpty(empty))
			{
				empty = string.Concat(empty, "=", newName);
			}
			return empty;
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