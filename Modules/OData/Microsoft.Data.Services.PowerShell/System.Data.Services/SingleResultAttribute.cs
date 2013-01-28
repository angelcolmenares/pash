namespace System.Data.Services
{
    using System;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=true)]
    internal sealed class SingleResultAttribute : Attribute
    {
        internal static bool MethodHasSingleResult(MethodInfo method)
        {
            return (method.GetCustomAttributes(typeof(SingleResultAttribute), true).Length > 0);
        }
    }
}

