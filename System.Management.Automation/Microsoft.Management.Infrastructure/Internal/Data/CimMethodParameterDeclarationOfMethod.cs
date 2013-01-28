using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal sealed class CimMethodParameterDeclarationOfMethod : CimMethodParameterDeclaration
	{
		private readonly ClassHandle classHandle;

		private readonly int index;

		private readonly int parameterName;

		public override CimType CimType
		{
			get
			{
				MiType miType = MiType.Boolean;
				MiResult methodAtGetType = ClassMethods.GetMethodAt_GetType(this.classHandle, this.index, this.parameterName, out miType);
				CimException.ThrowIfMiResultFailure(methodAtGetType);
				return miType.ToCimType();
			}
		}

		public override string Name
		{
			get
			{
				string str = null;
				MiResult methodAtGetName = ClassMethods.GetMethodAt_GetName(this.classHandle, this.index, this.parameterName, out str);
				CimException.ThrowIfMiResultFailure(methodAtGetName);
				return str;
			}
		}

		public override CimReadOnlyKeyedCollection<CimQualifier> Qualifiers
		{
			get
			{
				return new CimMethodParameterQualifierCollection(this.classHandle, this.index, this.parameterName);
			}
		}

		public override string ReferenceClassName
		{
			get
			{
				string str = null;
				MiResult methodAtGetReferenceClass = ClassMethods.GetMethodAt_GetReferenceClass(this.classHandle, this.index, this.parameterName, out str);
				CimException.ThrowIfMiResultFailure(methodAtGetReferenceClass);
				return str;
			}
		}

		internal CimMethodParameterDeclarationOfMethod(ClassHandle classHandle, int index, int name)
		{
			this.classHandle = classHandle;
			this.index = index;
			this.parameterName = name;
		}
	}
}