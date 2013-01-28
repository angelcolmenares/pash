using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal class CimMethodQualifierCollection : CimReadOnlyKeyedCollection<CimQualifier>
	{
		private readonly ClassHandle classHandle;

		private readonly int methodIndex;

		public override int Count
		{
			get
			{
				int num = 0;
				MiResult methodQualifierCount = ClassMethods.GetMethodQualifierCount(this.classHandle, this.methodIndex, out num);
				CimException.ThrowIfMiResultFailure(methodQualifierCount);
				return num;
			}
		}

		public override CimQualifier this[string methodName]
		{
			get
			{
				int num = 0;
				if (!string.IsNullOrWhiteSpace(methodName))
				{
					MiResult methodQualifierElementGetIndex = ClassMethods.GetMethodQualifierElement_GetIndex(this.classHandle, this.methodIndex, methodName, out num);
					MiResult miResult = methodQualifierElementGetIndex;
					if (miResult != MiResult.NOT_FOUND)
					{
						CimException.ThrowIfMiResultFailure(methodQualifierElementGetIndex);
						return new CimMethodQualifierDeclarationOfMethod(this.classHandle, this.methodIndex, num);
					}
					else
					{
						return null;
					}
				}
				else
				{
					throw new ArgumentNullException("methodName");
				}
			}
		}

		internal CimMethodQualifierCollection(ClassHandle classHandle, int index)
		{
			this.classHandle = classHandle;
			this.methodIndex = index;
		}

		public override IEnumerator<CimQualifier> GetEnumerator()
		{
			int num = this.Count;
			for (int i = 0; i < num; i++)
			{
				yield return new CimMethodQualifierDeclarationOfMethod(this.classHandle, this.methodIndex, i);
			}
		}
	}
}