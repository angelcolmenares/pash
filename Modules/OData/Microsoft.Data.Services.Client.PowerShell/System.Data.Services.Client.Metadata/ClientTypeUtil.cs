namespace System.Data.Services.Client.Metadata
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal static class ClientTypeUtil
    {
        internal static readonly PropertyInfo[] EmptyPropertyInfoArray = new PropertyInfo[0];

        internal static bool CanAssignNull(Type type)
        {
            return (!type.IsValueTypeEx() || (type.IsGenericTypeEx() && (type.GetGenericTypeDefinition() == typeof(Nullable<>))));
        }

        internal static string FullName(this IEdmType edmType)
        {
            IEdmSchemaElement element = edmType as IEdmSchemaElement;
            if (element != null)
            {
                return element.FullName();
            }
            return null;
        }

        internal static MethodInfo GetAddToCollectionMethod(Type collectionType, out Type type)
        {
            return GetMethodForGenericType(collectionType, typeof(ICollection<>), "Add", out type);
        }

        internal static ClientPropertyAnnotation GetClientPropertyAnnotation(this IEdmModel model, IEdmProperty edmProperty)
        {
            return model.GetAnnotationValue<ClientPropertyAnnotation>(edmProperty);
        }

        internal static ClientTypeAnnotation GetClientTypeAnnotation(this IEdmModel model, IEdmProperty edmProperty)
        {
            IEdmType definition = edmProperty.Type.Definition;
            return model.GetAnnotationValue<ClientTypeAnnotation>(definition);
        }

        internal static ClientTypeAnnotation GetClientTypeAnnotation(this IEdmModel model, IEdmType edmType)
        {
            return model.GetAnnotationValue<ClientTypeAnnotation>(edmType);
        }

        internal static ClientTypeAnnotation GetClientTypeAnnotation(this ClientEdmModel model, Type type)
        {
            IEdmType orCreateEdmType = model.GetOrCreateEdmType(type);
            return model.GetClientTypeAnnotation(orCreateEdmType);
        }

        internal static Type GetImplementationType(Type type, Type genericTypeDefinition)
        {
            if (IsConstructedGeneric(type, genericTypeDefinition))
            {
                return type;
            }
            Type type2 = null;
            foreach (Type type3 in type.GetInterfaces())
            {
                if (IsConstructedGeneric(type3, genericTypeDefinition))
                {
                    if (null != type2)
                    {
                        throw System.Data.Services.Client.Error.NotSupported(System.Data.Services.Client.Strings.ClientType_MultipleImplementationNotSupported);
                    }
                    type2 = type3;
                }
            }
            return type2;
        }

        internal static PropertyInfo[] GetKeyPropertiesOnType(Type type)
        {
            bool flag;
            return GetKeyPropertiesOnType(type, out flag);
        }

        internal static PropertyInfo[] GetKeyPropertiesOnType(Type type, out bool hasProperties)
        {
            Func<string, bool> predicate = null;
            if (CommonUtil.IsUnsupportedType(type))
            {
                throw new InvalidOperationException(System.Data.Services.Client.Strings.ClientType_UnsupportedType(type));
            }
            string str = type.ToString();
            IEnumerable<object> customAttributes = type.GetCustomAttributes(true);
            bool flag = customAttributes.OfType<DataServiceEntityAttribute>().Any<DataServiceEntityAttribute>();
            DataServiceKeyAttribute dataServiceKeyAttribute = customAttributes.OfType<DataServiceKeyAttribute>().FirstOrDefault<DataServiceKeyAttribute>();
            List<PropertyInfo> list = new List<PropertyInfo>();
            PropertyInfo[] properties = GetPropertiesOnType(type, false).ToArray<PropertyInfo>();
            hasProperties = properties.Length > 0;
            KeyKind notKey = KeyKind.NotKey;
            KeyKind kind2 = KeyKind.NotKey;
            foreach (PropertyInfo info in properties)
            {
                if ((kind2 = IsKeyProperty(info, dataServiceKeyAttribute)) != KeyKind.NotKey)
                {
                    if (kind2 > notKey)
                    {
                        list.Clear();
                        notKey = kind2;
                        list.Add(info);
                    }
                    else if (kind2 == notKey)
                    {
                        list.Add(info);
                    }
                }
            }
            Type declaringType = null;
            foreach (PropertyInfo info2 in list)
            {
                if (null == declaringType)
                {
                    declaringType = info2.DeclaringType;
                }
                else if (declaringType != info2.DeclaringType)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_KeysOnDifferentDeclaredType(str));
                }
                if (!PrimitiveType.IsKnownType(info2.PropertyType))
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_KeysMustBeSimpleTypes(str));
                }
            }
            if ((kind2 == KeyKind.AttributedKey) && (list.Count != dataServiceKeyAttribute.KeyNames.Count))
            {
                if (predicate == null)
                {
                    predicate = a => null == (from b in properties
                        where b.Name == a
                        select b).FirstOrDefault<PropertyInfo>();
                }
                string str2 = dataServiceKeyAttribute.KeyNames.Cast<string>().Where<string>(predicate).First<string>();
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_MissingProperty(str, str2));
            }
            if (list.Count > 0)
            {
                return list.ToArray();
            }
            if (!flag)
            {
                return null;
            }
            return EmptyPropertyInfoArray;
        }

        internal static Type GetMemberType(MemberInfo member)
        {
            PropertyInfo info = member as PropertyInfo;
            if (info != null)
            {
                return info.PropertyType;
            }
            FieldInfo info2 = member as FieldInfo;
            return info2.FieldType;
        }

        internal static MethodInfo GetMethodForGenericType(Type propertyType, Type genericTypeDefinition, string methodName, out Type type)
        {
            type = null;
            Type implementationType = GetImplementationType(propertyType, genericTypeDefinition);
            if (null != implementationType)
            {
                Type[] genericArguments = implementationType.GetGenericArguments();
                MethodInfo method = implementationType.GetMethod(methodName);
                type = genericArguments[genericArguments.Length - 1];
                return method;
            }
            return null;
        }

        internal static IEnumerable<PropertyInfo> GetPropertiesOnType(Type type, bool declaredOnly)
        {
            type.ToString();
            if (!PrimitiveType.IsKnownType(type))
            {
                foreach (PropertyInfo iteratorVariable0 in type.GetPublicPropertiesEx(true, declaredOnly))
                {
                    Type propertyType = iteratorVariable0.PropertyType;
                    propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                    if (((!propertyType.IsPointer && ((!propertyType.IsArray || (typeof(byte[]) == propertyType)) || (typeof(char[]) == propertyType))) && (((typeof(IntPtr) != propertyType) && (typeof(UIntPtr) != propertyType)) && (!declaredOnly || !IsOverride(type, iteratorVariable0)))) && ((iteratorVariable0.CanRead && (!propertyType.IsValueType() || iteratorVariable0.CanWrite)) && (!propertyType.ContainsGenericParametersEx() && (iteratorVariable0.GetIndexParameters().Length == 0))))
                    {
                        yield return iteratorVariable0;
                    }
                }
            }
        }

        private static bool IsConstructedGeneric(Type type, Type genericTypeDefinition)
        {
            return ((type.IsGenericTypeEx() && (type.GetGenericTypeDefinition() == genericTypeDefinition)) && !type.ContainsGenericParametersEx());
        }

        private static KeyKind IsKeyProperty(PropertyInfo propertyInfo, DataServiceKeyAttribute dataServiceKeyAttribute)
        {
            string name = propertyInfo.Name;
            KeyKind notKey = KeyKind.NotKey;
            if ((dataServiceKeyAttribute != null) && dataServiceKeyAttribute.KeyNames.Contains(name))
            {
                return KeyKind.AttributedKey;
            }
            if (name.EndsWith("ID", StringComparison.Ordinal))
            {
                string str2 = propertyInfo.DeclaringType.Name;
                if ((name.Length == (str2.Length + 2)) && name.StartsWith(str2, StringComparison.Ordinal))
                {
                    return KeyKind.TypeNameId;
                }
                if (2 == name.Length)
                {
                    notKey = KeyKind.Id;
                }
            }
            return notKey;
        }

        private static bool IsOverride(Type type, PropertyInfo propertyInfo)
        {
            MethodInfo getMethod = propertyInfo.GetGetMethod();
            return ((getMethod != null) && (getMethod.GetBaseDefinition().DeclaringType != type));
        }

        internal static void SetClientPropertyAnnotation(this IEdmProperty edmProperty, ClientPropertyAnnotation annotation)
        {
            ClientEdmModel.GetModel(annotation.MaxProtocolVersion).SetAnnotationValue<ClientPropertyAnnotation>(edmProperty, annotation);
        }

        internal static void SetClientTypeAnnotation(this IEdmType edmType, ClientTypeAnnotation annotation)
        {
            ClientEdmModel.GetModel(annotation.MaxProtocolVersion).SetAnnotationValue<ClientTypeAnnotation>(edmType, annotation);
        }

        internal static IEdmTypeReference ToEdmTypeReference(this IEdmType edmType, bool isNullable)
        {
            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Primitive:
                    return new EdmPrimitiveTypeReference((IEdmPrimitiveType) edmType, isNullable);

                case EdmTypeKind.Entity:
                    return new EdmEntityTypeReference((IEdmEntityType) edmType, true);

                case EdmTypeKind.Complex:
                    return new EdmComplexTypeReference((IEdmComplexType) edmType, true);

                case EdmTypeKind.Row:
                    return new EdmRowTypeReference((IEdmRowType) edmType, true);

                case EdmTypeKind.Collection:
                    return new EdmCollectionTypeReference((IEdmCollectionType) edmType, false);

                case EdmTypeKind.EntityReference:
                    return new EdmEntityReferenceTypeReference((IEdmEntityReferenceType) edmType, true);
            }
            return null;
        }

        internal static bool TypeIsEntity(Type t, DataServiceProtocolVersion maxProtocolVersion)
        {
            return (ClientEdmModel.GetModel(maxProtocolVersion).GetOrCreateEdmType(t).TypeKind == EdmTypeKind.Entity);
        }

        internal static bool TypeOrElementTypeIsEntity(Type type)
        {
            type = TypeSystem.GetElementType(type);
            type = Nullable.GetUnderlyingType(type) ?? type;
            return (!PrimitiveType.IsKnownType(type) && (GetKeyPropertiesOnType(type) != null));
        }

        

        private enum KeyKind
        {
            NotKey,
            Id,
            TypeNameId,
            AttributedKey
        }
    }
}

