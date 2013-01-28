namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Runspaces;

    internal abstract class PropertyOnlyAdapter : DotNetAdapter
    {
        protected PropertyOnlyAdapter()
        {
        }

        protected abstract void DoAddAllProperties<T>(object obj, PSMemberInfoInternalCollection<T> members) where T: PSMemberInfo;
        protected abstract PSProperty DoGetProperty(object obj, string propertyName);
        protected override ConsolidatedString GetInternedTypeNameHierarchy(object obj)
        {
            return new ConsolidatedString(this.GetTypeNameHierarchy(obj), true);
        }

        protected override T GetMember<T>(object obj, string memberName)
        {
            PSProperty property = this.DoGetProperty(obj, memberName);
            if (typeof(T).IsAssignableFrom(typeof(PSProperty)) && (property != null))
            {
                return (property as T);
            }
            if (typeof(T).IsAssignableFrom(typeof(PSMethod)))
            {
                T dotNetMethod = PSObject.dotNetInstanceAdapter.GetDotNetMethod<T>(obj, memberName);
                if ((dotNetMethod != null) && (property == null))
                {
                    return dotNetMethod;
                }
            }
            if (DotNetAdapter.IsTypeParameterizedProperty(typeof(T)))
            {
                PSParameterizedProperty dotNetProperty = PSObject.dotNetInstanceAdapter.GetDotNetProperty<PSParameterizedProperty>(obj, memberName);
                if ((dotNetProperty != null) && (property == null))
                {
                    return (dotNetProperty as T);
                }
            }
            return default(T);
        }

        protected override PSMemberInfoInternalCollection<T> GetMembers<T>(object obj)
        {
            PSMemberInfoInternalCollection<T> members = new PSMemberInfoInternalCollection<T>();
            if (typeof(T).IsAssignableFrom(typeof(PSProperty)))
            {
                this.DoAddAllProperties<T>(obj, members);
            }
            PSObject.dotNetInstanceAdapter.AddAllMethods<T>(obj, members, true);
            if (DotNetAdapter.IsTypeParameterizedProperty(typeof(T)))
            {
                PSMemberInfoInternalCollection<PSParameterizedProperty> internals2 = new PSMemberInfoInternalCollection<PSParameterizedProperty>();
                PSObject.dotNetInstanceAdapter.AddAllProperties<PSParameterizedProperty>(obj, internals2, true);
                foreach (PSParameterizedProperty property in internals2)
                {
                    try
                    {
                        members.Add(property as T);
                    }
                    catch (ExtendedTypeSystemException)
                    {
                    }
                }
            }
            return members;
        }

        internal override bool SiteBinderCanOptimize
        {
            get
            {
                return false;
            }
        }
    }
}

