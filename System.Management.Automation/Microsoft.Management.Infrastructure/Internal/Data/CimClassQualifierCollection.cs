using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal class CimClassQualifierCollection : CimReadOnlyKeyedCollection<CimQualifier>
	{
		private readonly ClassHandle classHandle;

		public override int Count
		{
			get
			{
				int num = 0;
				MiResult qualifierCount = ClassMethods.GetQualifier_Count(this.classHandle, out num);
				CimException.ThrowIfMiResultFailure(qualifierCount);
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
					MiResult classQualifierIndex = ClassMethods.GetClassQualifier_Index(this.classHandle, qualifierName, out num);
					MiResult miResult = classQualifierIndex;
					if (miResult != MiResult.NOT_FOUND)
					{
						CimException.ThrowIfMiResultFailure(classQualifierIndex);
						return new CimQualifierOfClass(this.classHandle, num);
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

		internal CimClassQualifierCollection(ClassHandle classHandle)
		{
			this.classHandle = classHandle;
		}

		public override IEnumerator<CimQualifier> GetEnumerator()
		{
			int num = this.Count;
			for (int i = 0; i < num; i++)
			{
				yield return new CimQualifierOfClass(this.classHandle, i);
			}
		}
	}
}