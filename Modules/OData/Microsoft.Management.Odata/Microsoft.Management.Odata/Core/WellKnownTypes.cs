using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security;

namespace Microsoft.Management.Odata.Core
{
	internal static class WellKnownTypes
	{
		public const string Namespace = "PowerShell";

		private static Dictionary<string, WellKnownTypeFactory> typeFactoryMap;

		static WellKnownTypes()
		{
			WellKnownTypes.typeFactoryMap = new Dictionary<string, WellKnownTypeFactory>();
			WellKnownTypes.typeFactoryMap.Add("PowerShell.PSCredential", new WellKnownTypeFactory(WellKnownTypes.CsdlPSCredential.Factory));
			WellKnownTypes.typeFactoryMap.Add("PowerShell.SecureString", new WellKnownTypeFactory(WellKnownTypes.CsdlSecureString.Factory));
		}

		internal static SecureString CreateSecureString(string rawString)
		{
			SecureString secureString = new SecureString();
			if (rawString != null)
			{
				string str = rawString;
				for (int i = 0; i < str.Length; i++)
				{
					char chr = str[i];
					secureString.AppendChar(chr);
				}
			}
			return secureString;
		}

		public static WellKnownTypeFactory GetFactory(string typeName)
		{
			WellKnownTypeFactory wellKnownTypeFactory = null;
			WellKnownTypes.typeFactoryMap.TryGetValue(typeName, out wellKnownTypeFactory);
			if (wellKnownTypeFactory == null)
			{
				object[] objArray = new object[1];
				objArray[0] = typeName;
				throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.ComplexTypeNotSupported, objArray));
			}
			else
			{
				return wellKnownTypeFactory;
			}
		}

		public static bool TryGetFactory(string typeName, out WellKnownTypeFactory factory)
		{
			bool flag;
			factory = null;
			try
			{
				factory = WellKnownTypes.GetFactory(typeName);
				flag = true;
			}
			catch (NotImplementedException notImplementedException)
			{
				return false;
			}
			return flag;
		}

		internal static class CsdlPSCredential
		{
			public const string TypeName = "PSCredential";

			public const string UserNameField = "UserName";

			public const string PasswordField = "Password";

			public static object Factory(Dictionary<string, object> properties)
			{
				object obj = null;
				object obj1 = null;
				properties.TryGetValue("UserName", out obj);
				properties.TryGetValue("Password", out obj1);
				if (obj != null || obj1 != null)
				{
					if (obj != null)
					{
						return new PSCredential((string)obj, WellKnownTypes.CreateSecureString(obj1 as string));
					}
					else
					{
						throw new InvalidCastException(ExceptionHelpers.GetExceptionMessage(Resources.FieldParameterNullOrEmpty, new object[0]));
					}
				}
				else
				{
					return null;
				}
			}
		}

		internal static class CsdlSecureString
		{
			public const string TypeName = "SecureString";

			public const string ContentsField = "Contents";

			public static object Factory(Dictionary<string, object> properties)
			{
				object obj = null;
				properties.TryGetValue("Contents", out obj);
				if (obj != null)
				{
					return WellKnownTypes.CreateSecureString(obj as string);
				}
				else
				{
					return null;
				}
			}
		}
	}
}