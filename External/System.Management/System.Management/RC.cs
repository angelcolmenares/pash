using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace System.Management
{
	internal sealed class RC
	{
		private readonly static ResourceManager resMgr;

		static RC()
		{
			RC.resMgr = new ResourceManager(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly(), null);
		}

		private RC()
		{
		}

		public static string GetString(string strToGet)
		{
			return RC.resMgr.GetString(strToGet, CultureInfo.CurrentCulture);
		}
	}
}