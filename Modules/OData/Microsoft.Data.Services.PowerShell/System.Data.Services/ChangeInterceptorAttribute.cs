namespace System.Data.Services
{
    using System;
    using System.Diagnostics;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited=true)]
    internal sealed class ChangeInterceptorAttribute : Attribute
    {
        private readonly string entitySetName;

        public ChangeInterceptorAttribute(string entitySetName)
        {
            if (entitySetName == null)
            {
                throw Error.ArgumentNull("entitySetName");
            }
            this.entitySetName = entitySetName;
        }

        public string EntitySetName
        {
            [DebuggerStepThrough]
            get
            {
                return this.entitySetName;
            }
        }
    }
}

