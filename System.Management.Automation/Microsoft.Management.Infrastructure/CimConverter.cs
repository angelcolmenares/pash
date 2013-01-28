using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure
{
	public static class CimConverter
	{
		private static Dictionary<Type, CimType> _dotNetTypeToScalarCimType;

		private static Dictionary<Type, CimType> _dotNetTypeToArrayCimType;

		static CimConverter()
		{
			CimConverter.InitializeDotNetTypeToCimTypeDictionaries();
		}

		public static CimType GetCimType(Type dotNetType)
		{
			CimType cimType = CimType.Unknown;
			if (dotNetType != null)
			{
				Type[] interfaces = dotNetType.GetInterfaces();
				Type type = interfaces.SingleOrDefault<Type>((Type i) => {
					if (!i.IsGenericType)
					{
						cimType = CimType.Unknown;
                        return false;
					}
					else
					{
						return i.GetGenericTypeDefinition().Equals(typeof(IList<>));
					}
				}
				);
				if (type == null)
				{
					if (!CimConverter._dotNetTypeToScalarCimType.TryGetValue(dotNetType, out cimType))
					{
						return CimType.Unknown;
					}
					else
					{
						return cimType;
					}
				}
				else
				{
					Type genericArguments = type.GetGenericArguments()[0];
					if (!CimConverter._dotNetTypeToArrayCimType.TryGetValue(genericArguments, out cimType))
					{
						return CimType.Unknown;
					}
					else
					{
						return cimType;
					}
				}
			}
			else
			{
				throw new ArgumentNullException("dotNetType");
			}
		}

		private static CimType GetCimTypeFromDotNetValue(object dotNetValue)
		{
			if (dotNetValue != null)
			{
				CimType cimType = CimConverter.GetCimType(dotNetValue.GetType());
				if (cimType != CimType.Unknown)
				{
					return cimType;
				}
			}
			IList lists = dotNetValue as IList;
			if (lists != null)
			{
				IEnumerable<CimType> cimTypes = lists.Cast<object>().Select<object, CimType>(new Func<object, CimType>(CimConverter.GetCimTypeFromDotNetValue));
				List<CimType> list = cimTypes.Where<CimType>((CimType x) => (x != CimType.Unknown)).Distinct<CimType>().ToList<CimType>();
				if (list.Count == 1)
				{
					CimType item = list[0];
					CimType cimType1 = item;
					switch (cimType1)
					{
						case CimType.Boolean:
						{
							return CimType.BooleanArray;
						}
						case CimType.UInt8:
						{
							return CimType.UInt8Array;
						}
						case CimType.SInt8:
						{
							return CimType.SInt8Array;
						}
						case CimType.UInt16:
						{
							return CimType.UInt16Array;
						}
						case CimType.SInt16:
						{
							return CimType.SInt16Array;
						}
						case CimType.UInt32:
						{
							return CimType.UInt32Array;
						}
						case CimType.SInt32:
						{
							return CimType.SInt32Array;
						}
						case CimType.UInt64:
						{
							return CimType.UInt64Array;
						}
						case CimType.SInt64:
						{
							return CimType.SInt64Array;
						}
						case CimType.Real32:
						{
							return CimType.Real32Array;
						}
						case CimType.Real64:
						{
							return CimType.Real64Array;
						}
						case CimType.Char16:
						{
							return CimType.Char16Array;
						}
						case CimType.DateTime:
						{
							return CimType.DateTimeArray;
						}
						case CimType.String:
						{
							return CimType.StringArray;
						}
						case CimType.Reference:
						{
							return CimType.ReferenceArray;
						}
						case CimType.Instance:
						{
							return CimType.InstanceArray;
						}
					}
				}
			}
			return CimType.Unknown;
		}

		internal static CimType GetCimTypeFromDotNetValueOrThrowAnException(object dotNetValue)
		{
			CimType cimTypeFromDotNetValue = CimConverter.GetCimTypeFromDotNetValue(dotNetValue);
			if (cimTypeFromDotNetValue != CimType.Unknown)
			{
				return cimTypeFromDotNetValue;
			}
			else
			{
				throw new ArgumentException(Strings.DotNetValueToCimTypeConversionNotPossible);
			}
		}

		public static Type GetDotNetType(CimType cimType)
		{
			CimType cimType1 = cimType;
			switch (cimType1)
			{
				case CimType.Unknown:
				{
					return null;
				}
				case CimType.Boolean:
				{
					return typeof(bool);
				}
				case CimType.UInt8:
				{
					return typeof(byte);
				}
				case CimType.SInt8:
				{
					return typeof(sbyte);
				}
				case CimType.UInt16:
				{
					return typeof(ushort);
				}
				case CimType.SInt16:
				{
					return typeof(short);
				}
				case CimType.UInt32:
				{
					return typeof(uint);
				}
				case CimType.SInt32:
				{
					return typeof(int);
				}
				case CimType.UInt64:
				{
					return typeof(ulong);
				}
				case CimType.SInt64:
				{
					return typeof(long);
				}
				case CimType.Real32:
				{
					return typeof(float);
				}
				case CimType.Real64:
				{
					return typeof(double);
				}
				case CimType.Char16:
				{
					return typeof(char);
				}
				case CimType.DateTime:
				{
					return null;
				}
				case CimType.String:
				{
					return typeof(string);
				}
				case CimType.Reference:
				{
					return typeof(CimInstance);
				}
				case CimType.Instance:
				{
					return typeof(CimInstance);
				}
				case CimType.BooleanArray:
				{
					return typeof(bool[]);
				}
				case CimType.UInt8Array:
				{
					return typeof(byte[]);
				}
				case CimType.SInt8Array:
				{
					return typeof(sbyte[]);
				}
				case CimType.UInt16Array:
				{
					return typeof(ushort[]);
				}
				case CimType.SInt16Array:
				{
					return typeof(long[]);
				}
				case CimType.UInt32Array:
				{
					return typeof(uint[]);
				}
				case CimType.SInt32Array:
				{
					return typeof(int[]);
				}
				case CimType.UInt64Array:
				{
					return typeof(ulong[]);
				}
				case CimType.SInt64Array:
				{
					return typeof(long[]);
				}
				case CimType.Real32Array:
				{
					return typeof(float[]);
				}
				case CimType.Real64Array:
				{
					return typeof(double[]);
				}
				case CimType.Char16Array:
				{
					return typeof(char[]);
				}
				case CimType.DateTimeArray:
				{
					return null;
				}
				case CimType.StringArray:
				{
					return typeof(string[]);
				}
				case CimType.ReferenceArray:
				{
					return typeof(CimInstance[]);
				}
				case CimType.InstanceArray:
				{
					return typeof(CimInstance[]);
				}
			}
			return null;
		}

		private static void InitializeDotNetTypeToCimTypeDictionaries()
		{
			Dictionary<Type, CimType> types = new Dictionary<Type, CimType>();
			types.Add(typeof(sbyte), CimType.SInt8);
			types.Add(typeof(byte), CimType.UInt8);
			types.Add(typeof(short), CimType.SInt16);
			types.Add(typeof(ushort), CimType.UInt16);
			types.Add(typeof(int), CimType.SInt32);
			types.Add(typeof(uint), CimType.UInt32);
			types.Add(typeof(long), CimType.SInt64);
			types.Add(typeof(ulong), CimType.UInt64);
			types.Add(typeof(float), CimType.Real32);
			types.Add(typeof(double), CimType.Real64);
			types.Add(typeof(bool), CimType.Boolean);
			types.Add(typeof(string), CimType.String);
			types.Add(typeof(DateTime), CimType.DateTime);
			types.Add(typeof(TimeSpan), CimType.DateTime);
			types.Add(typeof(CimInstance), CimType.Instance);
			types.Add(typeof(char), CimType.Char16);
			CimConverter._dotNetTypeToScalarCimType = types;
			Dictionary<Type, CimType> types1 = new Dictionary<Type, CimType>();
			types1.Add(typeof(sbyte), CimType.SInt8Array);
			types1.Add(typeof(byte), CimType.UInt8Array);
			types1.Add(typeof(short), CimType.SInt16Array);
			types1.Add(typeof(ushort), CimType.UInt16Array);
			types1.Add(typeof(int), CimType.SInt32Array);
			types1.Add(typeof(uint), CimType.UInt32Array);
			types1.Add(typeof(long), CimType.SInt64Array);
			types1.Add(typeof(ulong), CimType.UInt64Array);
			types1.Add(typeof(float), CimType.Real32Array);
			types1.Add(typeof(double), CimType.Real64Array);
			types1.Add(typeof(bool), CimType.BooleanArray);
			types1.Add(typeof(string), CimType.StringArray);
			types1.Add(typeof(DateTime), CimType.DateTimeArray);
			types1.Add(typeof(TimeSpan), CimType.DateTimeArray);
			types1.Add(typeof(CimInstance), CimType.InstanceArray);
			types1.Add(typeof(char), CimType.Char16Array);
			CimConverter._dotNetTypeToArrayCimType = types1;
		}
	}
}