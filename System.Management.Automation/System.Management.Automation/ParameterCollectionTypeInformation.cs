namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class ParameterCollectionTypeInformation
    {
        internal ParameterCollectionTypeInformation(Type type)
        {
            this.ParameterCollectionType = System.Management.Automation.ParameterCollectionType.NotCollection;
            if (type.IsSubclassOf(typeof(Array)))
            {
                this.ParameterCollectionType = System.Management.Automation.ParameterCollectionType.Array;
                this.ElementType = type.GetElementType();
            }
            else if (!typeof(IDictionary).IsAssignableFrom(type) && (!(from i in type.GetInterfaces()
                where i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                select i).Any<Type>() && (!type.IsGenericType || (type.GetGenericTypeDefinition() != typeof(IDictionary<,>)))))
            {
                bool flag = type.GetInterface(typeof(IList).Name) != null;
                if ((flag && type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Collection<>)))
                {
                    this.ParameterCollectionType = System.Management.Automation.ParameterCollectionType.IList;
                    Type[] genericArguments = type.GetGenericArguments();
                    this.ElementType = genericArguments[0];
                }
                else
                {
                    Type type2 = (from i in type.GetInterfaces()
                        where i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(ICollection<>))
                        select i).FirstOrDefault<Type>();
                    if (type2 != null)
                    {
                        this.ParameterCollectionType = System.Management.Automation.ParameterCollectionType.ICollectionGeneric;
                        Type[] typeArray2 = type2.GetGenericArguments();
                        this.ElementType = typeArray2[0];
                    }
                    else if (flag)
                    {
                        this.ParameterCollectionType = System.Management.Automation.ParameterCollectionType.IList;
                    }
                }
            }
        }

        internal Type ElementType { get; set; }

        internal System.Management.Automation.ParameterCollectionType ParameterCollectionType { get; private set; }
    }
}

