using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace System.Runtime
{
	internal static class TypeHelper
	{
		public readonly static Type ArrayType;

		public readonly static Type BoolType;

		public readonly static Type GenericCollectionType;

		public readonly static Type ByteType;

		public readonly static Type SByteType;

		public readonly static Type CharType;

		public readonly static Type ShortType;

		public readonly static Type UShortType;

		public readonly static Type IntType;

		public readonly static Type UIntType;

		public readonly static Type LongType;

		public readonly static Type ULongType;

		public readonly static Type FloatType;

		public readonly static Type DoubleType;

		public readonly static Type DecimalType;

		public readonly static Type ExceptionType;

		public readonly static Type NullableType;

		public readonly static Type ObjectType;

		public readonly static Type StringType;

		public readonly static Type TypeType;

		public readonly static Type VoidType;

		static TypeHelper()
		{
			TypeHelper.ArrayType = typeof(Array);
			TypeHelper.BoolType = typeof(bool);
			TypeHelper.GenericCollectionType = typeof(ICollection<>);
			TypeHelper.ByteType = typeof(byte);
			TypeHelper.SByteType = typeof(sbyte);
			TypeHelper.CharType = typeof(char);
			TypeHelper.ShortType = typeof(short);
			TypeHelper.UShortType = typeof(ushort);
			TypeHelper.IntType = typeof(int);
			TypeHelper.UIntType = typeof(uint);
			TypeHelper.LongType = typeof(long);
			TypeHelper.ULongType = typeof(ulong);
			TypeHelper.FloatType = typeof(float);
			TypeHelper.DoubleType = typeof(double);
			TypeHelper.DecimalType = typeof(decimal);
			TypeHelper.ExceptionType = typeof(Exception);
			TypeHelper.NullableType = typeof(Nullable<>);
			TypeHelper.ObjectType = typeof(object);
			TypeHelper.StringType = typeof(string);
			TypeHelper.TypeType = typeof(Type);
			TypeHelper.VoidType = typeof(void);
		}

		public static bool AreReferenceTypesCompatible(Type sourceType, Type destinationType)
		{
			if (!object.ReferenceEquals(sourceType, destinationType))
			{
				return TypeHelper.IsImplicitReferenceConversion(sourceType, destinationType);
			}
			else
			{
				return true;
			}
		}

		public static bool AreTypesCompatible(object source, Type destinationType)
		{
			if (source != null)
			{
				return TypeHelper.AreTypesCompatible(source.GetType(), destinationType);
			}
			else
			{
				if (!destinationType.IsValueType)
				{
					return true;
				}
				else
				{
					return TypeHelper.IsNullableType(destinationType);
				}
			}
		}

		public static bool AreTypesCompatible(Type sourceType, Type destinationType)
		{
			if (!object.ReferenceEquals(sourceType, destinationType))
			{
				if (TypeHelper.IsImplicitNumericConversion(sourceType, destinationType) || TypeHelper.IsImplicitReferenceConversion(sourceType, destinationType) || TypeHelper.IsImplicitBoxingConversion(sourceType, destinationType))
				{
					return true;
				}
				else
				{
					return TypeHelper.IsImplicitNullableConversion(sourceType, destinationType);
				}
			}
			else
			{
				return true;
			}
		}

		public static bool ContainsCompatibleType(IEnumerable<Type> enumerable, Type targetType)
		{
			bool flag;
			IEnumerator<Type> enumerator = enumerable.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					Type current = enumerator.Current;
					if (!TypeHelper.AreTypesCompatible(current, targetType))
					{
						continue;
					}
					flag = true;
					return flag;
				}
				return false;
			}
			return flag;
		}

		public static T Convert<T>(object source)
		{
			T t = default(T);
			if (!(source is T))
			{
				if (source != null)
				{
					if (!TypeHelper.TryNumericConversion<T>(source, out t))
					{
						throw Fx.Exception.AsError(new InvalidCastException(InternalSR.CannotConvertObject(source, typeof(T))));
					}
					else
					{
						return t;
					}
				}
				else
				{
					if (!typeof(T).IsValueType || TypeHelper.IsNullableType(typeof(T)))
					{
						T t1 = default(T);
						return t1;
					}
					else
					{
						throw Fx.Exception.AsError(new InvalidCastException(InternalSR.CannotConvertObject(source, typeof(T))));
					}
				}
			}
			else
			{
				return (T)source;
			}
		}

		public static IEnumerable<Type> GetCompatibleTypes(IEnumerable<Type> enumerable, Type targetType)
		{
			foreach (Type type in enumerable)
			{
				if (!TypeHelper.AreTypesCompatible(type, targetType))
				{
					continue;
				}
				yield return type;
			}
		}

		public static object GetDefaultValueForType(Type type)
		{
			if (type.IsValueType)
			{
				if (type.IsEnum)
				{
					Array values = Enum.GetValues(type);
					if (values.Length > 0)
					{
						return values.GetValue(0);
					}
				}
				return Activator.CreateInstance(type);
			}
			else
			{
				return null;
			}
		}

		public static IEnumerable<Type> GetImplementedTypes(Type type)
		{
			Dictionary<Type, object> types = new Dictionary<Type, object>();
			TypeHelper.GetImplementedTypesHelper(type, types);
			return types.Keys;
		}

		private static void GetImplementedTypesHelper(Type type, Dictionary<Type, object> typesEncountered)
		{
			if (!typesEncountered.ContainsKey(type))
			{
				typesEncountered.Add(type, type);
				Type[] interfaces = type.GetInterfaces();
				for (int i = 0; i < (int)interfaces.Length; i++)
				{
					TypeHelper.GetImplementedTypesHelper(interfaces[i], typesEncountered);
				}
				for (Type j = type.BaseType; j != null && j != TypeHelper.ObjectType; j = j.BaseType)
				{
					TypeHelper.GetImplementedTypesHelper(j, typesEncountered);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private static bool IsImplicitBoxingConversion(Type sourceType, Type destinationType)
		{
			if (!sourceType.IsValueType || !(destinationType == TypeHelper.ObjectType) && !(destinationType == typeof(ValueType)))
			{
				if (!sourceType.IsEnum || !(destinationType == typeof(Enum)))
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
		}

		private static bool IsImplicitNullableConversion(Type sourceType, Type destinationType)
		{
			if (TypeHelper.IsNullableType(destinationType))
			{
				destinationType = destinationType.GetGenericArguments()[0];
				if (TypeHelper.IsNullableType(sourceType))
				{
					sourceType = sourceType.GetGenericArguments()[0];
				}
				return TypeHelper.AreTypesCompatible(sourceType, destinationType);
			}
			else
			{
				return false;
			}
		}

		private static bool IsImplicitNumericConversion(Type source, Type destination)
		{
			TypeCode typeCode = Type.GetTypeCode(source);
			TypeCode typeCode1 = Type.GetTypeCode(destination);
			TypeCode typeCode2 = typeCode;
			switch (typeCode2)
			{
				case TypeCode.Char:
				{
					TypeCode typeCode3 = typeCode1;
					switch (typeCode3)
					{
						case TypeCode.UInt16:
						case TypeCode.Int32:
						case TypeCode.UInt32:
						case TypeCode.Int64:
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
						{
							return true;
						}
					}
					return false;
				}
				case TypeCode.SByte:
				{
					TypeCode typeCode4 = typeCode1;
					switch (typeCode4)
					{
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
						{
							return true;
						}
						case TypeCode.UInt16:
						case TypeCode.UInt32:
						case TypeCode.UInt64:
						{
							return false;
						}
						default:
						{
							return false;
						}
					}
				}
				case TypeCode.Byte:
				{
					TypeCode typeCode5 = typeCode1;
					switch (typeCode5)
					{
						case TypeCode.Int16:
						case TypeCode.UInt16:
						case TypeCode.Int32:
						case TypeCode.UInt32:
						case TypeCode.Int64:
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
						{
							return true;
						}
					}
					return false;
				}
				case TypeCode.Int16:
				{
					TypeCode typeCode6 = typeCode1;
					switch (typeCode6)
					{
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
						{
							return true;
						}
						case TypeCode.UInt32:
						case TypeCode.UInt64:
						{
							return false;
						}
						default:
						{
							return false;
						}
					}
				}
				case TypeCode.UInt16:
				{
					TypeCode typeCode7 = typeCode1;
					switch (typeCode7)
					{
						case TypeCode.Int32:
						case TypeCode.UInt32:
						case TypeCode.Int64:
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
						{
							return true;
						}
					}
					return false;
				}
				case TypeCode.Int32:
				{
					TypeCode typeCode8 = typeCode1;
					switch (typeCode8)
					{
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
						{
							return true;
						}
						case TypeCode.UInt64:
						{
							return false;
						}
						default:
						{
							return false;
						}
					}
				}
				case TypeCode.UInt32:
				{
					TypeCode typeCode9 = typeCode1;
					switch (typeCode9)
					{
						case TypeCode.UInt32:
						case TypeCode.Int64:
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
						{
							return true;
						}
					}
					return false;
				}
				case TypeCode.Int64:
				case TypeCode.UInt64:
				{
					TypeCode typeCode10 = typeCode1;
					switch (typeCode10)
					{
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
						{
							return true;
						}
					}
					return false;
				}
				case TypeCode.Single:
				{
					return typeCode1 == TypeCode.Double;
				}
			}
			return false;
		}

		private static bool IsImplicitReferenceConversion(Type sourceType, Type destinationType)
		{
			return destinationType.IsAssignableFrom(sourceType);
		}

		public static bool IsNonNullableValueType(Type type)
		{
			if (type.IsValueType)
			{
				if (!type.IsGenericType)
				{
					return type != TypeHelper.StringType;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		private static bool IsNullableType(Type type)
		{
			if (!type.IsGenericType)
			{
				return false;
			}
			else
			{
				return type.GetGenericTypeDefinition() == TypeHelper.NullableType;
			}
		}

		public static bool IsNullableValueType(Type type)
		{
			if (!type.IsValueType)
			{
				return false;
			}
			else
			{
				return TypeHelper.IsNullableType(type);
			}
		}

		public static bool ShouldFilterProperty(PropertyDescriptor property, Attribute[] attributes)
		{
			if (attributes == null || (int)attributes.Length == 0)
			{
				return false;
			}
			else
			{
				for (int i = 0; i < (int)attributes.Length; i++)
				{
					Attribute attribute = attributes[i];
					Attribute item = property.Attributes[attribute.GetType()];
					if (item != null)
					{
						if (!attribute.Match(item))
						{
							return true;
						}
					}
					else
					{
						if (!attribute.IsDefaultAttribute())
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		private static bool TryNumericConversion<T>(object source, out T result)
		{
			TypeCode typeCode = Type.GetTypeCode(source.GetType());
			TypeCode typeCode1 = Type.GetTypeCode(typeof(T));
			TypeCode typeCode2 = typeCode;
			switch (typeCode2)
			{
				case TypeCode.Char:
				{
					char chr = (char)source;
					TypeCode typeCode3 = typeCode1;
					switch (typeCode3)
					{
						case TypeCode.UInt16:
						{
							result = (T)(object)((ushort)chr);
							return true;
						}
						case TypeCode.Int32:
						{
							result = (T)(object)((int)chr);
							return true;
						}
						case TypeCode.UInt32:
						{
							result = (T)(object)((uint)chr);
							return true;
						}
						case TypeCode.Int64:
						{
							result = (T)(object)((long)((ulong)chr));
							return true;
						}
						case TypeCode.UInt64:
						{
							result = (T)(object)((ulong)chr);
							return true;
						}
						case TypeCode.Single:
						{
							result = (T)(object)((float)chr);
							return true;
						}
						case TypeCode.Double:
						{
							result = (T)(object)((double)chr);
							return true;
						}
						case TypeCode.Decimal:
						{
							result = (T)(object)chr;
							return true;
						}
					}
				}
				break;
				case TypeCode.SByte:
				{
					sbyte num = (sbyte)source;
					TypeCode typeCode4 = typeCode1;
					if (typeCode4 == TypeCode.Int16)
					{
						result = (T)(object)((short)num);
						return true;
					}
					else if (typeCode4 == TypeCode.UInt16 || typeCode4 == TypeCode.UInt32 || typeCode4 == TypeCode.UInt64)
					{
						break;
					}
					else if (typeCode4 == TypeCode.Int32)
					{
						result = (T)(object)((int)num);
						return true;
					}
					else if (typeCode4 == TypeCode.Int64)
					{
						result = (T)(object)((long)num);
						return true;
					}
					else if (typeCode4 == TypeCode.Single)
					{
						result = (T)(object)((float)num);
						return true;
					}
					else if (typeCode4 == TypeCode.Double)
					{
						result = (T)(object)((double)num);
						return true;
					}
					else if (typeCode4 == TypeCode.Decimal)
					{
						result = (T)(object)num;
						return true;
					}
					break;
				}
				case TypeCode.Byte:
				{
					byte num1 = (byte)source;
					TypeCode typeCode5 = typeCode1;
					switch (typeCode5)
					{
						case TypeCode.Int16:
						{
							result = (T)(object)((short)num1);
							return true;
						}
						case TypeCode.UInt16:
						{
							result = (T)(object)((ushort)num1);
							return true;
						}
						case TypeCode.Int32:
						{
							result = (T)(object)((int)num1);
							return true;
						}
						case TypeCode.UInt32:
						{
							result = (T)(object)((uint)num1);
							return true;
						}
						case TypeCode.Int64:
						{
							result = (T)(object)((long)((ulong)num1));
							return true;
						}
						case TypeCode.UInt64:
						{
							result = (T)(object)((ulong)num1);
							return true;
						}
						case TypeCode.Single:
						{
							result = (T)(object)((float)num1);
							return true;
						}
						case TypeCode.Double:
						{
							result = (T)(object)((double)num1);
							return true;
						}
						case TypeCode.Decimal:
						{
							result = (T)(object)num1;
							return true;
						}
					}
				}
				break;
				case TypeCode.Int16:
				{
					short num2 = (short)source;
					TypeCode typeCode6 = typeCode1;
					if (typeCode6 == TypeCode.Int32)
					{
						result = (T)(object)((int)num2);
						return true;
					}
					else if (typeCode6 == TypeCode.UInt32 || typeCode6 == TypeCode.UInt64)
					{
						break;
					}
					else if (typeCode6 == TypeCode.Int64)
					{
						result = (T)(object)((long)num2);
						return true;
					}
					else if (typeCode6 == TypeCode.Single)
					{
						result = (T)(object)((float)num2);
						return true;
					}
					else if (typeCode6 == TypeCode.Double)
					{
						result = (T)(object)((double)num2);
						return true;
					}
					else if (typeCode6 == TypeCode.Decimal)
					{
						result = (T)(object)num2;
						return true;
					}
					break;
				}
				break;
				case TypeCode.UInt16:
				{
					ushort num3 = (ushort)source;
					TypeCode typeCode7 = typeCode1;
					switch (typeCode7)
					{
						case TypeCode.Int32:
						{
							result = (T)(object)((int)num3);
							return true;
						}
						case TypeCode.UInt32:
						{
							result = (T)(object)((uint)num3);
							return true;
						}
						case TypeCode.Int64:
						{
							result = (T)(object)((long)((ulong)num3));
							return true;
						}
						case TypeCode.UInt64:
						{
							result = (T)(object)((ulong)num3);
							return true;
						}
						case TypeCode.Single:
						{
							result = (T)(object)((float)num3);
							return true;
						}
						case TypeCode.Double:
						{
							result = (T)(object)((double)num3);
							return true;
						}
						case TypeCode.Decimal:
						{
							result = (T)(object)num3;
							return true;
						}
					}
				}
				break;
				case TypeCode.Int32:
				{
					int num4 = (int)source;
					TypeCode typeCode8 = typeCode1;
					if (typeCode8 == TypeCode.Int64)
					{
						result = (T)(object)((long)num4);
						return true;
					}
					else if (typeCode8 == TypeCode.UInt64)
					{
						break;
					}
					else if (typeCode8 == TypeCode.Single)
					{
						result = (T)(object)((float)num4);
						return true;
					}
					else if (typeCode8 == TypeCode.Double)
					{
						result = (T)(object)((double)num4);
						return true;
					}
					else if (typeCode8 == TypeCode.Decimal)
					{
						result = (T)(object)num4;
						return true;
					}
					break;
				}
				break;
				case TypeCode.UInt32:
				{
					uint num5 = (uint)source;
					TypeCode typeCode9 = typeCode1;
					switch (typeCode9)
					{
						case TypeCode.UInt32:
						{
							result = (T)(object)num5;
							return true;
						}
						case TypeCode.Int64:
						{
							result = (T)(object)((long)((ulong)num5));
							return true;
						}
						case TypeCode.UInt64:
						{
							result = (T)(object)((ulong)num5);
							return true;
						}
						case TypeCode.Single:
						{
							result = (T)(object)((float)((float)num5));
							return true;
						}
						case TypeCode.Double:
						{
							result = (T)(object)((double)((float)num5));
							return true;
						}
						case TypeCode.Decimal:
						{
							result = (T)(object)num5;
							return true;
						}
					}
				}
				break;
				case TypeCode.Int64:
				{
					long num6 = (long)source;
					TypeCode typeCode10 = typeCode1;
					switch (typeCode10)
					{
						case TypeCode.Single:
						{
							result = (T)(object)((float)num6);
							return true;
						}
						case TypeCode.Double:
						{
							result = (T)(object)((double)num6);
							return true;
						}
						case TypeCode.Decimal:
						{
							result = (T)(object)num6;
							return true;
						}
					}
				}
				break;
				case TypeCode.UInt64:
				{
					ulong num7 = (ulong)source;
					TypeCode typeCode11 = typeCode1;
					switch (typeCode11)
					{
						case TypeCode.Single:
						{
							result = (T)(object)((float)((float)num7));
							return true;
						}
						case TypeCode.Double:
						{
							result = (T)(object)((double)((float)num7));
							return true;
						}
						case TypeCode.Decimal:
						{
							result = (T)(object)num7;
							return true;
						}
					}
				}
				break;
				case TypeCode.Single:
				{
					if (typeCode1 != TypeCode.Double)
					{
						break;
					}
					result = (T)(object)((double)((float)source));
					return true;
				}
				break;
			}
			result = default(T);
			return false;
		}
	}
}