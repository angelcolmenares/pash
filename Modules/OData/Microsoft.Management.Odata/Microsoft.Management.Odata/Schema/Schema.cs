using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq;
using System.Text;

namespace Microsoft.Management.Odata.Schema
{
	internal class Schema
	{
		public Dictionary<string, Schema.AssociationType> AssociationTypes
		{
			get;
			private set;
		}

		public string ContainerName
		{
			get;
			private set;
		}

		public string ContainerNamespace
		{
			get;
			private set;
		}

		public Dictionary<string, EntityMetadata> EntityMetadataDictionary
		{
			get;
			private set;
		}

		public Dictionary<string, ResourceSet> ResourceSets
		{
			get;
			private set;
		}

		public Dictionary<string, ResourceType> ResourceTypes
		{
			get;
			private set;
		}

		internal Schema(string containerName, string containerNamespace)
		{
			this.ContainerName = containerName;
			this.ContainerNamespace = containerNamespace;
			this.ResourceSets = new Dictionary<string, ResourceSet>();
			this.ResourceTypes = new Dictionary<string, ResourceType>();
			this.AssociationTypes = new Dictionary<string, Schema.AssociationType>();
			this.EntityMetadataDictionary = new Dictionary<string, EntityMetadata>();
		}

		internal Schema.AssociationType AddAssociationType(string name, string typeNamespace)
		{
			string str = string.Concat(typeNamespace, ".", name);
			Schema.AssociationType associationType = new Schema.AssociationType(str);
			this.AssociationTypes.Add(str, associationType);
			return associationType;
		}

		internal void AddComplexCollectionProperty(ResourceType resourceType, string name, ResourceType complexType)
		{
			CollectionResourceType collectionResourceType = ResourceType.GetCollectionResourceType(complexType);
			ResourceProperty resourcePropertyWithDescription = new ResourcePropertyWithDescription(name, ResourcePropertyKind.Collection, collectionResourceType);
			resourcePropertyWithDescription.CanReflectOnInstanceTypeProperty = false;
			PropertyCustomState propertyCustomState = new PropertyCustomState();
			resourcePropertyWithDescription.CustomState = propertyCustomState;
			resourceType.AddProperty(resourcePropertyWithDescription);
		}

		internal void AddComplexProperty(ResourceType resourceType, string name, ResourceType complexType)
		{
			if (complexType.ResourceTypeKind == ResourceTypeKind.ComplexType)
			{
				ResourceProperty resourcePropertyWithDescription = new ResourcePropertyWithDescription(name, ResourcePropertyKind.ComplexType, complexType);
				resourcePropertyWithDescription.CanReflectOnInstanceTypeProperty = false;
				PropertyCustomState propertyCustomState = new PropertyCustomState();
				resourcePropertyWithDescription.CustomState = propertyCustomState;
				resourceType.AddProperty(resourcePropertyWithDescription);
				return;
			}
			else
			{
				throw new InvalidResourceTypeException(complexType.Name, complexType.ResourceTypeKind.ToString(), ResourceTypeKind.ComplexType.ToString());
			}
		}

		internal ResourceType AddComplexType(string name)
		{
			return this.AddResourceType(name, ResourceTypeKind.ComplexType, this.ContainerNamespace, null, null);
		}

		internal ResourceType AddComplexType(string name, string typeNamespace, string clrType)
		{
			return this.AddResourceType(name, ResourceTypeKind.ComplexType, typeNamespace, null, clrType);
		}

		public void AddEntity(string entityName, bool includeEntitySet, Schema other)
		{
			this.ResourceTypes.Add(entityName, other.ResourceTypes[entityName]);
			if (includeEntitySet)
			{
				foreach (string key in other.ResourceSets.Keys)
				{
					if (other.ResourceSets[key].ResourceType.FullName != entityName)
					{
						continue;
					}
					this.ResourceSets.Add(key, other.ResourceSets[key]);
				}
			}
		}

		internal ResourceType AddEntityType(string name)
		{
			return this.AddResourceType(name, ResourceTypeKind.EntityType, this.ContainerNamespace, null, null);
		}

		internal ResourceType AddEntityType(string name, string typeNamespace, string clrType)
		{
			return this.AddResourceType(name, ResourceTypeKind.EntityType, typeNamespace, null, clrType);
		}

