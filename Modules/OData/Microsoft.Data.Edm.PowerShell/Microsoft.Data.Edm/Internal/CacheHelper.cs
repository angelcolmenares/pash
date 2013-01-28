using System;

namespace Microsoft.Data.Edm.Internal
{
	internal static class CacheHelper
	{
		internal readonly static object Unknown;

		internal readonly static object CycleSentinel;

		internal readonly static object SecondPassCycleSentinel;

		private readonly static object BoxedTrue;

		private readonly static object BoxedFalse;

		static CacheHelper()
		{
			CacheHelper.Unknown = new object();
			CacheHelper.CycleSentinel = new object();
			CacheHelper.SecondPassCycleSentinel = new object();
			CacheHelper.BoxedTrue = true;
			CacheHelper.BoxedFalse = false;
		}

		internal static object BoxedBool(bool value)
		{
			if (value)
			{
				return CacheHelper.BoxedTrue;
			}
			else
			{
				return CacheHelper.BoxedFalse;
			}
		}
	}
}