using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Management.Odata.Core
{
	internal static class ClrInstanceBuilder
	{
		public static object Build(Type type, Dictionary<string, object> properties)
		{
			object obj = TypeSystem.CreateInstance(type);
			foreach (string key in properties.Keys)
			{
				object item = properties[key];
				PropertyInfo settablePropertyInfo = TypeSystem.GetSettablePropertyInfo(type, key);
				if (settablePropertyInfo == null)
				{
					FieldInfo fieldInfoFromPropertyName = TypeSystem.GetFieldInfoFromPropertyName(type, key);
					if (fieldInfoFromPropertyName != null)
					{
						item = TypeSystem.ConvertEnumerableToCollection(item, fieldInfoFromPropertyName.FieldType);
						fieldInfoFromPropertyName.SetValue(obj, item);
					}
					else
					{
						object[] assemblyQualifiedName = new object[2];
						assemblyQualifiedName[0] = key;
						assemblyQualifiedName[1] = type.AssemblyQualifiedName;
						throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.SettablePropertyNotFound, assemblyQualifiedName), "instance");
					}
				}
				else
				{
					item = TypeSystem.ConvertEnumerableToCollection(item, settablePropertyInfo.PropertyType);
					settablePropertyInfo.SetValue(obj, item, null);
				}
			}
			return obj;
		}
	}
}