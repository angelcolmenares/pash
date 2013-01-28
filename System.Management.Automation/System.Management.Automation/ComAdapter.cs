namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    internal class ComAdapter : Adapter
    {
        private readonly ComTypeInfo _comTypeInfo;

        internal ComAdapter(ComTypeInfo typeinfo)
        {
            this._comTypeInfo = typeinfo;
        }

        internal static string GetComTypeName(string clsid)
        {
            StringBuilder builder = new StringBuilder("System.__ComObject");
            builder.Append("#{");
            builder.Append(clsid);
            builder.Append("}");
            return builder.ToString();
        }

        protected override T GetMember<T>(object obj, string memberName)
        {
            if (this._comTypeInfo.Properties.ContainsKey(memberName))
            {
                ComProperty adapterData = this._comTypeInfo.Properties[memberName];
                if (adapterData.IsParameterized)
                {
                    if (typeof(T).IsAssignableFrom(typeof(PSParameterizedProperty)))
                    {
                        return (new PSParameterizedProperty(adapterData.Name, this, obj, adapterData) as T);
                    }
                }
                else if (typeof(T).IsAssignableFrom(typeof(PSProperty)))
                {
                    return (new PSProperty(adapterData.Name, this, obj, adapterData) as T);
                }
            }
            if ((typeof(T).IsAssignableFrom(typeof(PSMethod)) && (this._comTypeInfo != null)) && this._comTypeInfo.Methods.ContainsKey(memberName))
            {
                ComMethod method = this._comTypeInfo.Methods[memberName];
                PSMethod method2 = new PSMethod(method.Name, this, obj, method);
                return (method2 as T);
            }
            return default(T);
        }

        protected override PSMemberInfoInternalCollection<T> GetMembers<T>(object obj)
        {
            PSMemberInfoInternalCollection<T> internals = new PSMemberInfoInternalCollection<T>();
            bool flag = typeof(T).IsAssignableFrom(typeof(PSProperty));
            bool flag2 = typeof(T).IsAssignableFrom(typeof(PSParameterizedProperty));
            if (flag || flag2)
            {
                foreach (ComProperty property in this._comTypeInfo.Properties.Values)
                {
                    if (property.IsParameterized)
                    {
                        if (flag2)
                        {
                            internals.Add(new PSParameterizedProperty(property.Name, this, obj, property) as T);
                        }
                    }
                    else if (flag)
                    {
                        internals.Add(new PSProperty(property.Name, this, obj, property) as T);
                    }
                }
            }
            if (typeof(T).IsAssignableFrom(typeof(PSMethod)))
            {
                foreach (ComMethod method in this._comTypeInfo.Methods.Values)
                {
                    if (internals[method.Name] == null)
                    {
                        PSMethod method2 = new PSMethod(method.Name, this, obj, method);
                        internals.Add(method2 as T);
                    }
                }
            }
            return internals;
        }

        protected override IEnumerable<string> GetTypeNameHierarchy(object obj)
        {
            yield return GetComTypeName(this._comTypeInfo.Clsid);
            foreach (string iteratorVariable0 in Adapter.GetDotNetTypeNameHierarchy(obj))
            {
                yield return iteratorVariable0;
            }
        }

        protected override Collection<string> MethodDefinitions(PSMethod method)
        {
            ComMethod adapterData = (ComMethod) method.adapterData;
            return adapterData.MethodDefinitions();
        }

        protected override object MethodInvoke(PSMethod method, object[] arguments)
        {
            ComMethod adapterData = (ComMethod) method.adapterData;
            return adapterData.InvokeMethod(method, arguments);
        }

        protected override Collection<string> ParameterizedPropertyDefinitions(PSParameterizedProperty property)
        {
            ComProperty adapterData = (ComProperty) property.adapterData;
            return new Collection<string> { adapterData.GetDefinition() };
        }

        protected override object ParameterizedPropertyGet(PSParameterizedProperty property, object[] arguments)
        {
            ComProperty adapterData = (ComProperty) property.adapterData;
            return adapterData.GetValue(property.baseObject, arguments);
        }

        protected override bool ParameterizedPropertyIsGettable(PSParameterizedProperty property)
        {
            ComProperty adapterData = (ComProperty) property.adapterData;
            return adapterData.IsGettable;
        }

        protected override bool ParameterizedPropertyIsSettable(PSParameterizedProperty property)
        {
            ComProperty adapterData = (ComProperty) property.adapterData;
            return adapterData.IsSettable;
        }

        protected override void ParameterizedPropertySet(PSParameterizedProperty property, object setValue, object[] arguments)
        {
            ((ComProperty) property.adapterData).SetValue(property.baseObject, setValue, arguments);
        }

        protected override string ParameterizedPropertyToString(PSParameterizedProperty property)
        {
            ComProperty adapterData = (ComProperty) property.adapterData;
            return adapterData.ToString();
        }

        protected override string ParameterizedPropertyType(PSParameterizedProperty property)
        {
            ComProperty adapterData = (ComProperty) property.adapterData;
            return adapterData.Type.FullName;
        }

        protected override AttributeCollection PropertyAttributes(PSProperty property)
        {
            return new AttributeCollection(new Attribute[0]);
        }

        protected override object PropertyGet(PSProperty property)
        {
            ComProperty adapterData = (ComProperty) property.adapterData;
            return adapterData.GetValue(property.baseObject);
        }

        protected override bool PropertyIsGettable(PSProperty property)
        {
            ComProperty adapterData = (ComProperty) property.adapterData;
            return adapterData.IsGettable;
        }

        protected override bool PropertyIsSettable(PSProperty property)
        {
            ComProperty adapterData = (ComProperty) property.adapterData;
            return adapterData.IsSettable;
        }

        protected override void PropertySet(PSProperty property, object setValue, bool convertIfPossible)
        {
            ((ComProperty) property.adapterData).SetValue(property.baseObject, setValue);
        }

        protected override string PropertyToString(PSProperty property)
        {
            ComProperty adapterData = (ComProperty) property.adapterData;
            return adapterData.ToString();
        }

        protected override string PropertyType(PSProperty property, bool forDisplay)
        {
            ComProperty adapterData = (ComProperty) property.adapterData;
            if (!forDisplay)
            {
                return adapterData.Type.FullName;
            }
            return ToStringCodeMethods.Type(adapterData.Type, false);
        }

        
    }
}

