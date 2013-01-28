using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Management
{
	internal class UnixEnumWbemClassObject : IEnumWbemClassObject
	{
		private IEnumerable<IWbemClassObject_DoNotMarshal> _objects;
		private IEnumerator<IWbemClassObject_DoNotMarshal> _enumerator;

		internal UnixEnumWbemClassObject (IEnumerable<IWbemClassObject_DoNotMarshal> objects)
		{
  			_objects = objects;
			_enumerator = _objects.GetEnumerator ();
		}

		#region IEnumWbemClassObject implementation

		public int Clone_ (out IEnumWbemClassObject ppEnum)
		{
			ppEnum = this;
			return 0;
		}

		public int Next_ (int lTimeout, int uCount, IWbemClassObject_DoNotMarshal[] apObjects, out uint puReturned)
		{
			uint ret = 0;
			while(_enumerator.MoveNext())
			{
				apObjects[ret] = _enumerator.Current;
				ret++;
				if (ret >= uCount) break;
			}
			puReturned = ret;
			return ret > 0 ? 0 : 1;
		}

		public int NextAsync_ (uint uCount, IWbemObjectSink pSink)
		{
			return 0;
		}

		public int Reset_ ()
		{
			_enumerator = _objects.GetEnumerator ();
			return 0;
		}

		public int Skip_ (int lTimeout, uint nCount)
		{
			return 0;
		}

		#endregion
	}
}

