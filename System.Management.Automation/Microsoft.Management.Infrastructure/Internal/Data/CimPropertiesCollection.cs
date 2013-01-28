using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Internal.Data
{
	internal class CimPropertiesCollection : CimKeyedCollection<CimProperty>
	{
		private readonly SharedInstanceHandle _instanceHandle;

		private readonly CimInstance _instance;

		public override int Count
		{
			get
			{
				int num = 0;
				MiResult elementCount = InstanceMethods.GetElementCount(this._instanceHandle.Handle, out num);
				CimException.ThrowIfMiResultFailure(elementCount);
				return num;
			}
		}

		public override CimProperty this[string propertyName]
		{
			get
			{
				int num = 0;
				if (!string.IsNullOrWhiteSpace(propertyName))
				{
					MiResult elementGetIndex = InstanceMethods.GetElement_GetIndex(this._instanceHandle.Handle, propertyName, out num);
					MiResult miResult = elementGetIndex;
					if (miResult != MiResult.NO_SUCH_PROPERTY)
					{
						CimException.ThrowIfMiResultFailure(elementGetIndex);
						return new CimPropertyOfInstance(this._instanceHandle, this._instance, num);
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

		internal CimPropertiesCollection(SharedInstanceHandle instanceHandle, CimInstance instance)
		{
			this._instanceHandle = instanceHandle;
			this._instance = instance;
		}

		public override void Add(CimProperty newProperty)
		{
			if (newProperty != null)
			{
				MiResult miResult = InstanceMethods.AddElement(this._instanceHandle.Handle, newProperty.Name, CimInstance.ConvertToNativeLayer(newProperty.Value), newProperty.CimType.ToMiType(), newProperty.Flags.ToMiFlags());
				CimException.ThrowIfMiResultFailure(miResult);
				return;
			}
			else
			{
				throw new ArgumentNullException("newProperty");
			}
		}

		public override IEnumerator<CimProperty> GetEnumerator()
		{
			int num = this.Count;
			for (int i = 0; i < num; i++)
			{
				yield return new CimPropertyOfInstance(this._instanceHandle, this._instance, i);
			}
		}
	}
}