		internal void AddKeyProperty(ResourceType resourceType, string name, Type propertyType, object defaultValue)
		{
			if (resourceType.ResourceTypeKind == ResourceTypeKind.EntityType)
			{
				this.AddPrimitiveProperty(resourceType, name, propertyType, ResourcePropertyKind.Primitive | ResourcePropertyKind.Key, defaultValue);
				return;
			}
			else
			{
				throw new InvalidResourceTypeException(resourceType.Name, resourceType.ResourceTypeKind.ToString(), ResourceTypeKind.EntityType.ToString());
			}
		}

		internal void AddPrimitiveCollectionProperty(ResourceType resourceType, string name, Type propertyType, object defaultValue)
		{
			CollectionResourceType collectionResourceType = ResourceType.GetCollectionResourceType(ResourceType.GetPrimitiveResourceType(propertyType));
			ResourceProperty resourcePropertyWithDescription = new ResourcePropertyWithDescription(name, ResourcePropertyKind.Collection, collectionResourceType);
			resourcePropertyWithDescription.CanReflectOnInstanceTypeProperty = false;
			PropertyCustomState propertyCustomState = new PropertyCustomState();
			propertyCustomState.DefaultValue = defaultValue;
			resourcePropertyWithDescription.CustomState = propertyCustomState;
			resourceType.AddProperty(resourcePropertyWithDescription);
		}

		internal void AddPrimitiveProperty(ResourceType resourceType, string name, Type propertyType, bool isEtag, object defaultValue)
		{
			ResourcePropertyKind resourcePropertyKind = ResourcePropertyKind.Primitive;
			if (isEtag)
			{
				resourcePropertyKind = resourcePropertyKind | ResourcePropertyKind.ETag;
			}
			this.AddPrimitiveProperty(resourceType, name, propertyType, resourcePropertyKind, defaultValue);
		}

