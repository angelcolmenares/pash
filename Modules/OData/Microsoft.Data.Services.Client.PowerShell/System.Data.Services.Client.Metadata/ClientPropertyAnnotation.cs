namespace System.Data.Services.Client.Metadata
{
    using Microsoft.Data.Edm;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Spatial;

    [DebuggerDisplay("{PropertyName}")]
    internal sealed class ClientPropertyAnnotation
    {
        private readonly Action<object, object> collectionAdd;
        private readonly Action<object> collectionClear;
        private readonly Func<object, object, bool> collectionContains;
        private readonly Type collectionGenericType;
        private readonly Func<object, object, bool> collectionRemove;
        internal readonly Type DeclaringClrType;
        private readonly Action<object, string, object> dictionarySetter;
        internal readonly Type DictionaryValueType;
        internal readonly IEdmProperty EdmProperty;
        internal readonly bool IsKnownType;
        private bool? isPrimitiveOrComplexCollection;
        private bool? isSpatialType;
        internal readonly DataServiceProtocolVersion MaxProtocolVersion;
        private ClientPropertyAnnotation mimeTypeProperty;
        internal readonly Type NullablePropertyType;
        private readonly Func<object, object> propertyGetter;
        internal readonly string PropertyName;
        private readonly Action<object, object> propertySetter;
        internal readonly Type PropertyType;

        internal ClientPropertyAnnotation(IEdmProperty edmProperty, PropertyInfo propertyInfo, DataServiceProtocolVersion maxProtocolVersion)
        {
            ParameterExpression expression;
            ParameterExpression expression2;
            this.EdmProperty = edmProperty;
            this.PropertyName = propertyInfo.Name;
            this.NullablePropertyType = propertyInfo.PropertyType;
            this.PropertyType = Nullable.GetUnderlyingType(this.NullablePropertyType) ?? this.NullablePropertyType;
            this.DeclaringClrType = propertyInfo.DeclaringType;
            MethodInfo getMethod = propertyInfo.GetGetMethod();
            MethodInfo setMethod = propertyInfo.GetSetMethod();
            this.propertyGetter = (getMethod == null) ? null : ((Func<object, object>) Expression.Lambda(Expression.Convert(Expression.Call(Expression.Convert(expression = Expression.Parameter(typeof(object), "instance"), this.DeclaringClrType), getMethod), typeof(object)), new ParameterExpression[] { expression }).Compile());
            this.propertySetter = (setMethod == null) ? null : ((Action<object, object>) Expression.Lambda(Expression.Call(Expression.Convert(expression, this.DeclaringClrType), setMethod, new Expression[] { Expression.Convert(expression2 = Expression.Parameter(typeof(object), "value"), this.NullablePropertyType) }), new ParameterExpression[] { expression, expression2 }).Compile());
            this.MaxProtocolVersion = maxProtocolVersion;
            this.IsKnownType = PrimitiveType.IsKnownType(this.PropertyType);
            if (!this.IsKnownType)
            {
                MethodInfo method = ClientTypeUtil.GetMethodForGenericType(this.PropertyType, typeof(IDictionary<,>), "set_Item", out this.DictionaryValueType);
                if (method != null)
                {
                    ParameterExpression expression3;
                    this.dictionarySetter = (Action<object, string, object>) Expression.Lambda(Expression.Call(Expression.Convert(expression, typeof(IDictionary<,>).MakeGenericType(new Type[] { typeof(string), this.DictionaryValueType })), method, expression3 = Expression.Parameter(typeof(string), "propertyName"), Expression.Convert(expression2, this.DictionaryValueType)), new ParameterExpression[] { expression, expression3, expression2 }).Compile();
                }
                else
                {
                    MethodInfo info4 = ClientTypeUtil.GetMethodForGenericType(this.PropertyType, typeof(ICollection<>), "Contains", out this.collectionGenericType);
                    MethodInfo addToCollectionMethod = ClientTypeUtil.GetAddToCollectionMethod(this.PropertyType, out this.collectionGenericType);
                    MethodInfo info6 = ClientTypeUtil.GetMethodForGenericType(this.PropertyType, typeof(ICollection<>), "Remove", out this.collectionGenericType);
                    MethodInfo info7 = ClientTypeUtil.GetMethodForGenericType(this.PropertyType, typeof(ICollection<>), "Clear", out this.collectionGenericType);
                    this.collectionContains = (info4 == null) ? null : ((Func<object, object, bool>) Expression.Lambda(Expression.Call(Expression.Convert(expression, this.PropertyType), info4, new Expression[] { Expression.Convert(expression2, this.collectionGenericType) }), new ParameterExpression[] { expression, expression2 }).Compile());
                    this.collectionAdd = (addToCollectionMethod == null) ? null : ((Action<object, object>) Expression.Lambda(Expression.Call(Expression.Convert(expression, this.PropertyType), addToCollectionMethod, new Expression[] { Expression.Convert(expression2, this.collectionGenericType) }), new ParameterExpression[] { expression, expression2 }).Compile());
                    this.collectionRemove = (info6 == null) ? null : ((Func<object, object, bool>) Expression.Lambda(Expression.Call(Expression.Convert(expression, this.PropertyType), info6, new Expression[] { Expression.Convert(expression2, this.collectionGenericType) }), new ParameterExpression[] { expression, expression2 }).Compile());
                    this.collectionClear = (info7 == null) ? null : ((Action<object>) Expression.Lambda(Expression.Call(Expression.Convert(expression, this.PropertyType), info7), new ParameterExpression[] { expression }).Compile());
                }
            }
        }

        internal void AddValueToBackingICollectionInstance(object collectionInstance, object value)
        {
            this.collectionAdd(collectionInstance, value);
        }

        internal void ClearBackingICollectionInstance(object collectionInstance)
        {
            this.collectionClear(collectionInstance);
        }

        internal object GetValue(object instance)
        {
            return this.propertyGetter(instance);
        }

        internal void RemoveValue(object instance, object value)
        {
            this.collectionRemove(instance, value);
        }

        internal void SetValue(object instance, object value, string propertyName, bool allowAdd)
        {
            if (this.dictionarySetter != null)
            {
                this.dictionarySetter(instance, propertyName, value);
            }
            else if (allowAdd && (this.collectionAdd != null))
            {
                if (!this.collectionContains(instance, value))
                {
                    this.AddValueToBackingICollectionInstance(instance, value);
                }
            }
            else
            {
                if (this.propertySetter == null)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_MissingProperty(value.GetType().ToString(), propertyName));
                }
                this.propertySetter(instance, value);
            }
        }

        internal Type EntityCollectionItemType
        {
            get
            {
                if (!this.IsEntityCollection)
                {
                    return null;
                }
                return this.collectionGenericType;
            }
        }

        internal bool IsDictionary
        {
            get
            {
                return (this.DictionaryValueType != null);
            }
        }

        internal bool IsEntityCollection
        {
            get
            {
                return ((this.collectionGenericType != null) && !this.IsPrimitiveOrComplexCollection);
            }
        }

        internal bool IsPrimitiveOrComplexCollection
        {
            get
            {
                if (!this.isPrimitiveOrComplexCollection.HasValue)
                {
                    if (this.collectionGenericType == null)
                    {
                        this.isPrimitiveOrComplexCollection = false;
                    }
                    else
                    {
                        bool flag = (this.EdmProperty.PropertyKind == EdmPropertyKind.Structural) && (this.EdmProperty.Type.TypeKind() == EdmTypeKind.Collection);
                        if (flag && (this.MaxProtocolVersion <= DataServiceProtocolVersion.V2))
                        {
                            throw new InvalidOperationException(System.Data.Services.Client.Strings.ClientType_CollectionPropertyNotSupportedInV2AndBelow(this.DeclaringClrType.FullName, this.PropertyName));
                        }
                        this.isPrimitiveOrComplexCollection = new bool?(flag);
                    }
                }
                return this.isPrimitiveOrComplexCollection.Value;
            }
        }

        internal bool IsSpatialType
        {
            get
            {
                if (!this.isSpatialType.HasValue)
                {
                    if (typeof(ISpatial).IsAssignableFrom(this.PropertyType))
                    {
                        this.isSpatialType = true;
                    }
                    else
                    {
                        this.isSpatialType = false;
                    }
                }
                return this.isSpatialType.Value;
            }
        }

        internal bool IsStreamLinkProperty
        {
            get
            {
                return (this.PropertyType == typeof(DataServiceStreamLink));
            }
        }

        internal ClientPropertyAnnotation MimeTypeProperty
        {
            get
            {
                return this.mimeTypeProperty;
            }
            set
            {
                this.mimeTypeProperty = value;
            }
        }

        internal Type PrimitiveOrComplexCollectionItemType
        {
            get
            {
                if (this.IsPrimitiveOrComplexCollection)
                {
                    return this.collectionGenericType;
                }
                return null;
            }
        }
    }
}

