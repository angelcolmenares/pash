using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal class CimClassPropertiesCollection : CimReadOnlyKeyedCollection<CimPropertyDeclaration>
	{
		private readonly ClassHandle classHandle;

		public override int Count
		{
			get
			{
				int num = 0;
				MiResult elementCount = ClassMethods.GetElementCount(this.classHandle, out num);
				CimException.ThrowIfMiResultFailure(elementCount);
				return num;
			}
		}

		public override CimPropertyDeclaration this[string propertyName]
		{
			get
			{
				int num = 0;
				if (!string.IsNullOrWhiteSpace(propertyName))
				{
					MiResult elementGetIndex = ClassMethods.GetElement_GetIndex(this.classHandle, propertyName, out num);
					MiResult miResult = elementGetIndex;
					if (miResult != MiResult.NO_SUCH_PROPERTY)
					{
						CimException.ThrowIfMiResultFailure(elementGetIndex);
						return new CimClassPropertyOfClass(this.classHandle, num);
					}
					else
					{
						return null;
					}
				}
				else
				{
					throw new ArgumentNullException("propertyName");
				}
			}
		}

		internal CimClassPropertiesCollection(ClassHandle classHandle)
		{
			this.classHandle = classHandle;
		}

		public override IEnumerator<CimPropertyDeclaration> GetEnumerator()
		{
			int num = this.Count;
			for (int i = 0; i < num; i++)
			{
				yield return new CimClassPropertyOfClass(this.classHandle, i);
			}
		}
	}
}