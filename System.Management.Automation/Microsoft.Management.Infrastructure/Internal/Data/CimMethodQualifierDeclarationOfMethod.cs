using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal sealed class CimMethodQualifierDeclarationOfMethod : CimQualifier
	{
		private readonly ClassHandle classHandle;

		private readonly int qualifierIndex;

		private readonly int methodIndex;

		public override CimType CimType
		{
			get
			{
				MiType miType = MiType.Boolean;
				MiResult methodQualifierElementAtGetType = ClassMethods.GetMethodQualifierElementAt_GetType(this.classHandle, this.methodIndex, this.qualifierIndex, out miType);
				CimException.ThrowIfMiResultFailure(methodQualifierElementAtGetType);
				return miType.ToCimType();
			}
		}

		public override CimFlags Flags
		{
			get
			{
				MiFlags miFlag = 0;
				MiResult methodQualifierElementAtGetFlags = ClassMethods.GetMethodQualifierElementAt_GetFlags(this.classHandle, this.methodIndex, this.qualifierIndex, out miFlag);
				CimException.ThrowIfMiResultFailure(methodQualifierElementAtGetFlags);
				return miFlag.ToCimFlags();
			}
		}

		public override string Name
		{
			get
			{
				string str = null;
				MiResult methodQualifierElementAtGetName = ClassMethods.GetMethodQualifierElementAt_GetName(this.classHandle, this.methodIndex, this.qualifierIndex, out str);
				CimException.ThrowIfMiResultFailure(methodQualifierElementAtGetName);
				return str;
			}
		}

		public override object Value
		{
			get
			{
				object obj = null;
				MiResult methodQualifierElementAtGetValue = ClassMethods.GetMethodQualifierElementAt_GetValue(this.classHandle, this.methodIndex, this.qualifierIndex, out obj);
				CimException.ThrowIfMiResultFailure(methodQualifierElementAtGetValue);
				return CimInstance.ConvertFromNativeLayer(obj, null, null, false);
			}
		}

		internal CimMethodQualifierDeclarationOfMethod(ClassHandle classHandle, int methodIndex, int qualifierIndex)
		{
			this.classHandle = classHandle;
			this.qualifierIndex = qualifierIndex;
			this.methodIndex = methodIndex;
		}
	}
}