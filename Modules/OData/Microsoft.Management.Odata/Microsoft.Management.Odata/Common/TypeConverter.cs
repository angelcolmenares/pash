using Microsoft.Management.Odata.Core;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.Odata.Common
{
	internal static class TypeConverter
	{
		public static object ConvertTo(object srcObject, Type destType)
		{
			object obj;
			if (srcObject != null)
			{
				if (!srcObject.GetType().IsEnum || !(destType != typeof(string)))
				{
					try
					{
						if (srcObject != null && srcObject.GetType() == typeof(string))
						{
							if (!TypeSystem.IsNullableType(destType) || !string.IsNullOrEmpty(srcObject as string))
							{
								if (destType == typeof(bool))
								{
									obj = bool.Parse(srcObject as string);
									return obj;
								}
							}
							else
							{
								obj = null;
								return obj;
							}
						}
						obj = LanguagePrimitives.ConvertTo(srcObject, destType, CultureInfo.InvariantCulture);
					}
					catch (PSInvalidCastException pSInvalidCastException1)
					{
						PSInvalidCastException pSInvalidCastException = pSInvalidCastException1;
						throw new InvalidCastException(ExceptionHelpers.GetInvalidCastExceptionMessage(srcObject.GetType(), destType), pSInvalidCastException);
					}
					catch (FormatException formatException1)
					{
						FormatException formatException = formatException1;
						throw new InvalidCastException(ExceptionHelpers.GetInvalidCastExceptionMessage(srcObject.GetType(), destType), formatException);
					}
					return obj;
				}
				else
				{
					throw new InvalidCastException(ExceptionHelpers.GetInvalidCastExceptionMessage(srcObject.GetType(), destType));
				}
			}
			else
			{
				return null;
			}
		}
	}
}