using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal sealed class CimQualifierOfProperty : CimQualifier
	{
		private readonly ClassHandle classHandle;

		private readonly int index;

		private readonly string propertyName;

		public override CimType CimType
		{
			get
			{
				MiType miType = MiType.Boolean;
				MiResult propertyQualifierElementAtGetType = ClassMethods.GetPropertyQualifierElementAt_GetType(this.classHandle, this.index, this.propertyName, out miType);
				CimException.ThrowIfMiResultFailure(propertyQualifierElementAtGetType);
				return miType.ToCimType();
			}
		}

		public override CimFlags Flags
		{
			get
			{
				MiFlags miFlag = 0;
				MiResult propertyQualifierElementAtGetFlags = ClassMethods.GetPropertyQualifierElementAt_GetFlags(this.classHandle, this.index, this.propertyName, out miFlag);
				CimException.ThrowIfMiResultFailure(propertyQualifierElementAtGetFlags);
				return miFlag.ToCimFlags();
			}
		}

		public override string Name
		{
			get
			{
				string str = null;
				MiResult propertyQualifierElementAtGetName = ClassMethods.GetPropertyQualifierElementAt_GetName(this.classHandle, this.index, this.propertyName, out str);
				CimException.ThrowIfMiResultFailure(propertyQualifierElementAtGetName);
				return str;
			}
		}

		public override object Value
		{
			get
			{
				object obj = null;
				MiResult propertyQualifierElementAtGetValue = ClassMethods.GetPropertyQualifierElementAt_GetValue(this.classHandle, this.index, this.propertyName, out obj);
				CimException.ThrowIfMiResultFailure(propertyQualifierElementAtGetValue);
				return CimInstance.ConvertFromNativeLayer(obj, null, null, false);
			}
		}

		internal CimQualifierOfProperty(ClassHandle classHandle, string propertyName, int index)
		{
			this.classHandle = classHandle;
			this.index = index;
			this.propertyName = propertyName;
		}
	}
}