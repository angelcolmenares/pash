using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class ADMultivalueArrayParameter<T> : IADCustomParameter
	{
		private T[] _array;

		internal ADMultivalueArrayParameter(T[] array)
		{
			this._array = array;
		}

		public ADPropertyValueCollection ConvertToADPropertyValueCollection(string parameterName)
		{
			ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection();
			aDPropertyValueCollection.TrackChanges = true;
			T[] tArray = this._array;
			for (int i = 0; i < (int)tArray.Length; i++)
			{
				T t = tArray[i];
				if (t != null)
				{
					aDPropertyValueCollection.Add(t);
				}
				else
				{
					aDPropertyValueCollection.Value = null;
				}
			}
			return aDPropertyValueCollection;
		}

		public object GetOriginalValue()
		{
			return this._array;
		}
	}
}