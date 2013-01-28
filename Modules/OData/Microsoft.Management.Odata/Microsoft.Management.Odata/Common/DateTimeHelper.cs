using System;

namespace Microsoft.Management.Odata.Common
{
	internal static class DateTimeHelper
	{
		public static DateTime Now
		{
			get
			{
				DateTime? testHookDateTime = DateTimeHelper.TestHookDateTime;
				if (testHookDateTime.HasValue)
				{
					DateTime? nullable = DateTimeHelper.TestHookDateTime;
					return nullable.Value;
				}
				else
				{
					return DateTime.Now;
				}
			}
		}

		internal static DateTime? TestHookDateTime
		{
			get;set;
		}

		public static DateTime UtcNow
		{
			get
			{
				DateTime? testHookDateTime = DateTimeHelper.TestHookDateTime;
				if (testHookDateTime.HasValue)
				{
					DateTime? nullable = DateTimeHelper.TestHookDateTime;
					DateTime value = nullable.Value;
					return value.ToUniversalTime();
				}
				else
				{
					return DateTime.UtcNow;
				}
			}
		}

	}
}