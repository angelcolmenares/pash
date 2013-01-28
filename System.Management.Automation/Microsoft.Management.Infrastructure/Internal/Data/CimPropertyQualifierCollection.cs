using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal class CimPropertyQualifierCollection : CimReadOnlyKeyedCollection<CimQualifier>
	{
		private readonly ClassHandle classHandle;

		private readonly string name;

		public override int Count
		{
			get
			{
				int num = 0;
				MiResult propertyQualifierCount = ClassMethods.GetPropertyQualifier_Count(this.classHandle, this.name, out num);
				CimException.ThrowIfMiResultFailure(propertyQualifierCount);
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
					MiResult propertyQualifierIndex = ClassMethods.GetPropertyQualifier_Index(this.classHandle, this.name, qualifierName, out num);
					MiResult miResult = propertyQualifierIndex;
					if (miResult != MiResult.NO_SUCH_PROPERTY)
					{
						CimException.ThrowIfMiResultFailure(propertyQualifierIndex);
						return new CimQualifierOfProperty(this.classHandle, this.name, num);
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

		internal CimPropertyQualifierCollection(ClassHandle classHandle, string name)
		{
			this.classHandle = classHandle;
			this.name = name;
		}

		public override IEnumerator<CimQualifier> GetEnumerator()
		{
			int num = this.Count;
			for (int i = 0; i < num; i++)
			{
				yield return new CimQualifierOfProperty(this.classHandle, this.name, i);
			}
		}
	}
}