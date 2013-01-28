using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Cim;
using Microsoft.PowerShell.Cmdletization;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal abstract class ExtrinsicMethodInvocationJob : MethodInvocationJobBase<CimMethodResultBase>
	{
		internal ExtrinsicMethodInvocationJob(CimJobContext jobContext, bool passThru, string methodSubject, MethodInvocationInfo methodInvocationInfo) : base(jobContext, passThru, methodSubject, methodInvocationInfo)
		{
		}

		internal CimMethodParametersCollection GetCimMethodParametersCollection()
		{
			CimMethodParametersCollection cimMethodParametersCollection = new CimMethodParametersCollection();
			foreach (MethodParameter methodInputParameter in base.GetMethodInputParameters())
			{
				CimMethodParameter cimMethodParameter = CimMethodParameter.Create(methodInputParameter.Name, methodInputParameter.Value, CimValueConverter.GetCimTypeEnum(methodInputParameter.ParameterType), (CimFlags)((long)0));
				cimMethodParametersCollection.Add(cimMethodParameter);
			}
			return cimMethodParametersCollection;
		}

		private void OnNext(CimMethodResult methodResult)
		{
			Dictionary<string, MethodParameter> strs = new Dictionary<string, MethodParameter>(StringComparer.OrdinalIgnoreCase);
			foreach (MethodParameter methodOutputParameter in base.GetMethodOutputParameters())
			{
				this.ProcessOutParameter(methodResult, methodOutputParameter, strs);
			}
			if (strs.Count != 1)
			{
				if (strs.Count > 1)
				{
					PSObject pSObject = new PSObject();
					foreach (KeyValuePair<string, MethodParameter> str in strs)
					{
						PSNoteProperty pSNoteProperty = new PSNoteProperty(str.Key, str.Value.Value);
						pSObject.Properties.Add(pSNoteProperty);
					}
					this.WriteObject(pSObject);
				}
			}
			else
			{
				MethodParameter methodParameter = strs.Values.Single<MethodParameter>();
				if (methodParameter.Value != null)
				{
					IEnumerable enumerable = LanguagePrimitives.GetEnumerable(methodParameter.Value);
					if (enumerable == null)
					{
						this.WriteObject(methodParameter.Value, methodParameter);
						return;
					}
					else
					{
						foreach (object obj in enumerable)
						{
							this.WriteObject(obj, methodParameter);
						}
					}
				}
				else
				{
					return;
				}
			}
		}

		private void OnNext(CimMethodStreamedResult streamedResult)
		{
			MethodParameter methodParameter = base.GetMethodOutputParameters().SingleOrDefault<MethodParameter>((MethodParameter p) => p.Name.Equals(streamedResult.ParameterName, StringComparison.OrdinalIgnoreCase));
			if (methodParameter != null)
			{
				IEnumerable enumerable = LanguagePrimitives.GetEnumerable(streamedResult.ItemValue);
				if (enumerable == null)
				{
					this.WriteObject(streamedResult.ItemValue, methodParameter);
				}
				else
				{
					foreach (object obj in enumerable)
					{
						this.WriteObject(obj, methodParameter);
					}
				}
				return;
			}
			else
			{
				object[] methodSubject = new object[3];
				methodSubject[0] = base.MethodSubject;
				methodSubject[1] = base.MethodName;
				methodSubject[2] = streamedResult.ParameterName;
				string str = string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_InvalidOutputParameterName, methodSubject);
				throw CimJobException.CreateWithFullControl(base.JobContext, str, "CimJob_InvalidOutputParameterName", ErrorCategory.MetadataError, null);
			}
		}

		public override void OnNext(CimMethodResultBase item)
		{
			base.ExceptionSafeWrapper(() => {
				CimMethodResult cimMethodResult = item as CimMethodResult;
				if (cimMethodResult == null)
				{
					CimMethodStreamedResult cimMethodStreamedResult = item as CimMethodStreamedResult;
					if (cimMethodStreamedResult != null)
					{
						this.OnNext(cimMethodStreamedResult);
					}
					return;
				}
				else
				{
					this.OnNext(cimMethodResult);
					return;
				}
			}
			);
		}

		private void ProcessOutParameter(CimMethodResult methodResult, MethodParameter methodParameter, IDictionary<string, MethodParameter> cmdletOutput)
		{
			object value;
			CimMethodParameter item = methodResult.OutParameters[methodParameter.Name];
			if (item == null)
			{
				value = null;
			}
			else
			{
				value = item.Value;
			}
			object obj = value;
			object dotNet = CimValueConverter.ConvertFromCimToDotNet(obj, methodParameter.ParameterType);
			if (MethodParameterBindings.Out != (methodParameter.Bindings & MethodParameterBindings.Out))
			{
				if (MethodParameterBindings.Error == (methodParameter.Bindings & MethodParameterBindings.Error))
				{
					bool flag = (bool)LanguagePrimitives.ConvertTo(dotNet, typeof(bool), CultureInfo.InvariantCulture);
					if (flag)
					{
						string str = (string)LanguagePrimitives.ConvertTo(dotNet, typeof(string), CultureInfo.InvariantCulture);
						CimJobException cimJobException = CimJobException.CreateFromMethodErrorCode(base.GetDescription(), base.JobContext, base.MethodName, str);
						throw cimJobException;
					}
				}
			}
			else
			{
				methodParameter.Value = dotNet;
				cmdletOutput.Add(methodParameter.Name, methodParameter);
				CimInstance[] cimInstanceArray = dotNet as CimInstance[];
				if (cimInstanceArray != null)
				{
					CimInstance[] cimInstanceArray1 = cimInstanceArray;
					for (int i = 0; i < (int)cimInstanceArray1.Length; i++)
					{
						CimInstance cimInstance = cimInstanceArray1[i];
                        CimCmdletAdapter.AssociateSessionOfOriginWithInstance(cimInstance, this.JobContext.Session);
					}
				}
				CimInstance cimInstance1 = dotNet as CimInstance;
				if (cimInstance1 != null)
				{
					CimCmdletAdapter.AssociateSessionOfOriginWithInstance(cimInstance1, this.JobContext.Session);
					return;
				}
			}
		}

		private void WriteObject(object cmdletOutput, MethodParameter methodParameter)
		{
			if (cmdletOutput != null && !string.IsNullOrEmpty(methodParameter.ParameterTypeName))
			{
				PSObject pSObject = PSObject.AsPSObject(cmdletOutput);
				if (!pSObject.TypeNames.Contains<string>(methodParameter.ParameterTypeName, StringComparer.OrdinalIgnoreCase))
				{
					pSObject.TypeNames.Insert(0, methodParameter.ParameterTypeName);
				}
			}
			this.WriteObject(cmdletOutput);
		}
	}
}