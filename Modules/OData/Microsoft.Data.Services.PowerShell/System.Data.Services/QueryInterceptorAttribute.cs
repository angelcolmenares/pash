namespace System.Data.Services
{
    using System;
    using System.Diagnostics;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited=true)]
    internal sealed class QueryInterceptorAttribute : Attribute
    {
        private readonly string entitySetName;

        public QueryInterceptorAttribute(string entitySetName)
        {
            this.entitySetName = WebUtil.CheckArgumentNull<string>(entitySetName, "entitySetName");
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

