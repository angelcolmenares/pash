using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADSetCmdletBase<P, F, O> : ADCmdletBase<P>, IDynamicParameters, IADErrorTarget
	where P : ADParameterSet, new()
	where F : ADFactory<O>, new()
	where O : ADEntity, new()
	{
		internal F _factory;

		private bool _setProcessed;

		public ADSetCmdletBase()
		{
			this._factory = Activator.CreateInstance<F>();
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWritableDCRequiredCSRoutine));
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.ADSetCmdletBaseBeginCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADSetCmdletBaseProcessCSRoutine));
		}

		protected bool ADSetCmdletBaseBeginCSRoutine()
		{
			bool flag;
			if (this._cmdletParameters.Contains("Instance"))
			{
				O item = (O)(this._cmdletParameters["Instance"] as O);
				this.SetPipelinedSessionInfo(item.SessionInfo);
				CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
				this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
				this.ValidateParameters();
				if (item.IsSearchResult)
				{
					if (item.GetType() == typeof(O))
					{
						this._factory.ValidateObjectClass(item);
						ADObject directoryObjectFromIdentity = this._factory.GetDirectoryObjectFromIdentity(item, cmdletSessionInfo.DefaultPartitionPath);
						using (ADActiveObject aDActiveObject = new ADActiveObject(base.GetSessionInfo(), directoryObjectFromIdentity))
						{
							if (base.ShouldProcessOverride(item.IdentifyingString, "Set"))
							{
								this._factory.UpdateFromObject(item, directoryObjectFromIdentity);
								this._factory.PreCommitProcesing(ADFactory<O>.DirectoryOperation.Update, item, this._cmdletParameters, directoryObjectFromIdentity);
								aDActiveObject.Update();
								if (this._factory.PostCommitProcesing(ADFactory<O>.DirectoryOperation.Update, item, this._cmdletParameters, directoryObjectFromIdentity))
								{
									aDActiveObject.Update();
								}
								if (this._cmdletParameters.Contains("PassThru"))
								{
									O extendedObjectFromDN = this._factory.GetExtendedObjectFromDN(directoryObjectFromIdentity.DistinguishedName);
									if (extendedObjectFromDN != null)
									{
										base.WriteObject(extendedObjectFromDN);
									}
								}
								this._setProcessed = true;
								return true;
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
						object[] str = new object[1];
						str[0] = typeof(O).ToString();
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InstanceMustBeOfType, str));
					}
				}
				else
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.OnlySearchResultsSupported, new object[0]));
				}
			}
			return true;
		}

		protected bool ADSetCmdletBaseProcessCSRoutine()
		{
			if (!this._setProcessed)
			{
				if (this._cmdletParameters.Contains("Identity"))
				{
					O item = (O)(this._cmdletParameters["Identity"] as O);
					this.SetPipelinedSessionInfo(item.SessionInfo);
					CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
					this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
					this.ValidateParameters();
					this.SetFromIdentity(item);
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "Identity,Instance";
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequiredMultiple, objArray));
				}
			}
			return true;
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

		protected internal virtual bool SetFromIdentity(O identity)
		{
			bool flag;
			ADSessionInfo sessionInfo = base.GetSessionInfo();
			string item = this._cmdletParameters["Partition"] as string;
			if (item == null)
			{
				item = this.GetDefaultPartitionPath();
				if (item == null && !identity.IsSearchResult)
				{
					object[] objArray = new object[1];
					objArray[0] = "Partition";
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
				}
			}
			ADObject directoryObjectFromIdentity = this._factory.GetDirectoryObjectFromIdentity(identity, item);
			using (ADActiveObject aDActiveObject = new ADActiveObject(sessionInfo, directoryObjectFromIdentity))
			{
				if (base.ShouldProcessOverride(directoryObjectFromIdentity.IdentifyingString, "Set"))
				{
					this._factory.UpdateFromParameters(this._cmdletParameters, this._cmdletParameters["Add"] as Hashtable, this._cmdletParameters["Replace"] as Hashtable, this._cmdletParameters["Remove"] as Hashtable, this._cmdletParameters["Clear"] as string[], directoryObjectFromIdentity);
					O o = default(O);
					this._factory.PreCommitProcesing(ADFactory<O>.DirectoryOperation.Update, o, this._cmdletParameters, directoryObjectFromIdentity);
					aDActiveObject.Update();
					directoryObjectFromIdentity.TrackChanges = true;
					O o1 = default(O);
					if (this._factory.PostCommitProcesing(ADFactory<O>.DirectoryOperation.Update, o1, this._cmdletParameters, directoryObjectFromIdentity))
					{
						aDActiveObject.Update();
					}
					if (this._cmdletParameters.GetSwitchParameterBooleanValue("PassThru"))
					{
						O extendedObjectFromDN = this._factory.GetExtendedObjectFromDN(directoryObjectFromIdentity.DistinguishedName);
						base.WriteObject(extendedObjectFromDN);
					}
					return true;
				}
				else
				{
					flag = true;
				}
			}
			return flag;
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