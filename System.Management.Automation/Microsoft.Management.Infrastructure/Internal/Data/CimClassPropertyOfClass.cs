using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal sealed class CimClassPropertyOfClass : CimPropertyDeclaration
	{
		private readonly ClassHandle classHandle;

		private readonly int index;

		public override CimType CimType
		{
			get
			{
				MiType miType = MiType.Boolean;
				MiResult elementAtGetType = ClassMethods.GetElementAt_GetType(this.classHandle, this.index, out miType);
				CimException.ThrowIfMiResultFailure(elementAtGetType);
				return miType.ToCimType();
			}
		}

		public override CimFlags Flags
		{
			get
			{
				MiFlags miFlag = 0;
				MiResult elementAtGetFlags = ClassMethods.GetElementAt_GetFlags(this.classHandle, this.index, out miFlag);
				CimException.ThrowIfMiResultFailure(elementAtGetFlags);
				return miFlag.ToCimFlags();
			}
		}

		public override string Name
		{
			get
			{
				string str = null;
				MiResult elementAtGetName = ClassMethods.GetElementAt_GetName(this.classHandle, this.index, out str);
				CimException.ThrowIfMiResultFailure(elementAtGetName);
				return str;
			}
		}

		public override CimReadOnlyKeyedCollection<CimQualifier> Qualifiers
		{
			get
			{
				return new CimPropertyQualifierCollection(this.classHandle, this.Name);
			}
		}

		public override string ReferenceClassName
		{
			get
			{
				string str = null;
				MiResult elementAtGetReferenceClass = ClassMethods.GetElementAt_GetReferenceClass(this.classHandle, this.index, out str);
				CimException.ThrowIfMiResultFailure(elementAtGetReferenceClass);
				return str;
			}
		}

		public override object Value
		{
			get
			{
				object obj = null;
				MiResult elementAtGetValue = ClassMethods.GetElementAt_GetValue(this.classHandle, this.index, out obj);
				CimException.ThrowIfMiResultFailure(elementAtGetValue);
				return CimInstance.ConvertFromNativeLayer(obj, null, null, false);
			}
		}

		internal CimClassPropertyOfClass(ClassHandle classHandle, int index)
		{
			this.classHandle = classHandle;
			this.index = index;
		}
	}
}