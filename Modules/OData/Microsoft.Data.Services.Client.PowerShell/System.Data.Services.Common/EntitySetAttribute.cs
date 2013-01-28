namespace System.Data.Services.Common
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    internal sealed class EntitySetAttribute : Attribute
    {
        private readonly string entitySet;

        public EntitySetAttribute(string entitySet)
        {
            this.entitySet = entitySet;
        }

        public string EntitySet
        {
            get
            {
                return this.entitySet;
            }
        }
    }
}

