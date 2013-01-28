using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Data.Edm.EdmToClrConversion
{
	internal class EdmToClrConverter
	{
		private readonly static Type TypeICollectionOfT;

		private readonly static Type TypeIListOfT;

		private readonly static Type TypeListOfT;

		private readonly static Type TypeIEnumerableOfT;

		private readonly static Type TypeNullableOfT;

		private readonly static MethodInfo CastToClrTypeMethodInfo;

		private readonly static MethodInfo EnumerableToListOfTMethodInfo;

		private readonly TryCreateObjectInstance tryCreateObjectInstanceDelegate;

		private readonly Dictionary<IEdmStructuredValue, object> convertedObjects;

		private readonly Dictionary<Type, MethodInfo> enumTypeConverters;

		private readonly Dictionary<Type, MethodInfo> enumerableConverters;

		static EdmToClrConverter()
		{
			EdmToClrConverter.TypeICollectionOfT = typeof(ICollection<>);
			EdmToClrConverter.TypeIListOfT = typeof(IList<>);
			EdmToClrConverter.TypeListOfT = typeof(List<>);
			EdmToClrConverter.TypeIEnumerableOfT = typeof(IEnumerable<>);
			EdmToClrConverter.TypeNullableOfT = typeof(Nullable<>);
			EdmToClrConverter.CastToClrTypeMethodInfo = typeof(EdmToClrConverter.CastHelper).GetMethod("CastToClrType");
			EdmToClrConverter.EnumerableToListOfTMethodInfo = typeof(EdmToClrConverter.CastHelper).GetMethod("EnumerableToListOfT");
		}

		public EdmToClrConverter()
		{
			this.convertedObjects = new Dictionary<IEdmStructuredValue, object>();
			this.enumTypeConverters = new Dictionary<Type, MethodInfo>();
			this.enumerableConverters = new Dictionary<Type, MethodInfo>();
		}

		public EdmToClrConverter(TryCreateObjectInstance tryCreateObjectInstanceDelegate)
		{
			this.convertedObjects = new Dictionary<IEdmStructuredValue, object>();
			this.enumTypeConverters = new Dictionary<Type, MethodInfo>();
			this.enumerableConverters = new Dictionary<Type, MethodInfo>();
			EdmUtil.CheckArgumentNull<TryCreateObjectInstance>(tryCreateObjectInstanceDelegate, "tryCreateObjectInstanceDelegate");
			this.tryCreateObjectInstanceDelegate = tryCreateObjectInstanceDelegate;
		}

		internal static bool AsClrBoolean(IEdmValue edmValue)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			return ((IEdmBooleanValue)edmValue).Value;
		}

		internal static byte AsClrByte(IEdmValue edmValue)
		{
			return (byte)EdmToClrConverter.AsClrInt64(edmValue);
		}

		internal static byte[] AsClrByteArray(IEdmValue edmValue)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			if (edmValue as IEdmNullValue == null)
			{
				return ((IEdmBinaryValue)edmValue).Value;
			}
			else
			{
				return null;
			}
		}

		internal static char AsClrChar(IEdmValue edmValue)
		{
			return Convert.ToChar (EdmToClrConverter.AsClrInt64(edmValue));
		}

		internal static DateTime AsClrDateTime(IEdmValue edmValue)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			return ((IEdmDateTimeValue)edmValue).Value;
		}

		internal static DateTimeOffset AsClrDateTimeOffset(IEdmValue edmValue)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			return ((IEdmDateTimeOffsetValue)edmValue).Value;
		}

		internal static decimal AsClrDecimal(IEdmValue edmValue)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			return ((IEdmDecimalValue)edmValue).Value;
		}

		internal static double AsClrDouble(IEdmValue edmValue)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			return ((IEdmFloatingValue)edmValue).Value;
		}

		internal static short AsClrInt16(IEdmValue edmValue)
		{
			return (short)EdmToClrConverter.AsClrInt64(edmValue);
		}

		internal static int AsClrInt32(IEdmValue edmValue)
		{
			return (int)EdmToClrConverter.AsClrInt64(edmValue);
		}

		internal static long AsClrInt64(IEdmValue edmValue)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			return ((IEdmIntegerValue)edmValue).Value;
		}

		private object AsClrObject(IEdmValue edmValue, Type clrObjectType)
		{
			object obj = null;
			bool flag;
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			EdmUtil.CheckArgumentNull<Type>(clrObjectType, "clrObjectType");
			if (edmValue as IEdmNullValue == null)
			{
				IEdmStructuredValue edmStructuredValue = edmValue as IEdmStructuredValue;
				if (edmStructuredValue != null)
				{
					if (!this.convertedObjects.TryGetValue(edmStructuredValue, out obj))
					{
						if (clrObjectType.IsClass())
						{
							if (this.tryCreateObjectInstanceDelegate == null || !this.tryCreateObjectInstanceDelegate(edmStructuredValue, clrObjectType, this, out obj, out flag))
							{
								obj = Activator.CreateInstance(clrObjectType);
								flag = false;
							}
							else
							{
								if (obj != null)
								{
									Type type = obj.GetType();
									if (clrObjectType.IsAssignableFrom(type))
									{
										clrObjectType = type;
									}
									else
									{
										throw new InvalidCastException(Strings.EdmToClr_TryCreateObjectInstanceReturnedWrongObject(type.FullName, clrObjectType.FullName));
									}
								}
							}
							this.convertedObjects[edmStructuredValue] = obj;
							if (!flag && obj != null)
							{
								this.PopulateObjectProperties(edmStructuredValue, obj, clrObjectType);
							}
							return obj;
						}
						else
						{
							throw new InvalidCastException(Strings.EdmToClr_StructuredValueMappedToNonClass);
						}
					}
					else
					{
						return obj;
					}
				}
				else
				{
					if (edmValue as IEdmCollectionValue == null)
					{
						throw new InvalidCastException(Strings.EdmToClr_CannotConvertEdmValueToClrType(EdmToClrConverter.GetEdmValueInterfaceName(edmValue), clrObjectType.FullName));
					}
					else
					{
						throw new InvalidCastException(Strings.EdmToClr_CannotConvertEdmCollectionValueToClrType(clrObjectType.FullName));
					}
				}
			}
			else
			{
				return null;
			}
		}

		internal static float AsClrSingle(IEdmValue edmValue)
		{
			return (float)EdmToClrConverter.AsClrDouble(edmValue);
		}

		internal static string AsClrString(IEdmValue edmValue)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			if (edmValue as IEdmNullValue == null)
			{
				return ((IEdmStringValue)edmValue).Value;
			}
			else
			{
				return null;
			}
		}

		internal static TimeSpan AsClrTime(IEdmValue edmValue)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			return ((IEdmTimeValue)edmValue).Value;
		}

		public T AsClrValue<T>(IEdmValue edmValue)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			bool flag = false;
			return (T)this.AsClrValue(edmValue, typeof(T), flag);
		}

		public object AsClrValue(IEdmValue edmValue, Type clrType)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(edmValue, "edmValue");
			EdmUtil.CheckArgumentNull<Type>(clrType, "clrType");
			bool flag = true;
			return this.AsClrValue(edmValue, clrType, flag);
		}

		private object AsClrValue(IEdmValue edmValue, Type clrType, bool convertEnumValues)
		{
			object obj = null;
			TypeCode typeCode = PlatformHelper.GetTypeCode(clrType);
			if (typeCode != TypeCode.Object)
			{
				bool flag = clrType.IsEnum();
				if (flag)
				{
					IEdmEnumValue edmEnumValue = edmValue as IEdmEnumValue;
					if (edmEnumValue != null)
					{
						edmValue = edmEnumValue.Value;
					}
				}
				if (EdmToClrConverter.TryConvertAsPrimitiveType(PlatformHelper.GetTypeCode(clrType), edmValue, out obj))
				{
					if (!flag || !convertEnumValues)
					{
						return obj;
					}
					else
					{
						return this.GetEnumValue(obj, clrType);
					}
				}
				else
				{
					throw new InvalidCastException(Strings.EdmToClr_UnsupportedTypeCode(typeCode));
				}
			}
			else
			{
				if (!clrType.IsGenericType() || !(clrType.GetGenericTypeDefinition() == EdmToClrConverter.TypeNullableOfT))
				{
					if (clrType != typeof(DateTime))
					{
						if (clrType != typeof(DateTimeOffset))
						{
							if (clrType != typeof(TimeSpan))
							{
								if (clrType != typeof(byte[]))
								{
									if (!clrType.IsGenericType() || !clrType.IsInterface() || !(clrType.GetGenericTypeDefinition() == EdmToClrConverter.TypeICollectionOfT) && !(clrType.GetGenericTypeDefinition() == EdmToClrConverter.TypeIListOfT) && !(clrType.GetGenericTypeDefinition() == EdmToClrConverter.TypeIEnumerableOfT))
									{
										return this.AsClrObject(edmValue, clrType);
									}
									else
									{
										return this.AsListOfT(edmValue, clrType);
									}
								}
								else
								{
									return EdmToClrConverter.AsClrByteArray(edmValue);
								}
							}
							else
							{
								return EdmToClrConverter.AsClrTime(edmValue);
							}
						}
						else
						{
							return EdmToClrConverter.AsClrDateTimeOffset(edmValue);
						}
					}
					else
					{
						return EdmToClrConverter.AsClrDateTime(edmValue);
					}
				}
				else
				{
					if (edmValue as IEdmNullValue == null)
					{
						return this.AsClrValue(edmValue, clrType.GetGenericArguments().Single<Type>());
					}
					else
					{
						return null;
					}
				}
			}
		}

		private IEnumerable AsIEnumerable(IEdmValue edmValue, Type elementType)
		{
			foreach (IEdmDelayedValue edmDelayedValue in ((IEdmCollectionValue)edmValue).Elements)
			{
				yield return this.AsClrValue(edmDelayedValue.Value, elementType);
			}
		}

		private object AsListOfT(IEdmValue edmValue, Type clrType)
		{
			MethodInfo methodInfo = null;
			object obj;
			Type type = clrType.GetGenericArguments().Single<Type>();
			if (!this.enumerableConverters.TryGetValue(type, out methodInfo))
			{
				Type[] typeArray = new Type[1];
				typeArray[0] = type;
				methodInfo = EdmToClrConverter.EnumerableToListOfTMethodInfo.MakeGenericMethod(typeArray);
				this.enumerableConverters.Add(type, methodInfo);
			}
			try
			{
				object[] objArray = new object[1];
				objArray[0] = this.AsIEnumerable(edmValue, type);
				obj = methodInfo.Invoke(null, objArray);
			}
			catch (TargetInvocationException targetInvocationException1)
			{
				TargetInvocationException targetInvocationException = targetInvocationException1;
				if (targetInvocationException.InnerException == null || targetInvocationException.InnerException as InvalidCastException == null)
				{
					throw;
				}
				else
				{
					throw targetInvocationException.InnerException;
				}
			}
			return obj;
		}

		private static MethodInfo FindICollectionOfElementTypeAddMethod(Type collectionType, Type elementType)
		{
			Type[] typeArray = new Type[1];
			typeArray[0] = elementType;
			Type type = typeof(ICollection<>).MakeGenericType(typeArray);
			return type.GetMethod("Add");
		}

		private static PropertyInfo FindProperty(Type clrObjectType, string propertyName)
		{
			PropertyInfo item = null;
			List<PropertyInfo> list = clrObjectType.GetProperties().Where<PropertyInfo>((PropertyInfo p) => p.Name == propertyName).ToList<PropertyInfo>();
			int count = list.Count;
			switch (count)
			{
				case 0:
				{
					return null;
				}
				break;
				case 1:
				{
					return list[0];
				}
				break;
				default:
				{
					item = list[0];
					for (int i = 1; i < list.Count; i++)
					{
						PropertyInfo propertyInfo = list[i];
						if (item.DeclaringType.IsAssignableFrom(propertyInfo.DeclaringType))
						{
							item = propertyInfo;
						}
					}
				}
				break;
			}
			return item;
		}

		private static string GetEdmValueInterfaceName(IEdmValue edmValue)
		{
			Type type = typeof(IEdmValue);
			Type[] interfaces = edmValue.GetType().GetInterfaces();
			foreach (Type type1 in (IEnumerable<Type>)interfaces.OrderBy<Type, string>((Type i) => i.FullName))
			{
				if (!type.IsAssignableFrom(type1) || !(type != type1))
				{
					continue;
				}
				type = type1;
			}
			return type.Name;
		}

		private object GetEnumValue(object clrValue, Type clrType)
		{
			MethodInfo methodInfo = null;
			object obj;
			if (!this.enumTypeConverters.TryGetValue(clrType, out methodInfo))
			{
				Type[] typeArray = new Type[1];
				typeArray[0] = clrType;
				methodInfo = EdmToClrConverter.CastToClrTypeMethodInfo.MakeGenericMethod(typeArray);
				this.enumTypeConverters.Add(clrType, methodInfo);
			}
			try
			{
				object[] objArray = new object[1];
				objArray[0] = clrValue;
				obj = methodInfo.Invoke(null, objArray);
			}
			catch (TargetInvocationException targetInvocationException1)
			{
				TargetInvocationException targetInvocationException = targetInvocationException1;
				if (targetInvocationException.InnerException == null || targetInvocationException.InnerException as InvalidCastException == null)
				{
					throw;
				}
				else
				{
					throw targetInvocationException.InnerException;
				}
			}
			return obj;
		}

		private void PopulateObjectProperties(IEdmStructuredValue edmValue, object clrObject, Type clrObjectType)
		{
			HashSetInternal<string> strs = new HashSetInternal<string>();
			foreach (IEdmPropertyValue propertyValue in edmValue.PropertyValues)
			{
				PropertyInfo propertyInfo = EdmToClrConverter.FindProperty(clrObjectType, propertyValue.Name);
				if (propertyInfo == null)
				{
					continue;
				}
				if (!strs.Contains(propertyValue.Name))
				{
					if (!this.TrySetCollectionProperty(propertyInfo, clrObject, propertyValue))
					{
						object obj = this.AsClrValue(propertyValue.Value, propertyInfo.PropertyType);
						propertyInfo.SetValue(clrObject, obj, null);
					}
					strs.Add(propertyValue.Name);
				}
				else
				{
					throw new InvalidCastException(Strings.EdmToClr_StructuredPropertyDuplicateValue(propertyValue.Name));
				}
			}
		}

		public void RegisterConvertedObject(IEdmStructuredValue edmValue, object clrObject)
		{
			this.convertedObjects.Add(edmValue, clrObject);
		}

		private static bool TryConvertAsPrimitiveType(TypeCode typeCode, IEdmValue edmValue, out object clrValue)
		{
			TypeCode typeCode1 = typeCode;
			switch (typeCode1)
			{
				case TypeCode.Boolean:
				{
					clrValue = EdmToClrConverter.AsClrBoolean(edmValue);
					return true;
				}
				case TypeCode.Char:
				{
					clrValue = EdmToClrConverter.AsClrChar(edmValue);
					return true;
				}
				case TypeCode.SByte:
				{
					clrValue = (sbyte)EdmToClrConverter.AsClrInt64(edmValue);
					return true;
				}
				case TypeCode.Byte:
				{
					clrValue = EdmToClrConverter.AsClrByte(edmValue);
					return true;
				}
				case TypeCode.Int16:
				{
					clrValue = EdmToClrConverter.AsClrInt16(edmValue);
					return true;
				}
				case TypeCode.UInt16:
				{
					clrValue = (ushort)EdmToClrConverter.AsClrInt64(edmValue);
					return true;
				}
				case TypeCode.Int32:
				{
					clrValue = EdmToClrConverter.AsClrInt32(edmValue);
					return true;
				}
				case TypeCode.UInt32:
				{
					clrValue = (uint)EdmToClrConverter.AsClrInt64(edmValue);
					return true;
				}
				case TypeCode.Int64:
				{
					clrValue = EdmToClrConverter.AsClrInt64(edmValue);
					return true;
				}
				case TypeCode.UInt64:
				{
					clrValue = (ulong)EdmToClrConverter.AsClrInt64(edmValue);
					return true;
				}
				case TypeCode.Single:
				{
					clrValue = EdmToClrConverter.AsClrSingle(edmValue);
					return true;
				}
				case TypeCode.Double:
				{
					clrValue = EdmToClrConverter.AsClrDouble(edmValue);
					return true;
				}
				case TypeCode.Decimal:
				{
					clrValue = EdmToClrConverter.AsClrDecimal(edmValue);
					return true;
				}
				case TypeCode.DateTime:
				{
					clrValue = EdmToClrConverter.AsClrDateTime(edmValue);
					return true;
				}
				case TypeCode.Object | TypeCode.DateTime:
				{
					clrValue = null;
					return false;
				}
				case TypeCode.String:
				{
					clrValue = EdmToClrConverter.AsClrString(edmValue);
					return true;
				}
				default:
				{
					clrValue = null;
					return false;
				}
			}
		}

		private bool TrySetCollectionProperty(PropertyInfo clrProperty, object clrObject, IEdmPropertyValue propertyValue)
		{
			Type type;
			Type propertyType = clrProperty.PropertyType;
			if (propertyType.IsGenericType() && propertyType.IsInterface())
			{
				Type genericTypeDefinition = propertyType.GetGenericTypeDefinition();
				bool typeIEnumerableOfT = genericTypeDefinition == EdmToClrConverter.TypeIEnumerableOfT;
				if (typeIEnumerableOfT || genericTypeDefinition == EdmToClrConverter.TypeICollectionOfT || genericTypeDefinition == EdmToClrConverter.TypeIListOfT)
				{
					object value = clrProperty.GetValue(clrObject, null);
					Type type1 = propertyType.GetGenericArguments().Single<Type>();
					if (value != null)
					{
						if (!typeIEnumerableOfT)
						{
							type = value.GetType();
						}
						else
						{
							throw new InvalidCastException(Strings.EdmToClr_IEnumerableOfTPropertyAlreadyHasValue(clrProperty.Name, clrProperty.DeclaringType.FullName));
						}
					}
					else
					{
						Type[] typeArray = new Type[1];
						typeArray[0] = type1;
						type = EdmToClrConverter.TypeListOfT.MakeGenericType(typeArray);
						value = Activator.CreateInstance(type);
						clrProperty.SetValue(clrObject, value, null);
					}
					MethodInfo methodInfo = EdmToClrConverter.FindICollectionOfElementTypeAddMethod(type, type1);
					foreach (object obj in this.AsIEnumerable(propertyValue.Value, type1))
					{
						try
						{
							object[] objArray = new object[1];
							objArray[0] = obj;
							methodInfo.Invoke(value, objArray);
						}
						catch (TargetInvocationException targetInvocationException1)
						{
							TargetInvocationException targetInvocationException = targetInvocationException1;
							if (targetInvocationException.InnerException == null || targetInvocationException.InnerException as InvalidCastException == null)
							{
								throw;
							}
							else
							{
								throw targetInvocationException.InnerException;
							}
						}
					}
					return true;
				}
			}
			return false;
		}

		private static class CastHelper
		{
			public static T CastToClrType<T>(object obj)
			{
				return (T)obj;
			}

			public static List<T> EnumerableToListOfT<T>(IEnumerable enumerable)
			{
				return enumerable.Cast<T>().ToList<T>();
			}
		}
	}
}