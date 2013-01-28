namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections;
    using System.DirectoryServices;
	using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class DirectoryEntryAdapter : DotNetAdapter
    {
        private static readonly DotNetAdapter dotNetAdapter = new DotNetAdapter();

        protected override T GetMember<T>(object obj, string memberName)
        {
            PSProperty property;
            DirectoryEntry entry = (DirectoryEntry) obj;
            PropertyValueCollection values = entry.Properties[memberName];
            object adapterData = values;
            try
            {
                object obj3 = entry.InvokeGet(memberName);
                if ((values == null) || ((values.Value == null) && (obj3 != null)))
                {
                    adapterData = obj3;
                }
				//values.PropertyName
                property = new PSProperty(memberName, this, obj, adapterData);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                property = null;
            }
            if (adapterData == null)
            {
                property = null;
            }
            if (typeof(T).IsAssignableFrom(typeof(PSProperty)) && (property != null))
            {
                return (property as T);
            }
            if ((typeof(T).IsAssignableFrom(typeof(PSMethod)) && (property == null)) && (base.GetDotNetProperty<T>(obj, memberName) == null))
            {
                return (new PSMethod(memberName, this, obj, null) as T);
            }
            return default(T);
        }

        protected override PSMemberInfoInternalCollection<T> GetMembers<T>(object obj)
        {
            DirectoryEntry entry = (DirectoryEntry) obj;
            PSMemberInfoInternalCollection<T> internals = new PSMemberInfoInternalCollection<T>();
            if ((entry.Properties == null) || (entry.Properties.PropertyNames == null))
            {
                return null;
            }
            int count = 0;
            try
            {
                count = entry.Properties.PropertyNames.Count;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            if (count > 0)
            {
				int i = 0;
				string[] pArray = entry.Properties.PropertyNames.OfType<string>().ToArray();
                foreach (PropertyValueCollection values in entry.Properties)
                {
					string propertyName = pArray[i];
                    internals.Add(new PSProperty(propertyName, this, obj, values) as T);
					i++;
				}
            }
            return internals;
        }

        protected override object MethodInvoke(PSMethod method, object[] arguments)
        {
            object[] objArray;
            Exception exception = null;
            ParameterInformation[] informationArray = new ParameterInformation[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                informationArray[i] = new ParameterInformation(typeof(object), false, null, false);
            }
            MethodInformation[] methods = new MethodInformation[] { new MethodInformation(false, false, informationArray) };
            Adapter.GetBestMethodAndArguments(method.Name, methods, arguments, out objArray);
            DirectoryEntry baseObject = (DirectoryEntry) method.baseObject;
            try
            {
                return baseObject.Invoke(method.Name, objArray);
            }
#if !MONO
            catch (DirectoryServicesCOMException exception2)
            {
                exception = exception2;
            }
#endif
            catch (TargetInvocationException exception3)
            {
                exception = exception3;
            }
            catch (COMException exception4)
            {
                exception = exception4;
            }
            PSMethod dotNetMethod = dotNetAdapter.GetDotNetMethod<PSMethod>(method.baseObject, method.name);
            if (dotNetMethod == null)
            {
                throw exception;
            }
            return dotNetMethod.Invoke(arguments);
        }

        protected override object MethodInvoke(PSMethod method, PSMethodInvocationConstraints invocationConstraints, object[] arguments)
        {
            return this.MethodInvoke(method, arguments);
        }

        protected override string MethodToString(PSMethod method)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str in this.MethodDefinitions(method))
            {
                builder.Append(str);
                builder.Append(", ");
            }
            builder.Remove(builder.Length - 2, 2);
            return builder.ToString();
        }

        protected override object PropertyGet(PSProperty property)
        {
            return property.adapterData;
        }

        protected override bool PropertyIsGettable(PSProperty property)
        {
            return true;
        }

        protected override bool PropertyIsSettable(PSProperty property)
        {
            return true;
        }

        protected override void PropertySet(PSProperty property, object setValue, bool convertIfPossible)
        {
            PropertyValueCollection adapterData = property.adapterData as PropertyValueCollection;
            if (adapterData != null)
            {
                try
                {
                    adapterData.Clear();
                }
                catch (COMException exception)
                {
                    if ((exception.ErrorCode != -2147467259) || (setValue == null))
                    {
                        throw;
                    }
                }
                IEnumerable enumerable = LanguagePrimitives.GetEnumerable(setValue);
                if (enumerable == null)
                {
                    adapterData.Add(setValue);
                }
                else
                {
                    foreach (object obj2 in enumerable)
                    {
                        adapterData.Add(obj2);
                    }
                }
            }
            else
            {
                DirectoryEntry baseObject = (DirectoryEntry) property.baseObject;
                ArrayList list = new ArrayList();
                IEnumerable enumerable2 = LanguagePrimitives.GetEnumerable(setValue);
                if (enumerable2 == null)
                {
                    list.Add(setValue);
                }
                else
                {
                    foreach (object obj3 in enumerable2)
                    {
                        list.Add(obj3);
                    }
                }
                baseObject.InvokeSet(property.name, list.ToArray());
            }
        }

        protected override string PropertyType(PSProperty property, bool forDisplay)
        {
            object obj2 = null;
            try
            {
                obj2 = base.BasePropertyGet(property);
            }
            catch (GetValueException)
            {
            }
            Type type = (obj2 == null) ? typeof(object) : obj2.GetType();
            if (!forDisplay)
            {
                return type.FullName;
            }
            return ToStringCodeMethods.Type(type, false);
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

