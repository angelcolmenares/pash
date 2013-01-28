using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADNewCmdletBase<P, F, O> : ADCmdletBase<P>, IDynamicParameters, IADErrorTarget
	where P : ADParameterSet, new()
	where F : ADFactory<O>, new()
	where O : ADObject, new()
	{
		protected P _defaultParamSet;

		private F _factory;

		private string _currentIdentity;

		public ADNewCmdletBase()
		{
			this._defaultParamSet = Activator.CreateInstance<P>();
			this._factory = Activator.CreateInstance<F>();
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWritableDCRequiredCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.ADNewCmdletBaseProcessCSRoutine));
		}

		private bool ADNewCmdletBaseProcessCSRoutine()
		{
			bool flag;
			this.ValidateParameters();
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			string item = this._cmdletParameters["Path"] as string;
			if (string.IsNullOrEmpty(item))
			{
				item = this.GetDefaultCreationPath();
				if (string.IsNullOrEmpty(item))
				{
					object[] objArray = new object[1];
					objArray[0] = "Path";
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
				}
			}
			string str = this.GenerateObjectClass(this._factory, this._cmdletParameters, this._cmdletParameters);
			string str1 = this.GenerateDN(this._factory, this._cmdletParameters, this._cmdletParameters, item);
			this._currentIdentity = str1;
			ADObject aDObject = new ADObject(str1, str);
			using (ADActiveObject aDActiveObject = new ADActiveObject(cmdletSessionInfo.ADSessionInfo, aDObject))
			{
				if (base.ShouldProcessOverride(aDObject.DistinguishedName, "New"))
				{
					this._factory.UpdateFromParameters(this._defaultParamSet, null, null, null, null, aDObject);
					if (this._cmdletParameters.Contains("Instance"))
					{
						this._factory.UpdateFromTemplate((O)(this._cmdletParameters["Instance"] as O), aDObject);
					}
					this._factory.UpdateFromParameters(this._cmdletParameters, null, this._cmdletParameters["OtherAttributes"] as Hashtable, null, null, aDObject);
					this._factory.PreCommitProcesing(ADFactory<O>.DirectoryOperation.Create, (O)(this._cmdletParameters["Instance"] as O), this._cmdletParameters, aDObject);
					aDActiveObject.Create();
					aDObject.TrackChanges = true;
					bool flag1 = this._factory.PostCommitProcesing(ADFactory<O>.DirectoryOperation.Create, (O)(this._cmdletParameters["Instance"] as O), this._cmdletParameters, aDObject);
					if (flag1)
					{
						aDActiveObject.Update();
					}
					if (this._cmdletParameters.GetSwitchParameterBooleanValue("PassThru"))
					{
						O extendedObjectFromDN = this._factory.GetExtendedObjectFromDN(aDObject.DistinguishedName);
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

		protected internal virtual string GenerateDN(F factory, ADParameterSet cmdletParameters, P dynamicParameters, string path)
		{
			string str = this.GenerateRDN(factory, cmdletParameters, dynamicParameters);
			return string.Concat(str, ",", path);
		}

		protected internal virtual string GenerateObjectClass(F factory, ADParameterSet cmdletParameters, P dynamicParameters)
		{
			return factory.StructuralObjectClass;
		}

		protected internal virtual string GenerateRDN(F factory, ADParameterSet cmdletParameters, P dynamicParameters)
		{
			string str = this.GenerateRDNPrefix(factory, cmdletParameters, dynamicParameters);
			string str1 = factory.GenerateObjectName(dynamicParameters);
			if (str1 != null)
			{
				str1 = Utils.EscapeDNComponent(str1);
			}
			return string.Concat(str, "=", str1);
		}

		protected internal virtual string GenerateRDNPrefix(F factory, ADParameterSet cmdletParameters, P dynamicParameters)
		{
			return factory.RDNPrefix;
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			return this._currentIdentity;
		}

		protected internal virtual void ValidateParameters()
		{
		}
	}
}