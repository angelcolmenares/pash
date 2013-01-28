namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    internal sealed class IgnorePropertiesAttribute : Attribute
    {
        private readonly ReadOnlyCollection<string> propertyNames;

        public IgnorePropertiesAttribute(string propertyName)
        {
            WebUtil.CheckArgumentNull<string>(propertyName, "propertyName");
            this.propertyNames = new ReadOnlyCollection<string>(new List<string>(new string[] { propertyName }));
        }

        public IgnorePropertiesAttribute(params string[] propertyNames)
        {
            WebUtil.CheckArgumentNull<string[]>(propertyNames, "propertyNames");
            if (propertyNames.Length == 0)
            {
                throw new ArgumentException(Strings.ETagAttribute_MustSpecifyAtleastOnePropertyName, "propertyNames");
            }
            this.propertyNames = new ReadOnlyCollection<string>(new List<string>(propertyNames));
        }

        internal static IEnumerable<string> GetProperties(Type type, bool inherit, BindingFlags bindingFlags)
        {
            IgnorePropertiesAttribute[] customAttributes = (IgnorePropertiesAttribute[]) type.GetCustomAttributes(typeof(IgnorePropertiesAttribute), inherit);
            if (customAttributes.Length != 1)
            {
                return WebUtil.EmptyStringArray;
            }
            foreach (string str in customAttributes[0].PropertyNames)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new InvalidOperationException(Strings.IgnorePropertiesAttribute_PropertyNameCannotBeNullOrEmpty);
                }
                if (type.GetProperty(str, bindingFlags) == null)
                {
                    throw new InvalidOperationException(Strings.IgnorePropertiesAttribute_InvalidPropertyName(str, type.FullName));
                }
            }
            return customAttributes[0].PropertyNames;
        }

        public ReadOnlyCollection<string> PropertyNames
        {
            get
            {
                return this.propertyNames;
            }
        }
    }
}

