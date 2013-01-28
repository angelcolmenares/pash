using System;
using System.Threading;

namespace Microsoft.PowerShell.ScheduledJob
{
	internal class StringUtil
	{
		public StringUtil()
		{
		}

		internal static string Format(string formatSpec, object o)
		{
			object[] objArray = new object[1];
			objArray[0] = o;
			return string.Format(Thread.CurrentThread.CurrentCulture, formatSpec, objArray);
		}

		internal static string Format(string formatSpec, object[] o)
		{
			return string.Format(Thread.CurrentThread.CurrentCulture, formatSpec, o);
		}
	}
}