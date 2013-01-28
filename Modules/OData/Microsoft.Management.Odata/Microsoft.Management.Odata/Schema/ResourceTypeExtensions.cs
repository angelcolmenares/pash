using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.PS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Providers;
using System.Linq;

namespace Microsoft.Management.Odata.Schema
{
	internal static class ResourceTypeExtensions
	{
		public static void AddDerivedType(this ResourceType resourceType, ResourceType derivedType)
		{
			object[] name = new object[1];
			name[0] = resourceType.Name;
			derivedType.ThrowIfNull("derivedType", Resources.NullPassedAsDerivedType, name);
			object[] fullName = new object[2];
			fullName[0] = resourceType.FullName;
			fullName[1] = derivedType.FullName;
			ExceptionHelpers.ThrowArgumentExceptionIf("derivedType", derivedType.BaseType != resourceType, Resources.NotBaseResourceType, fullName);
			(resourceType.CustomState as ResourceCustomState).DerivedTypes.Add(derivedType);
		}

		public static DSResource CreateKeyOnlyResource(ResourceType resourceType, Dictionary<string, object> inputKeys)
		{
			DSResource dSResource;
			ReadOnlyCollection<ResourceProperty> properties = resourceType.Properties;
			IEnumerable<ResourceProperty> resourceProperties = properties.Where<ResourceProperty>((ResourceProperty it) => (it.Kind & ResourcePropertyKind.Key) == ResourcePropertyKind.Key);
			IEnumerable<string> strs = resourceProperties.Select<ResourceProperty, string>((ResourceProperty it) => it.Name);
			if (inputKeys.Count == strs.Count<string>())
			{
				DSResource dSResource1 = new DSResource(resourceType, true);
				Dictionary<string, object>.Enumerator enumerator = inputKeys.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, object> current = enumerator.Current;
						if (strs.Contains<string>(current.Key))
						{
							dSResource1.SetValue(current.Key, current.Value);
						}
						else
						{
							TraceHelper.Current.DebugMessage(string.Concat("CreateKeyOnlyResource: Returning null. Property ", current.Key, " is not key in the resource ", resourceType.Name));
							dSResource = null;
							return dSResource;
						}
					}
					return dSResource1;
				}
				finally
				{
					enumerator.Dispose();
				}
				return dSResource;
			}
			else
			{
				object[] name = new object[4];
				name[0] = "CreateKeyOnlyResource: Number of keys of ResourceType and inside properties does not match. Returning null. \nResource type name ";
				name[1] = resourceType.Name;
				name[2] = "\nInput Properties count: ";
				name[3] = inputKeys.Count;
				TraceHelper.Current.DebugMessage(string.Concat(name));
				return null;
			}
		}

		public static DSResource CreateResourceWithKeyAndReferenceSetCmdlets(ResourceType resourceType, Dictionary<string, object> keyProperties, EntityMetadata entityMetadata)
		{
			DSResource dSResource = ResourceTypeExtensions.CreateKeyOnlyResource(resourceType, keyProperties);
			if (dSResource != null)
			{
				PSEntityMetadata pSEntityMetadatum = entityMetadata as PSEntityMetadata;
				ReadOnlyCollection<ResourceProperty> properties = resourceType.Properties;
				foreach (ResourceProperty resourceProperty in properties.Where<ResourceProperty>((ResourceProperty it) => (it.Kind & ResourcePropertyKind.ResourceSetReference) == ResourcePropertyKind.ResourceSetReference))
				{
					PSEntityMetadata.ReferenceSetCmdlets referenceSetCmdlet = null;
					if (!pSEntityMetadatum.CmdletsForReferenceSets.TryGetValue(resourceProperty.Name, out referenceSetCmdlet) || !referenceSetCmdlet.Cmdlets.ContainsKey(CommandType.GetReference))
					{
						continue;
					}
					if (referenceSetCmdlet.GetRefHidden)
					{
						dSResource.SetValue(resourceProperty.Name, null);
					}
					else
					{
						PSReferencedResourceSet pSReferencedResourceSet = new PSReferencedResourceSet(resourceProperty, resourceType);
						dSResource.SetValue(resourceProperty.Name, pSReferencedResourceSet);
					}
				}
				return dSResource;
			}
			else
			{
				return null;
			}
		}

		public static bool DoesClrTypeMatch(this ResourceType resourceType, Type clrType)
		{
			ResourceCustomState customState = resourceType.CustomState as ResourceCustomState;
			if (customState != null)
			{
				if (string.Equals(customState.ClrTypeString, clrType.AssemblyQualifiedName) || string.Equals(customState.ClrTypeString, clrType.FullName))
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

		public static ResourceType FindResourceType(this ResourceType resourceType, Type clrType)
		{
			ResourceType resourceType1;
			if (!resourceType.DoesClrTypeMatch(clrType))
			{
				ResourceCustomState customState = resourceType.CustomState as ResourceCustomState;
				if (customState != null)
				{
					HashSet<ResourceType>.Enumerator enumerator = customState.DerivedTypes.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							ResourceType current = enumerator.Current;
							if (!current.DoesClrTypeMatch(clrType))
							{
								continue;
							}
							resourceType1 = current;
							return resourceType1;
						}
						return resourceType;
					}
					finally
					{
						enumerator.Dispose();
					}
					return resourceType1;
				}
				else
				{
					return resourceType;
				}
			}
			else
			{
				return resourceType;
			}
		}

		public static HashSet<ResourceType> GetAllDependencies(this ResourceType resourceType)
		{
			HashSet<ResourceType> resourceTypes = new HashSet<ResourceType>();
			ResourceTypeExtensions.GetAllDependencies(resourceType, resourceTypes);
			return resourceTypes;
		}

		private static void GetAllDependencies(ResourceType resourceType, HashSet<ResourceType> dependencies)
		{
			foreach (ResourceProperty property in resourceType.Properties)
			{
				ResourceType itemType = null;
				if (property.ResourceType.ResourceTypeKind != ResourceTypeKind.ComplexType)
				{
					if (property.ResourceType.ResourceTypeKind == ResourceTypeKind.Collection)
					{
						CollectionResourceType collectionResourceType = property.ResourceType as CollectionResourceType;
						if (collectionResourceType.ItemType.ResourceTypeKind != ResourceTypeKind.Primitive)
						{
							itemType = collectionResourceType.ItemType;
						}
					}
				}
				else
				{
					itemType = property.ResourceType;
				}
				if (itemType == null || dependencies.Any<ResourceType>((ResourceType item) => item.FullName == itemType.FullName))
				{
					continue;
				}
				dependencies.Add(itemType);
				ResourceTypeExtensions.GetAllDependencies(itemType, dependencies);
			}
		}

		public static string GetClrTypeStr(this ResourceType resourceType)
		{
			return (resourceType.CustomState as ResourceCustomState).ClrTypeString;
		}

		public static HashSet<ResourceType> GetDerivedTypes(this ResourceType resourceType)
		{
			ResourceCustomState customState = resourceType.CustomState as ResourceCustomState;
			if (customState == null)
			{
				return new HashSet<ResourceType>();
			}
			else
			{
				return customState.DerivedTypes;
			}
		}

		private static void GetDescendants(ResourceType resourceType, HashSet<ResourceType> familyResources)
		{
			familyResources.Add(resourceType);
			foreach (ResourceType derivedType in resourceType.GetDerivedTypes())
			{
				Func<ResourceType, bool> func = null;
				HashSet<ResourceType> resourceTypes = familyResources;
				if (func == null)
				{
					func = (ResourceType item) => item.FullName == derivedType.FullName;
				}
				if (resourceTypes.Any<ResourceType>(func))
				{
					continue;
				}
				ResourceTypeExtensions.GetDescendants(derivedType, familyResources);
			}
		}

		public static HashSet<ResourceType> GetFamily(this ResourceType resourceType)
		{
			ResourceType topmostAncestor = ResourceTypeExtensions.GetTopmostAncestor(resourceType);
			HashSet<ResourceType> resourceTypes = new HashSet<ResourceType>();
			ResourceTypeExtensions.GetDescendants(topmostAncestor, resourceTypes);
			return resourceTypes;
		}

		public static ResourceType GetTopmostAncestor(ResourceType resourceType)
		{
			ResourceType baseType = resourceType;
			while (baseType.BaseType != null)
			{
				baseType = baseType.BaseType;
			}
			return baseType;
		}

		public static bool IsNullable(this ResourceType resourceType)
		{
			if (resourceType.ResourceTypeKind != ResourceTypeKind.Primitive)
			{
				return false;
			}
			else
			{
				return TypeSystem.IsNullableType(resourceType.InstanceType);
			}
		}

		public static bool IsPrimitive(this ResourceType resourceType)
		{
			return resourceType.ResourceTypeKind == ResourceTypeKind.Primitive;
		}

		public static bool IsSameOrBaseType(this ResourceType resourceType, ResourceType otherType)
		{
			if (resourceType != otherType)
			{
				ResourceType baseType = resourceType.BaseType;
				while (baseType != null)
				{
					if (otherType != baseType)
					{
						baseType = baseType.BaseType;
					}
					else
					{
						return true;
					}
				}
				return false;
			}
			else
			{
				return true;
			}
		}

		public static void SetClrTypeStr(this ResourceType resourceType, string type)
		{
			(resourceType.CustomState as ResourceCustomState).ClrTypeString = type;
		}
	}
}