using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace Microsoft.ActiveDirectory.TRLParser
{
	internal sealed class SR
	{
		public const string POLICY0000 = "POLICY0000";

		public const string POLICY0001 = "POLICY0001";

		public const string POLICY0002 = "POLICY0002";

		public const string POLICY0003 = "POLICY0003";

		public const string POLICY0004 = "POLICY0004";

		public const string POLICY0005 = "POLICY0005";

		public const string POLICY0006 = "POLICY0006";

		public const string POLICY0007 = "POLICY0007";

		public const string POLICY0008 = "POLICY0008";

		public const string POLICY0009 = "POLICY0009";

		public const string POLICY0010 = "POLICY0010";

		public const string POLICY0011 = "POLICY0011";

		public const string POLICY0012 = "POLICY0012";

		public const string POLICY0013 = "POLICY0013";

		public const string POLICY0014 = "POLICY0014";

		public const string POLICY0015 = "POLICY0015";

		public const string POLICY0016 = "POLICY0016";

		public const string POLICY0017 = "POLICY0017";

		public const string POLICY0018 = "POLICY0018";

		public const string POLICY0019 = "POLICY0019";

		public const string POLICY0020 = "POLICY0020";

		public const string POLICY0021 = "POLICY0021";

		public const string POLICY0022 = "POLICY0022";

		public const string POLICY0023 = "POLICY0023";

		public const string POLICY0024 = "POLICY0024";

		public const string POLICY0025 = "POLICY0025";

		public const string POLICY0026 = "POLICY0026";

		public const string POLICY0028 = "POLICY0028";

		public const string POLICY0029 = "POLICY0029";

		public const string POLICY0030 = "POLICY0030";

		public const string POLICY0031 = "POLICY0031";

		public const string POLICY0033 = "POLICY0033";

		public const string POLICY0034 = "POLICY0034";

		public const string POLICY0035 = "POLICY0035";

		public const string POLICY0036 = "POLICY0036";

		public const string POLICY0037 = "POLICY0037";

		public const string POLICY0038 = "POLICY0038";

		public const string POLICY0039 = "POLICY0039";

		public const string POLICY0040 = "POLICY0040";

		public const string POLICY0400 = "POLICY0400";

		public const string POLICY0401 = "POLICY0401";

		public const string POLICY0402 = "POLICY0402";

		public const string POLICY0403 = "POLICY0403";

		public const string INFO0001 = "INFO0001";

		private static ResourceManager s_instance;

		private static object s_syncRoot;

		private static ResourceManager Instance
		{
			get
			{
				if (SR.s_instance == null)
				{
					Monitor.Enter(SR.s_syncRoot);
					try
					{
						if (SR.s_instance == null)
						{
							SR.s_instance = new ResourceManager("Microsoft.ActiveDirectory.TRLParser", Assembly.GetExecutingAssembly());
						}
					}
					finally
					{
						Monitor.Exit(SR.s_syncRoot);
					}
				}
				return SR.s_instance;
			}
		}

		static SR()
		{
			SR.s_syncRoot = new object();
		}

		public SR()
		{
		}

		public static string GetString(string name, object[] args)
		{
			string str = SR.Instance.GetString(name, CultureInfo.CurrentUICulture);
			if (!string.IsNullOrEmpty(str))
			{
				if (args != null && (int)args.Length > 0)
				{
					str = string.Format(CultureInfo.CurrentCulture, str, args);
				}
				return str;
			}
			else
			{
				return name;
			}
		}
	}
}