using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Cim;
using Microsoft.PowerShell.Cmdletization;
using System;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal abstract class PropertySettingJob<T> : MethodInvocationJobBase<T>
	{
		private readonly CimInstance _objectToModify;

		private bool _localCopyHasBeenModified;

		internal virtual CimInstance ObjectToModify
		{
			get
			{
				if (!this._localCopyHasBeenModified)
				{
					this._localCopyHasBeenModified = true;
					foreach (MethodParameter methodInputParameter in base.GetMethodInputParameters())
					{
						CimProperty item = this._objectToModify.CimInstanceProperties[methodInputParameter.Name];
						if (item == null)
						{
							CimProperty cimProperty = CimProperty.Create(methodInputParameter.Name, methodInputParameter.Value, CimValueConverter.GetCimTypeEnum(methodInputParameter.ParameterType), (CimFlags)((long)0));
							this._objectToModify.CimInstanceProperties.Add(cimProperty);
						}
						else
						{
							item.Value = methodInputParameter.Value;
						}
					}
				}
				return this._objectToModify;
			}
		}

		internal PropertySettingJob(CimJobContext jobContext, bool passThru, CimInstance objectToModify, MethodInvocationInfo methodInvocationInfo) : base(jobContext, passThru, objectToModify.ToString(), methodInvocationInfo)
		{
			this._objectToModify = objectToModify;
		}
	}
}