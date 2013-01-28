using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.Schema;
using Microsoft.Management.Odata.Tracing;
using System;
using System.Data.Services.Providers;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.Odata.PS
{
	internal class EntityTypeSerializer : SerializerBase
	{
		private bool serializeKeyOnly;

		public PSEntityMetadata TestHookEntityMetadata
		{
			get;
			set;
		}

		public EntityTypeSerializer(ResourceType resourceType, bool serializeKeyOnly = false) : base(resourceType)
		{
			object[] resourceTypeKind = new object[2];
			resourceTypeKind[0] = resourceType.ResourceTypeKind;
			resourceTypeKind[1] = ResourceTypeKind.EntityType;
			ExceptionHelpers.ThrowArgumentExceptionIf("resourceType", resourceType.ResourceTypeKind != ResourceTypeKind.EntityType, new ExceptionHelpers.MessageLoader(SerializerBase.GetInvalidArgMessage), resourceTypeKind);
			this.serializeKeyOnly = serializeKeyOnly;
			this.TestHookEntityMetadata = null;
		}

		public override object Serialize(object clrObject, int depth)
		{
			object value;
			object[] name = new object[1];
			name[0] = base.ResourceType.Name;
			clrObject.ThrowIfNull("clrObject", new ParameterExtensions.MessageLoader(SerializerBase.GetNullPassedForSerializingEntityMessage), name);
			ResourceType resourceType = base.ResourceType;
			if (clrObject as PSObject == null)
			{
				resourceType = base.ResourceType.FindResourceType(clrObject.GetType());
			}
			else
			{
				PSObject pSObject = clrObject as PSObject;
				if (pSObject != null && pSObject.BaseObject != null)
				{
					resourceType = base.ResourceType.FindResourceType(pSObject.BaseObject.GetType());
				}
			}
			DSResource dSResource = new DSResource(resourceType, this.serializeKeyOnly);
			foreach (ResourceProperty property in resourceType.Properties)
			{
				if (this.serializeKeyOnly && (property.Kind & ResourcePropertyKind.Key) != ResourcePropertyKind.Key)
				{
					continue;
				}
				if ((property.Kind & ResourcePropertyKind.ResourceSetReference) == ResourcePropertyKind.ResourceSetReference)
				{
					PSEntityMetadata testHookEntityMetadata = this.TestHookEntityMetadata;
					if (testHookEntityMetadata == null)
					{
						DataContext currentContext = DataServiceController.Current.GetCurrentContext();
						if (currentContext != null)
						{
							testHookEntityMetadata = currentContext.UserSchema.GetEntityMetadata(base.ResourceType) as PSEntityMetadata;
						}
					}
					if (testHookEntityMetadata != null)
					{
						PSEntityMetadata.ReferenceSetCmdlets referenceSetCmdlet = null;
						if (testHookEntityMetadata.CmdletsForReferenceSets.TryGetValue(property.Name, out referenceSetCmdlet) && referenceSetCmdlet.Cmdlets.ContainsKey(CommandType.GetReference))
						{
							if (referenceSetCmdlet.GetRefHidden)
							{
								dSResource.SetValue(property.Name, null);
								continue;
							}
							else
							{
								PSReferencedResourceSet pSReferencedResourceSet = new PSReferencedResourceSet(property, base.ResourceType);
								dSResource.SetValue(property.Name, pSReferencedResourceSet);
								continue;
							}
						}
					}
				}
				if (clrObject != null)
				{
					value = SerializerBase.GetValue(property, clrObject);
				}
				else
				{
					value = null;
				}
				object obj = value;
				if (obj == null)
				{
					if (!property.ResourceType.IsPrimitive() || property.ResourceType.IsNullable())
					{
						if ((property.Kind & (ResourcePropertyKind.Primitive | ResourcePropertyKind.ResourceReference)) != 0)
						{
							Tracer tracer = new Tracer();
							tracer.DebugMessage(string.Concat(property.Name, " is null; skipping"));
							continue;
						}
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = property.Name;
						throw new PSObjectSerializationFailedException(string.Format(CultureInfo.CurrentCulture, Resources.PropertyNotFoundInPSObject, objArray));
					}
				}
				dSResource.SetValue(property.Name, SerializerBase.SerializeResourceProperty(obj, base.ResourceType, property, depth));
			}
			return dSResource;
		}
	}
}