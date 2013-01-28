namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    internal sealed class ETagAttribute : Attribute
    {
        private readonly ReadOnlyCollection<string> propertyNames;

        public ETagAttribute(string propertyName)
        {
            WebUtil.CheckArgumentNull<string>(propertyName, "propertyName");
            this.propertyNames = new ReadOnlyCollection<string>(new List<string>(new string[] { propertyName }));
        }

        public ETagAttribute(params string[] propertyNames)
        {
            WebUtil.CheckArgumentNull<string[]>(propertyNames, "propertyNames");
            if (propertyNames.Length == 0)
            {
                throw new ArgumentException(Strings.ETagAttribute_MustSpecifyAtleastOnePropertyName, "propertyNames");
            }
            this.propertyNames = new ReadOnlyCollection<string>(new List<string>(propertyNames));
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

