using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal class CimMethodDeclarationCollection : CimReadOnlyKeyedCollection<CimMethodDeclaration>
	{
		private readonly ClassHandle classHandle;

		public override int Count
		{
			get
			{
				int num = 0;
				MiResult methodCount = ClassMethods.GetMethodCount(this.classHandle, out num);
				CimException.ThrowIfMiResultFailure(methodCount);
				return num;
			}
		}

		public override CimMethodDeclaration this[string methodName]
		{
			get
			{
				int num = 0;
				if (!string.IsNullOrWhiteSpace(methodName))
				{
					MiResult methodGetIndex = ClassMethods.GetMethod_GetIndex(this.classHandle, methodName, out num);
					MiResult miResult = methodGetIndex;
					if (miResult != MiResult.METHOD_NOT_FOUND)
					{
						CimException.ThrowIfMiResultFailure(methodGetIndex);
						return new CimMethodDeclarationOfClass(this.classHandle, num);
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

		internal CimMethodDeclarationCollection(ClassHandle classHandle)
		{
			this.classHandle = classHandle;
		}

		public override IEnumerator<CimMethodDeclaration> GetEnumerator()
		{
			int num = this.Count;
			for (int i = 0; i < num; i++)
			{
				yield return new CimMethodDeclarationOfClass(this.classHandle, i);
			}
		}
	}
}