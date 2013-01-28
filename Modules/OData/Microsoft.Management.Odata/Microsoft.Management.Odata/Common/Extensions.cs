using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Core;
using System;
using System.Collections;

namespace Microsoft.Management.Odata.Common
{
	internal static class Extensions
	{
		public static object ConvertTo(this IEnumerable collection, Type convertToType)
		{
			if (!TypeSystem.IsArrayType(convertToType))
			{
				if (!TypeSystem.IsStackType(convertToType))
				{
					if (!TypeSystem.IsQueueType(convertToType))
					{
						if (!TypeSystem.ContainsDictionaryInterface(convertToType))
						{
							if (!TypeSystem.ContainsEnumerableInterface(convertToType))
							{
								return null;
							}
							else
							{
								return collection.ToList(convertToType);
							}
						}
						else
						{
							return collection.ToDictionary(convertToType);
						}
					}
					else
					{
						return collection.ToQueue(convertToType);
					}
				}
				else
				{
					return collection.ToStack(convertToType);
				}
			}
			else
			{
				return collection.ToArray(convertToType);
			}
		}

		public static object ToArray(this IEnumerable collection, Type arrayType)
		{
			ExceptionHelpers.ThrowArgumentExceptionIf("arrayType", !TypeSystem.IsArrayType(arrayType), Resources.NotValidArrayType, new object[0]);
			ArrayList arrayLists = new ArrayList();
			foreach (object obj in collection)
			{
				arrayLists.Add(obj);
			}
			Array arrays = Array.CreateInstance(arrayType.GetElementType(), arrayLists.Count);
			for (int i = 0; i < arrayLists.Count; i++)
			{
				arrays.SetValue(arrayLists[i], i);
			}
			return arrays;
		}

		public static object ToDictionary(this IEnumerable collection, Type dictionaryType)
		{
			ExceptionHelpers.ThrowArgumentExceptionIf("dictionaryType", !TypeSystem.ContainsInterface(dictionaryType, typeof(IDictionary)), Resources.NotValidDictionaryType, new object[0]);
			IDictionary dictionaries = TypeSystem.CreateInstance(dictionaryType) as IDictionary;
			foreach (object obj in collection)
			{
				object propertyValue = TypeSystem.GetPropertyValue(obj, "Key", true);
				object propertyValue1 = TypeSystem.GetPropertyValue(obj, "Value", true);
				dictionaries.Add(propertyValue, propertyValue1);
			}
			return dictionaries;
		}

		public static object ToList(this IEnumerable collection, Type listType)
		{
			ExceptionHelpers.ThrowArgumentExceptionIf("listType", !TypeSystem.ContainsInterface(listType, typeof(IList)), Resources.NotValidListType, new object[0]);
			IList lists = TypeSystem.CreateInstance(listType) as IList;
			foreach (object obj in collection)
			{
				lists.Add(obj);
			}
			return lists;
		}

		public static object ToQueue(this IEnumerable collection, Type queueType)
		{
			ExceptionHelpers.ThrowArgumentExceptionIf("queueType", !TypeSystem.IsQueueType(queueType), Resources.NotValidQueueType, new object[0]);
			object obj = TypeSystem.CreateInstance(queueType);
			foreach (object obj1 in collection)
			{
				object[] objArray = new object[1];
				objArray[0] = obj1;
				TypeSystem.InvokeMethod(obj, "Enqueue", objArray);
			}
			return obj;
		}

		public static object ToStack(this IEnumerable collection, Type stackType)
		{
			ExceptionHelpers.ThrowArgumentExceptionIf("stackType", !TypeSystem.IsStackType(stackType), Resources.NotValidStackType, new object[0]);
			IEnumerable enumerable = TypeSystem.CreateInstance(stackType) as IEnumerable;
			foreach (object obj in collection)
			{
				object[] objArray = new object[1];
				objArray[0] = obj;
				TypeSystem.InvokeMethod(enumerable, "Push", objArray);
			}
			object obj1 = TypeSystem.CreateInstance(stackType);
			foreach (object obj2 in enumerable)
			{
				object[] objArray1 = new object[1];
				objArray1[0] = obj2;
				TypeSystem.InvokeMethod(obj1, "Push", objArray1);
			}
			return obj1;
		}
	}
}