using System;
using System.Runtime.CompilerServices;

namespace System.Management
{
	internal class ValueTypeSafety
	{
		public ValueTypeSafety()
		{
		}

		public static object GetSafeObject(object theValue)
		{
			if (theValue != null)
			{
				if (!theValue.GetType().IsPrimitive)
				{
					return RuntimeHelpers.GetObjectValue(theValue);
				}
				else
				{
					return ((IConvertible)theValue).ToType(typeof(object), null);
				}
			}
			else
			{
				return null;
			}
		}
	}
}