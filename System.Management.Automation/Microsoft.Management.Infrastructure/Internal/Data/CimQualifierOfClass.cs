using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal sealed class CimQualifierOfClass : CimQualifier
	{
		private readonly ClassHandle classHandle;

		private readonly int index;

		public override CimType CimType
		{
			get
			{
				MiType miType = MiType.Boolean;
				MiResult qualifierElementAtGetType = ClassMethods.GetQualifierElementAt_GetType(this.classHandle, this.index, out miType);
				CimException.ThrowIfMiResultFailure(qualifierElementAtGetType);
				return miType.ToCimType();
			}
		}

		public override CimFlags Flags
		{
			get
			{
				MiFlags miFlag = 0;
				MiResult qualifierElementAtGetFlags = ClassMethods.GetQualifierElementAt_GetFlags(this.classHandle, this.index, out miFlag);
				CimException.ThrowIfMiResultFailure(qualifierElementAtGetFlags);
				return miFlag.ToCimFlags();
			}
		}

		public override string Name
		{
			get
			{
				string str = null;
				MiResult qualifierElementAtGetName = ClassMethods.GetQualifierElementAt_GetName(this.classHandle, this.index, out str);
				CimException.ThrowIfMiResultFailure(qualifierElementAtGetName);
				return str;
			}
		}

		public override object Value
		{
			get
			{
				object obj = null;
				MiResult qualifierElementAtGetValue = ClassMethods.GetQualifierElementAt_GetValue(this.classHandle, this.index, out obj);
				CimException.ThrowIfMiResultFailure(qualifierElementAtGetValue);
				return CimInstance.ConvertFromNativeLayer(obj, null, null, false);
			}
		}

		internal CimQualifierOfClass(ClassHandle classHandle, int index)
		{
			this.classHandle = classHandle;
			this.index = index;
		}
	}
}