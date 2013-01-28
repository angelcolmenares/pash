using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal sealed class CimQualifierOfMethodParameter : CimQualifier
	{
		private readonly ClassHandle classHandle;

		private readonly int qualifierIndex;

		private readonly int parameterName;

		private readonly int methodIndex;

		public override CimType CimType
		{
			get
			{
				MiType miType = MiType.Boolean;
				MiResult methodParameterGetQualifierElementAtGetType = ClassMethods.GetMethodParameterGetQualifierElementAt_GetType(this.classHandle, this.methodIndex, this.parameterName, this.qualifierIndex, out miType);
				CimException.ThrowIfMiResultFailure(methodParameterGetQualifierElementAtGetType);
				return miType.ToCimType();
			}
		}

		public override CimFlags Flags
		{
			get
			{
				MiFlags miFlag = 0;
				MiResult methodParameterGetQualifierElementAtGetFlags = ClassMethods.GetMethodParameterGetQualifierElementAt_GetFlags(this.classHandle, this.methodIndex, this.parameterName, this.qualifierIndex, out miFlag);
				CimException.ThrowIfMiResultFailure(methodParameterGetQualifierElementAtGetFlags);
				return miFlag.ToCimFlags();
			}
		}

		public override string Name
		{
			get
			{
				string str = null;
				MiResult methodParameterGetQualifierElementAtGetName = ClassMethods.GetMethodParameterGetQualifierElementAt_GetName(this.classHandle, this.methodIndex, this.parameterName, this.qualifierIndex, out str);
				CimException.ThrowIfMiResultFailure(methodParameterGetQualifierElementAtGetName);
				return str;
			}
		}

		public override object Value
		{
			get
			{
				object obj = null;
				MiResult methodParameterGetQualifierElementAtGetValue = ClassMethods.GetMethodParameterGetQualifierElementAt_GetValue(this.classHandle, this.methodIndex, this.parameterName, this.qualifierIndex, out obj);
				CimException.ThrowIfMiResultFailure(methodParameterGetQualifierElementAtGetValue);
				return CimInstance.ConvertFromNativeLayer(obj, null, null, false);
			}
		}

		internal CimQualifierOfMethodParameter(ClassHandle classHandle, int methodIndex, int parameterName, int index)
		{
			this.classHandle = classHandle;
			this.qualifierIndex = index;
			this.parameterName = parameterName;
			this.methodIndex = methodIndex;
		}
	}
}