		internal void AddPrimitiveProperty(ResourceType resourceType, string name, Type propertyType, ResourcePropertyKind flags, object defaultValue)
		{
			if (flags == ResourcePropertyKind.Primitive || flags == (ResourcePropertyKind.Primitive | ResourcePropertyKind.ETag) || flags == (ResourcePropertyKind.Primitive | ResourcePropertyKind.Key))
			{
				ResourceType primitiveResourceType = ResourceType.GetPrimitiveResourceType(propertyType);
				ResourcePropertyKind resourcePropertyKind = ResourcePropertyKind.Primitive;
				resourcePropertyKind = resourcePropertyKind | flags;
				ResourceProperty resourcePropertyWithDescription = new ResourcePropertyWithDescription(name, resourcePropertyKind, primitiveResourceType);
				resourcePropertyWithDescription.CanReflectOnInstanceTypeProperty = false;
				PropertyCustomState propertyCustomState = new PropertyCustomState();
				propertyCustomState.DefaultValue = defaultValue;
				resourcePropertyWithDescription.CustomState = propertyCustomState;
				resourceType.AddProperty(resourcePropertyWithDescription);
				return;
			}
			else
			{
				throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.SchemaInvalidKeyOrEtagDiscrepancy, new object[0]), "flags");
			}
		}

		internal void AddResourceReferenceProperty(ResourceType sourceType, string name, ResourceType targetType, Schema.AssociationType assocType, object defaultValue)
		{
			ResourceProperty resourcePropertyWithDescription = new ResourcePropertyWithDescription(name, ResourcePropertyKind.ResourceReference, targetType);
			resourcePropertyWithDescription.CanReflectOnInstanceTypeProperty = false;
			resourcePropertyWithDescription.CustomState = new ReferenceCustomState(false);
			resourcePropertyWithDescription.GetReferenceCustomState().AssociationType = assocType;
			resourcePropertyWithDescription.GetCustomState().DefaultValue = defaultValue;
			sourceType.AddProperty(resourcePropertyWithDescription);
		}

		internal ResourceSet AddResourceSet(string name, ResourceType entityType)
		{
			name.ThrowIfNullOrEmpty("name", Resources.NullIsPassedAsResourceSetName, new object[0]);
			object[] objArray = new object[1];
			objArray[0] = name;
			entityType.ThrowIfNull("entityType", Resources.ParameterSetNull, objArray);
			if (entityType.ResourceTypeKind == ResourceTypeKind.EntityType)
			{
				ResourceSet resourceSet = new ResourceSet(name, entityType);
				if (!this.ResourceSets.Keys.Contains<string>(name))
				{
					this.ResourceSets.Add(name, resourceSet);
					return resourceSet;
				}
				else
				{
					object[] item = new object[3];
					item[0] = name;
					item[1] = entityType.Name;
					item[2] = this.ResourceSets[name];
					throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.ResourceSetNameDuplicated, item));
				}
			}
			else
			{
				throw new InvalidResourceTypeException(entityType.Name, entityType.ResourceTypeKind.ToString(), ResourceTypeKind.EntityType.ToString());
			}
		}

		internal void AddResourceSetReferenceProperty(ResourceType sourceType, string name, ResourceType targetType, Schema.AssociationType assocType)
		{
			ResourceProperty resourcePropertyWithDescription = new ResourcePropertyWithDescription(name, ResourcePropertyKind.ResourceSetReference, targetType);
			resourcePropertyWithDescription.CanReflectOnInstanceTypeProperty = false;
			resourcePropertyWithDescription.CustomState = new ReferenceCustomState(true);
			resourcePropertyWithDescription.GetReferenceCustomState().AssociationType = assocType;
			sourceType.AddProperty(resourcePropertyWithDescription);
		}

		internal ResourceType AddResourceType(string name, ResourceTypeKind kind, string typeNamespace, ResourceType baseType, string clrType = null)
		{
			name.ThrowIfNullOrEmpty("name", Resources.NullIsPassedAsResourceName, new object[0]);
			if (typeNamespace == null)
			{
				typeNamespace = this.ContainerNamespace;
			}
			ResourceType resourceTypeWithDescription = new ResourceTypeWithDescription(typeof(DSResource), kind, baseType, typeNamespace, name, false);
			resourceTypeWithDescription.CanReflectOnInstanceType = false;
			resourceTypeWithDescription.CustomState = new ResourceCustomState(clrType);
			this.ResourceTypes.Add(resourceTypeWithDescription.FullName, resourceTypeWithDescription);
			return resourceTypeWithDescription;
		}

		internal void FreezeSchema()
		{
			foreach (string key in this.ResourceTypes.Keys)
			{
				this.ResourceTypes[key].SetReadOnly();
			}
			foreach (string str in this.ResourceSets.Keys)
			{
				this.ResourceSets[str].SetReadOnly();
			}
		}

		public EntityMetadata GetEntityMetadata(ResourceType resourceType)
		{
			object[] name = new object[3];
			name[0] = resourceType.Name;
			name[1] = ResourceTypeKind.EntityType;
			name[2] = resourceType.ResourceTypeKind;
			ExceptionHelpers.ThrowArgumentExceptionIf("resourceType", resourceType.ResourceTypeKind != ResourceTypeKind.EntityType, Resources.ResourceTypeIsInvalid, name);
			return this.EntityMetadataDictionary[resourceType.FullName];
		}

		internal void PopulateAllRelevantResourceTypes(Schema other)
		{
			Action<ResourceType> action = null;
			Func<ResourceType, bool> func = null;
			Action<ResourceType> action1 = null;
			Func<ResourceType, bool> func1 = null;
			foreach (ResourceType list in this.ResourceTypes.Values.ToList<ResourceType>())
			{
				HashSet<ResourceType> allDependencies = list.GetAllDependencies();
				if (allDependencies != null)
				{
					HashSet<ResourceType> resourceTypes = allDependencies;
					if (func == null)
					{
						func = (ResourceType item) => !this.ResourceTypes.ContainsKey(item.FullName);
					}
					List<ResourceType> list1 = resourceTypes.Where<ResourceType>(func).ToList<ResourceType>();
					if (action == null)
					{
						action = (ResourceType item) => this.ResourceTypes.Add(item.FullName, item);
					}
					list1.ForEach(action);
				}
				HashSet<ResourceType> derivedTypes = list.GetDerivedTypes();
				if (derivedTypes == null)
				{
					continue;
				}
				HashSet<ResourceType> resourceTypes1 = derivedTypes;
				if (func1 == null)
				{
					func1 = (ResourceType item) => !this.ResourceTypes.ContainsKey(item.FullName);
				}
				List<ResourceType> list2 = resourceTypes1.Where<ResourceType>(func1).ToList<ResourceType>();
				if (action1 == null)
				{
					action1 = (ResourceType item) => this.ResourceTypes.Add(item.FullName, item);
				}
				list2.ForEach(action1);
			}
		}

		internal string ToTraceMessage(string message)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(message);
			stringBuilder.AppendLine(string.Concat("ResourceSets \nCount = ", this.ResourceSets.Count));
			this.ResourceSets.Values.ToList<ResourceSet>().ForEach((ResourceSet item) => stringBuilder = item.ToTraceMessage(stringBuilder));
			stringBuilder.AppendLine(string.Concat("ResourceTypes \nCount = ", this.ResourceTypes.Count));
			this.ResourceTypes.Values.ToList<ResourceType>().ForEach((ResourceType item) => stringBuilder = item.ToTraceMessage(stringBuilder));
			stringBuilder.AppendLine(string.Concat("EntityMetadata \nCount = ", this.EntityMetadataDictionary.Count));
			this.EntityMetadataDictionary.Keys.ToList<string>().ForEach((string item) => stringBuilder = this.EntityMetadataDictionary[item].ToTraceMessage(item, stringBuilder));
			return stringBuilder.ToString();
		}

		public void Trace(string message)
		{
			if (TraceHelper.IsEnabled(5))
			{
				TraceHelper.Current.DebugMessage(this.ToTraceMessage(message));
			}
		}

		public void ValidateResourceLimits(DSConfiguration configuration)
		{
			foreach (DSConfiguration.WcfConfigElement entitySet in configuration.DataServicesConfig.EntitySets)
			{
				if (!(entitySet.Name != "*") || this.ResourceSets.ContainsKey(entitySet.Name))
				{
					continue;
				}
				object[] name = new object[1];
				name[0] = entitySet.Name;
				throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.ConfigurationHasInvalidEntitySet, name));
			}
		}

		internal class AssociationEnd
		{
			public string Name
			{
				get;
				private set;
			}

			public ResourceProperty Property
			{
				get;
				set;
			}

			public ResourceType Type
			{
				get;
				private set;
			}

			public AssociationEnd(string name, ResourceType type)
			{
				this.Name = name;
				this.Type = type;
			}
		}

		internal class AssociationType
		{
			public List<Schema.AssociationEnd> Ends
			{
				get;
				private set;
			}

			public string Name
			{
				get;
				private set;
			}

			public List<ResourceAssociationSet> WcfClass
			{
				get;
				private set;
			}

			public AssociationType(string name)
			{
				this.Name = name;
				this.Ends = new List<Schema.AssociationEnd>();
				this.WcfClass = new List<ResourceAssociationSet>();
			}

			public void AddEnds(Schema.AssociationEnd end1, Schema.AssociationEnd end2)
			{
				if (this.Ends.Count == 0)
				{
					this.Ends.Add(end1);
					this.Ends.Add(end2);
					return;
				}
				else
				{
					throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.AssociationMoreThanTwoEndNotSupported, new object[0]));
				}
			}

			public void CreateWcfType(Dictionary<string, ResourceSet> allSets)
			{
				ResourceType type = this.Ends[0].Type;
				ResourceType resourceType = this.Ends[1].Type;
				char[] chrArray = new char[1];
				chrArray[0] = '.';
				string[] strArrays = this.Name.Split(chrArray);
				string str = strArrays[(int)strArrays.Length - 1];
				ResourceSet resourceSet = allSets.Values.FirstOrDefault<ResourceSet>((ResourceSet it) => it.ResourceType == type);
				ResourceSet resourceSet1 = allSets.Values.FirstOrDefault<ResourceSet>((ResourceSet it) => it.ResourceType == resourceType);
				if (resourceSet == null)
				{
					foreach (ResourceSet value in allSets.Values)
					{
						if (!type.IsSameOrBaseType(value.ResourceType))
						{
							continue;
						}
						resourceSet = value;
						break;
					}
				}
				if (resourceSet1 == null)
				{
					foreach (ResourceSet value1 in allSets.Values)
					{
						if (!resourceType.IsSameOrBaseType(value1.ResourceType))
						{
							continue;
						}
						resourceSet1 = value1;
						break;
					}
				}
				if (resourceSet != null && resourceSet1 != null)
				{
					ResourceAssociationSet resourceAssociationSet = new ResourceAssociationSet(str, new ResourceAssociationSetEnd(resourceSet, type, this.Ends[0].Property), new ResourceAssociationSetEnd(resourceSet1, resourceType, this.Ends[1].Property));
					this.WcfClass.Add(resourceAssociationSet);
				}
			}
		}
	}
}