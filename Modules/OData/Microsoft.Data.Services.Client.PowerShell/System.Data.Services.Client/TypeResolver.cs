namespace System.Data.Services.Client
{
    using Microsoft.Data.Edm;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Runtime.CompilerServices;

    internal class TypeResolver
    {
        private readonly IDictionary<string, ClientTypeAnnotation> edmTypeNameMap = new Dictionary<string, ClientTypeAnnotation>(StringComparer.Ordinal);
        private readonly bool projectionQuery;

        internal TypeResolver(System.Data.Services.Client.ResponseInfo responseInfo, bool projection)
        {
            this.ResponseInfo = responseInfo;
            this.projectionQuery = projection;
        }

        internal ClientTypeAnnotation ResolveEdmTypeName(Type expectedType, string edmTypeName)
        {
            ClientTypeAnnotation clientTypeAnnotation;
            PrimitiveType type;
            string collectionItemWireTypeName = WebUtil.GetCollectionItemWireTypeName(edmTypeName);
            string str2 = collectionItemWireTypeName ?? edmTypeName;
            ClientEdmModel model = ClientEdmModel.GetModel(this.ResponseInfo.MaxProtocolVersion);
            if (PrimitiveType.TryGetPrimitiveType(str2, out type))
            {
                clientTypeAnnotation = model.GetClientTypeAnnotation(type.ClrType);
            }
            else if (!this.edmTypeNameMap.TryGetValue(str2, out clientTypeAnnotation))
            {
                clientTypeAnnotation = model.GetClientTypeAnnotation(str2);
            }
            if (collectionItemWireTypeName == null)
            {
                return clientTypeAnnotation;
            }
            Type elementType = clientTypeAnnotation.ElementType;
            if (type != null)
            {
                elementType = ClientTypeUtil.GetImplementationType(expectedType, typeof(ICollection<>)).GetGenericArguments()[0];
            }
            Type backingTypeForCollectionProperty = WebUtil.GetBackingTypeForCollectionProperty(expectedType, elementType);
            return model.GetClientTypeAnnotation(backingTypeForCollectionProperty);
        }

        private Type ResolveTypeFromName(string wireName, Type expectedType)
        {
            Type type;
            if (!ClientConvert.ToNamedType(wireName, out type))
            {
                type = this.ResponseInfo.ResolveTypeFromName(wireName);
                if (type == null)
                {
                    type = ClientTypeCache.ResolveFromName(wireName, expectedType);
                }
                if ((!this.projectionQuery && (type != null)) && !expectedType.IsAssignableFrom(type))
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_Current(expectedType, type));
                }
            }
            return (type ?? expectedType);
        }

        internal IEdmType ResolveWireTypeName(IEdmType expectedEdmType, string wireName)
        {
            Type elementType;
            ClientEdmModel model = ClientEdmModel.GetModel(this.ResponseInfo.MaxProtocolVersion);
            if (expectedEdmType != null)
            {
                ClientTypeAnnotation annotation = model.GetClientTypeAnnotation(expectedEdmType);
                elementType = annotation.ElementType;
                if (annotation.EdmType.TypeKind == EdmTypeKind.Primitive)
                {
                    return expectedEdmType;
                }
            }
            else
            {
                elementType = typeof(object);
            }
            Type type2 = this.ResolveTypeFromName(wireName, elementType);
            ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(type2));
            if (clientTypeAnnotation.IsEntityType)
            {
                clientTypeAnnotation.EnsureEPMLoaded();
            }
            switch (clientTypeAnnotation.EdmType.TypeKind)
            {
                case EdmTypeKind.Entity:
                case EdmTypeKind.Complex:
                {
                    string key = clientTypeAnnotation.EdmType.FullName();
                    if (!this.edmTypeNameMap.ContainsKey(key))
                    {
                        this.edmTypeNameMap.Add(key, clientTypeAnnotation);
                    }
                    break;
                }
            }
            return clientTypeAnnotation.EdmType;
        }

        internal System.Data.Services.Client.ResponseInfo ResponseInfo { get; private set; }
    }
}

