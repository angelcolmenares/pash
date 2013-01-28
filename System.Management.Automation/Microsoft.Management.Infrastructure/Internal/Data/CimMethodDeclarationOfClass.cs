using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal sealed class CimMethodDeclarationOfClass : CimMethodDeclaration
	{
		private readonly ClassHandle classHandle;

		private readonly int index;

		public override string Name
		{
			get
			{
				string str = null;
				MiResult methodElementAtGetName = ClassMethods.GetMethodElementAt_GetName(this.classHandle, this.index, out str);
				CimException.ThrowIfMiResultFailure(methodElementAtGetName);
				return str;
			}
		}

		public override CimReadOnlyKeyedCollection<CimMethodParameterDeclaration> Parameters
		{
			get
			{
				return new CimMethodParameterDeclarationCollection(this.classHandle, this.index);
			}
		}

		public override CimReadOnlyKeyedCollection<CimQualifier> Qualifiers
		{
			get
			{
				return new CimMethodQualifierCollection(this.classHandle, this.index);
			}
		}

		public override CimType ReturnType
		{
			get
			{
				MiType miType = MiType.Boolean;
				MiResult methodElementAtGetType = ClassMethods.GetMethodElementAt_GetType(this.classHandle, this.index, out miType);
				CimException.ThrowIfMiResultFailure(methodElementAtGetType);
				return miType.ToCimType();
			}
		}

		internal CimMethodDeclarationOfClass(ClassHandle classHandle, int index)
		{
			this.classHandle = classHandle;
			this.index = index;
		}
	}
}