using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal class CimMethodParameterQualifierCollection : CimReadOnlyKeyedCollection<CimQualifier>
	{
		private readonly ClassHandle classHandle;

		private readonly int methodIndex;

		private readonly int parameterName;

		public override int Count
		{
			get
			{
				int num = 0;
				MiResult methodParametersGetQualifiersCount = ClassMethods.GetMethodParametersGetQualifiersCount(this.classHandle, this.methodIndex, this.parameterName, out num);
				CimException.ThrowIfMiResultFailure(methodParametersGetQualifiersCount);
				return num;
			}
		}

		public override CimQualifier this[string qualifierName]
		{
			get
			{
				int num = 0;
				if (!string.IsNullOrWhiteSpace(qualifierName))
				{
					MiResult methodGetQualifierElementGetIndex = ClassMethods.GetMethodGetQualifierElement_GetIndex(this.classHandle, this.methodIndex, this.parameterName, qualifierName, out num);
					MiResult miResult = methodGetQualifierElementGetIndex;
					if (miResult != MiResult.NO_SUCH_PROPERTY)
					{
						CimException.ThrowIfMiResultFailure(methodGetQualifierElementGetIndex);
						return new CimQualifierOfMethodParameter(this.classHandle, this.methodIndex, this.parameterName, num);
					}
					else
					{
						return null;
					}
				}
				else
				{
					throw new ArgumentNullException("qualifierName");
				}
			}
		}

		internal CimMethodParameterQualifierCollection(ClassHandle classHandle, int methodIndex, int parameterName)
		{
			this.classHandle = classHandle;
			this.methodIndex = methodIndex;
			this.parameterName = parameterName;
		}

		public override IEnumerator<CimQualifier> GetEnumerator()
		{
			int num = this.Count;
			for (int i = 0; i < num; i++)
			{
				yield return new CimQualifierOfMethodParameter(this.classHandle, this.methodIndex, this.parameterName, i);
			}
		}
	}
}