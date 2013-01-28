using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Management.Automation;

namespace Microsoft.Management.Odata.PS
{
	internal class CollectionResourceTypeSerializer : SerializerBase
	{
		public CollectionResourceTypeSerializer(ResourceType resourceType) : base(resourceType)
		{
			object[] resourceTypeKind = new object[2];
			resourceTypeKind[0] = resourceType.ResourceTypeKind;
			resourceTypeKind[1] = ResourceTypeKind.Collection;
			ExceptionHelpers.ThrowArgumentExceptionIf("resourceType", resourceType.ResourceTypeKind != ResourceTypeKind.Collection, new ExceptionHelpers.MessageLoader(SerializerBase.GetInvalidArgMessage), resourceTypeKind);
		}

		public override object Serialize(object clrObject, int depth)
		{
			CollectionResourceType resourceType = base.ResourceType as CollectionResourceType;
			if (clrObject as PSObject != null)
			{
				clrObject = (clrObject as PSObject).BaseObject;
			}
			if (clrObject != null)
			{
				Type type = clrObject.GetType();
				if (!TypeSystem.ContainsDictionaryInterface(type))
				{
					if (!TypeSystem.ContainsEnumerableInterface(type))
					{
						object[] assemblyQualifiedName = new object[1];
						assemblyQualifiedName[0] = type.AssemblyQualifiedName;
						throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.CollectionSeralizationFailedNotValidCollectionType, assemblyQualifiedName));
					}
					else
					{
						return this.SerializeEnumerable(clrObject as IEnumerable, resourceType, depth);
					}
				}
				else
				{
					return this.SerializeDictionary(clrObject as IDictionary, resourceType, depth);
				}
			}
			else
			{
				return this.SerializeEnumerable(null, resourceType, depth);
			}
		}

		internal IEnumerable SerializeDictionary(IDictionary dictionary, CollectionResourceType collectionResourceType, int depth)
		{
			ResourceType itemType = collectionResourceType.ItemType;
			Type[] instanceType = new Type[1];
			instanceType[0] = itemType.InstanceType;
			Type[] typeArray = instanceType;
			Type type = typeof(List<>);
			Type type1 = type.MakeGenericType(typeArray);
			IList lists = (IList)Activator.CreateInstance(type1);
			foreach (object key in dictionary.Keys)
			{
				KeyValuePair<object, object> keyValuePair = new KeyValuePair<object, object>(key, dictionary[key]);
				lists.Add(SerializerBase.SerializeResourceType(keyValuePair, itemType, depth));
			}
			return lists;
		}

		internal IEnumerable SerializeEnumerable(IEnumerable clrObjects, CollectionResourceType collectionResourceType, int depth)
		{
			ResourceType itemType = collectionResourceType.ItemType;
			Type[] instanceType = new Type[1];
			instanceType[0] = itemType.InstanceType;
			Type[] typeArray = instanceType;
			Type type = typeof(List<>);
			Type type1 = type.MakeGenericType(typeArray);
			IList lists = (IList)Activator.CreateInstance(type1);
			if (clrObjects != null)
			{
				foreach (object clrObject in clrObjects)
				{
					lists.Add(SerializerBase.SerializeResourceType(clrObject, itemType, depth));
				}
			}
			return lists;
		}
	}
}