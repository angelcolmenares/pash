namespace System.Management.Automation
{
    using System;

    internal class BaseDotNetAdapterForAdaptedObjects : DotNetAdapter
    {
        protected override T GetMember<T>(object obj, string memberName)
        {
            PSProperty dotNetProperty = base.GetDotNetProperty<PSProperty>(obj, memberName);
            if (typeof(T).IsAssignableFrom(typeof(PSProperty)) && (dotNetProperty != null))
            {
                return (dotNetProperty as T);
            }
            if (typeof(T).Equals(typeof(PSMemberInfo)))
            {
                T dotNetMethod = PSObject.dotNetInstanceAdapter.GetDotNetMethod<T>(obj, memberName);
                if ((dotNetMethod != null) && (dotNetProperty == null))
                {
                    return dotNetMethod;
                }
            }
            if (DotNetAdapter.IsTypeParameterizedProperty(typeof(T)))
            {
                PSParameterizedProperty property2 = PSObject.dotNetInstanceAdapter.GetDotNetProperty<PSParameterizedProperty>(obj, memberName);
                if ((property2 != null) && (dotNetProperty == null))
                {
                    return (property2 as T);
                }
            }
            return default(T);
        }

        protected override PSMemberInfoInternalCollection<T> GetMembers<T>(object obj)
        {
            PSMemberInfoInternalCollection<T> members = new PSMemberInfoInternalCollection<T>();
            base.AddAllProperties<T>(obj, members, true);
            base.AddAllMethods<T>(obj, members, true);
            base.AddAllEvents<T>(obj, members, true);
            return members;
        }
    }
}

