using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Management.Odata.Core
{
	internal static class TypeSystem
	{
		private static Dictionary<Type, object> defaults;

		private static Dictionary<Type, object> explicitDefaults;

		static TypeSystem()
		{
			TypeSystem.defaults = new Dictionary<Type, object>();
			TypeSystem.defaults.Add(typeof(bool), false);
			TypeSystem.defaults.Add(typeof(char), char.MinValue);
			TypeSystem.defaults.Add(typeof(float), 0f);
			TypeSystem.defaults.Add(typeof(double), 0);
			TypeSystem.defaults.Add(typeof(byte), (byte)0);
			TypeSystem.defaults.Add(typeof(short), (short)0);
			TypeSystem.defaults.Add(typeof(int), 0);
			TypeSystem.defaults.Add(typeof(long), (long)0);
			TypeSystem.defaults.Add(typeof(sbyte), (sbyte)0);
			TypeSystem.defaults.Add(typeof(ushort), (ushort)0);
			TypeSystem.defaults.Add(typeof(uint), (uint)0);
			TypeSystem.defaults.Add(typeof(ulong), (ulong)((long)0));
			TypeSystem.explicitDefaults = new Dictionary<Type, object>();
			TypeSystem.explicitDefaults.Add(typeof(bool), false);
			TypeSystem.explicitDefaults.Add(typeof(char), (char)0);
			TypeSystem.explicitDefaults.Add(typeof(float), 0f);
			TypeSystem.explicitDefaults.Add(typeof(double), 0);
			TypeSystem.explicitDefaults.Add(typeof(byte), (byte)0);
			TypeSystem.explicitDefaults.Add(typeof(short), (short)0);
			TypeSystem.explicitDefaults.Add(typeof(int), 0);
			TypeSystem.explicitDefaults.Add(typeof(long), (long)0);
			TypeSystem.explicitDefaults.Add(typeof(sbyte), (sbyte)0);
			TypeSystem.explicitDefaults.Add(typeof(ushort), (ushort)0);
			TypeSystem.explicitDefaults.Add(typeof(uint), (uint)0);
			TypeSystem.explicitDefaults.Add(typeof(ulong), (ulong)((long)0));
			TypeSystem.explicitDefaults.Add(typeof(string), string.Empty);
			DateTime dateTime = new DateTime();
			TypeSystem.explicitDefaults.Add(typeof(DateTime), dateTime);
			TypeSystem.explicitDefaults.Add(typeof(decimal), new decimal(0));
			Guid guid = new Guid();
			TypeSystem.explicitDefaults.Add(typeof(Guid), guid);
		}

		public static bool ContainsDictionaryInterface(Type type)
		{
			return TypeSystem.ContainsInterface(type, typeof(IDictionary));
		}

		public static bool ContainsEnumerableInterface(Type type)
		{
			if (type != typeof(string))
			{
				return TypeSystem.ContainsInterface(type, typeof(IEnumerable));
			}
			else
			{
				return false;
			}
		}

		public static bool ContainsInterface(Type type, Type interfaceType)
		{
			if (type != null)
			{
				return type.GetInterfaces().Any<Type>((Type it) => it == interfaceType);
			}
			else
			{
				return false;
			}
		}

		public static object ConvertEnumerableToCollection(object value, Type convertToType)
		{
			if (value != null && value.GetType() != convertToType && TypeSystem.ContainsEnumerableInterface(value.GetType()))
			{
				object obj = (value as IEnumerable).ConvertTo(convertToType);
				if (obj != null)
				{
					value = obj;
				}
			}
			return value;
		}

		public static object ConvertEnumerableToCollection(object value, string convertToTypeName)
		{
			if (value != null && TypeSystem.ContainsEnumerableInterface(value.GetType()))
			{
				TypeWrapper typeWrapper = new TypeWrapper(convertToTypeName);
				value = TypeSystem.ConvertEnumerableToCollection(value, typeWrapper.Value);
			}
			return value;
		}

		public static object CreateInstance(Type clrType)
		{
			return Activator.CreateInstance(clrType);
		}

		public static object CreateInstance(Type clrType, object[] args)
		{
			return Activator.CreateInstance(clrType, args);
		}

		private static Type FindIEnumerable(Type seqType)
		{
			Type type;
			if (seqType == null || seqType == typeof(string))
			{
				return null;
			}
			else
			{
				if (!seqType.IsArray)
				{
					if (seqType.IsGenericType)
					{
						Type[] genericArguments = seqType.GetGenericArguments();
						int num = 0;
						while (num < (int)genericArguments.Length)
						{
							Type type1 = genericArguments[num];
							Type[] typeArray = new Type[1];
							typeArray[0] = type1;
							Type type2 = typeof(IEnumerable<>).MakeGenericType(typeArray);
							if (!type2.IsAssignableFrom(seqType))
							{
								num++;
							}
							else
							{
								type = type2;
								return type;
							}
						}
					}
					Type[] interfaces = seqType.GetInterfaces();
					if (interfaces != null && (int)interfaces.Length > 0)
					{
						Type[] typeArray1 = interfaces;
						int num1 = 0;
						while (num1 < (int)typeArray1.Length)
						{
							Type type3 = typeArray1[num1];
							Type type4 = TypeSystem.FindIEnumerable(type3);
							if (type4 == null)
							{
								num1++;
							}
							else
							{
								type = type4;
								return type;
							}
						}
					}
					if (!(seqType.BaseType != null) || !(seqType.BaseType != typeof(object)))
					{
						return null;
					}
					else
					{
						return TypeSystem.FindIEnumerable(seqType.BaseType);
					}
				}
				else
				{
					Type[] elementType = new Type[1];
					elementType[0] = seqType.GetElementType();
					return typeof(IEnumerable<>).MakeGenericType(elementType);
				}
			}
		}

		public static object GetDefaultValue(Type type)
		{
			if (TypeSystem.defaults.ContainsKey(type))
			{
				return TypeSystem.defaults[type];
			}
			else
			{
				return null;
			}
		}

		public static object GetExplicitDefaultValue(Type type)
		{
			type = TypeSystem.GetNonNullableType(type);
			if (!TypeSystem.explicitDefaults.ContainsKey(type))
			{
				throw new ArgumentException(string.Concat("no explicit default defined for type ", type.ToString()));
			}
			else
			{
				return TypeSystem.explicitDefaults[type];
			}
		}

		public static FieldInfo GetFieldInfoFromPropertyName(Type type, string propertyName)
		{
			IEnumerable<FieldInfo> fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where<FieldInfo>((FieldInfo it) => it.Name.ToLower().Contains(propertyName.ToLower()));
			if (fieldInfos.Count<FieldInfo>() != 1)
			{
				return null;
			}
			else
			{
				return fieldInfos.First<FieldInfo>();
			}
		}

		public static object GetFieldValue(object instance, string propertyName)
		{
			object value;
			Type type = instance.GetType();
			FieldInfo fieldInfoFromPropertyName = TypeSystem.GetFieldInfoFromPropertyName(type, propertyName);
			if (fieldInfoFromPropertyName != null)
			{
				try
				{
					value = fieldInfoFromPropertyName.GetValue(instance);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					TraceHelper.Current.DebugMessage(exception.ToTraceMessage(string.Concat("GetFieldValue failed to parse property: ", propertyName)));
					if (!exception.IsIgnorablePropertyException())
					{
						if (!exception.IsSevereException())
						{
							object[] message = new object[2];
							message[0] = propertyName;
							message[1] = exception.Message;
							throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.PropertyRetrievalFailed, message), exception);
						}
						else
						{
							throw;
						}
					}
					else
					{
						value = null;
					}
				}
				return value;
			}
			else
			{
				return null;
			}
		}

		public static Type GetIEnumerableElementType(Type type)
		{
			Type type1 = TypeSystem.FindIEnumerable(type);
			if (type1 != null)
			{
				return type1.GetGenericArguments()[0];
			}
			else
			{
				return null;
			}
		}

		public static Type GetNonNullableType(Type type)
		{
			if (!(type != typeof(string)) || !TypeSystem.IsNullableType(type))
			{
				return type;
			}
			else
			{
				return type.GetGenericArguments()[0];
			}
		}

		public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
		{
			Func<PropertyInfo, bool> func = null;
			PropertyInfo property = type.GetProperty(propertyName);
			if (property == null)
			{
				PropertyInfo[] properties = type.GetProperties();
				if (func == null)
				{
					func = (PropertyInfo it) => it.Name == propertyName;
				}
				property = properties.FirstOrDefault<PropertyInfo>(func);
			}
			return property;
		}

		public static object GetPropertyValue(object instance, string propertyName, bool throwIfMissing = true)
		{
			object value;
			Type type = instance.GetType();
			PropertyInfo propertyInfo = TypeSystem.GetPropertyInfo(type, propertyName);
			if (propertyInfo == null)
			{
				object fieldValue = TypeSystem.GetFieldValue(instance, propertyName);
				if (fieldValue != null)
				{
					return fieldValue;
				}
			}
			if (!throwIfMissing)
			{
				if (propertyInfo == null)
				{
					return null;
				}
			}
			else
			{
				if (propertyInfo == null)
				{
					object[] assemblyQualifiedName = new object[2];
					assemblyQualifiedName[0] = propertyName;
					assemblyQualifiedName[1] = type.AssemblyQualifiedName;
					throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.PropertyNotFoundInDotNetType, assemblyQualifiedName), "propertyName");
				}
			}
			try
			{
				value = propertyInfo.GetValue(instance, null);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				TraceHelper.Current.DebugMessage(exception.ToTraceMessage(string.Concat("GetPropertyValue failed to parse property: ", propertyName)));
				if (!exception.IsIgnorablePropertyException())
				{
					if (!exception.IsSevereException())
					{
						object[] message = new object[2];
						message[0] = propertyName;
						message[1] = exception.Message;
						throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.PropertyRetrievalFailed, message), exception);
					}
					else
					{
						throw;
					}
				}
				else
				{
					if (!throwIfMissing)
					{
						value = null;
					}
					else
					{
						object[] objArray = new object[2];
						objArray[0] = propertyName;
						objArray[1] = type.AssemblyQualifiedName;
						throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.PropertyNotFoundInDotNetType, objArray), "propertyName");
					}
				}
			}
			return value;
		}

		public static PropertyInfo GetSettablePropertyInfo(Type type, string propertyName)
		{
			PropertyInfo propertyInfo = TypeSystem.GetPropertyInfo(type, propertyName);
			if (propertyInfo == null)
			{
				return null;
			}
			else
			{
				if (propertyInfo.CanWrite)
				{
					return propertyInfo;
				}
				else
				{
					return null;
				}
			}
		}

		public static void InvokeMethod(object instance, string methodName, object[] args)
		{
			Type type = instance.GetType();
			MethodInfo method = type.GetMethod(methodName);
			if (method != null)
			{
				method.Invoke(instance, args);
			}
		}

		public static bool IsArrayType(Type type)
		{
			return type.IsArray;
		}

		public static bool IsContainerType(object source)
		{
			if (source as string == null)
			{
				if (source as ICollection != null || source as IEnumerable != null)
				{
					return true;
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

		public static bool IsNullableType(Type type)
		{
			if (type == typeof(string) || type == typeof(byte[]))
			{
				return true;
			}
			else
			{
				if (!(type != null) || !type.IsGenericType)
				{
					return false;
				}
				else
				{
					return type.GetGenericTypeDefinition() == typeof(Nullable<>);
				}
			}
		}

		public static bool IsQueueType(Type type)
		{
			if (type != typeof(Queue))
			{
				if (!type.IsGenericType)
				{
					return false;
				}
				else
				{
					return type.GetGenericTypeDefinition() == typeof(Queue<>);
				}
			}
			else
			{
				return true;
			}
		}

		public static bool IsStackType(Type type)
		{
			if (type != typeof(Stack))
			{
				if (!type.IsGenericType)
				{
					return false;
				}
				else
				{
					return type.GetGenericTypeDefinition() == typeof(Stack<>);
				}
			}
			else
			{
				return true;
			}
		}
	}
}