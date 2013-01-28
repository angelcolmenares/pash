using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal class CimMethodParameterDeclarationCollection : CimReadOnlyKeyedCollection<CimMethodParameterDeclaration>
	{
		private readonly ClassHandle classHandle;

		private readonly int methodIndex;

		public override int Count
		{
			get
			{
				int num = 0;
				MiResult methodParametersCount = ClassMethods.GetMethodParametersCount(this.classHandle, this.methodIndex, out num);
				CimException.ThrowIfMiResultFailure(methodParametersCount);
				return num;
			}
		}

		public override CimMethodParameterDeclaration this[string parameterName]
		{
			get
			{
				int num = 0;
				if (!string.IsNullOrWhiteSpace(parameterName))
				{
					MiResult methodElementGetIndex = ClassMethods.GetMethodElement_GetIndex(this.classHandle, this.methodIndex, parameterName, out num);
					MiResult miResult = methodElementGetIndex;
					if (miResult != MiResult.NOT_FOUND)
					{
						CimException.ThrowIfMiResultFailure(methodElementGetIndex);
						return new CimMethodParameterDeclarationOfMethod(this.classHandle, this.methodIndex, num);
					}
					else
					{
						return null;
					}
				}
				else
				{
					throw new ArgumentNullException("parameterName");
				}
			}
		}

		internal CimMethodParameterDeclarationCollection(ClassHandle classHandle, int index)
		{
			this.classHandle = classHandle;
			this.methodIndex = index;
		}

		public override IEnumerator<CimMethodParameterDeclaration> GetEnumerator()
		{
			int num = this.Count;
			for (int i = 0; i < num; i++)
			{
				yield return new CimMethodParameterDeclarationOfMethod(this.classHandle, this.methodIndex, i);
			}
		}
	}
}