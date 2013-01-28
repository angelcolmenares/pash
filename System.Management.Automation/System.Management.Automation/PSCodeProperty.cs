namespace System.Management.Automation
{
    using System;
    using System.Reflection;
    using System.Text;

    public class PSCodeProperty : PSPropertyInfo
    {
        private MethodInfo getterCodeReference;
        private MethodInfo setterCodeReference;

        internal PSCodeProperty(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
        }

        public PSCodeProperty(string name, MethodInfo getterCodeReference)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            if (getterCodeReference == null)
            {
                throw PSTraceSource.NewArgumentNullException("getterCodeReference");
            }
            this.SetGetter(getterCodeReference);
        }

        public PSCodeProperty(string name, MethodInfo getterCodeReference, MethodInfo setterCodeReference)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            if ((getterCodeReference == null) && (setterCodeReference == null))
            {
                throw PSTraceSource.NewArgumentNullException("getterCodeReference setterCodeReference");
            }
            this.SetGetter(getterCodeReference);
            this.SetSetter(setterCodeReference, getterCodeReference);
        }

        public override PSMemberInfo Copy()
        {
            PSCodeProperty destiny = new PSCodeProperty(base.name, this.getterCodeReference, this.setterCodeReference);
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        internal void SetGetter(MethodInfo methodForGet)
        {
            if (methodForGet == null)
            {
                this.getterCodeReference = null;
            }
            else
            {
                ParameterInfo[] parameters = methodForGet.GetParameters();
                if ((!methodForGet.IsPublic || !methodForGet.IsStatic) || ((methodForGet.ReturnType.Equals(typeof(void)) || (parameters.Length != 1)) || !parameters[0].ParameterType.Equals(typeof(PSObject))))
                {
                    throw new ExtendedTypeSystemException("GetterFormat", null, ExtendedTypeSystem.CodePropertyGetterFormat, new object[0]);
                }
                this.getterCodeReference = methodForGet;
            }
        }

        internal void SetGetterFromTypeTable(Type type, string methodName)
        {
            MemberInfo[] infoArray = type.GetMember(methodName, MemberTypes.Method, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (infoArray.Length != 1)
            {
                throw new ExtendedTypeSystemException("GetterFormatFromTypeTable", null, ExtendedTypeSystem.CodePropertyGetterFormat, new object[0]);
            }
            this.SetGetter((MethodInfo) infoArray[0]);
        }

        private void SetSetter(MethodInfo methodForSet, MethodInfo methodForGet)
        {
            if (methodForSet == null)
            {
                if (methodForGet == null)
                {
                    throw new ExtendedTypeSystemException("SetterAndGetterNullFormat", null, ExtendedTypeSystem.CodePropertyGetterAndSetterNull, new object[0]);
                }
                this.setterCodeReference = null;
            }
            else
            {
                ParameterInfo[] parameters = methodForSet.GetParameters();
                if (((!methodForSet.IsPublic || !methodForSet.IsStatic) || (!methodForSet.ReturnType.Equals(typeof(void)) || (parameters.Length != 2))) || (!parameters[0].ParameterType.Equals(typeof(PSObject)) || ((methodForGet != null) && !methodForGet.ReturnType.Equals(parameters[1].ParameterType))))
                {
                    throw new ExtendedTypeSystemException("SetterFormat", null, ExtendedTypeSystem.CodePropertySetterFormat, new object[0]);
                }
                this.setterCodeReference = methodForSet;
            }
        }

        internal void SetSetterFromTypeTable(Type type, string methodName)
        {
            MemberInfo[] infoArray = type.GetMember(methodName, MemberTypes.Method, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (infoArray.Length != 1)
            {
                throw new ExtendedTypeSystemException("SetterFormatFromTypeTable", null, ExtendedTypeSystem.CodePropertySetterFormat, new object[0]);
            }
            this.SetSetter((MethodInfo) infoArray[0], this.getterCodeReference);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.TypeNameOfValue);
            builder.Append(" ");
            builder.Append(base.Name);
            builder.Append("{");
            if (this.IsGettable)
            {
                builder.Append("get=");
                builder.Append(this.getterCodeReference.Name);
                builder.Append(";");
            }
            if (this.IsSettable)
            {
                builder.Append("set=");
                builder.Append(this.setterCodeReference.Name);
                builder.Append(";");
            }
            builder.Append("}");
            return builder.ToString();
        }

        public MethodInfo GetterCodeReference
        {
            get
            {
                return this.getterCodeReference;
            }
        }

        public override bool IsGettable
        {
            get
            {
                return (this.getterCodeReference != null);
            }
        }

        public override bool IsSettable
        {
            get
            {
                return (this.SetterCodeReference != null);
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.CodeProperty;
            }
        }

        public MethodInfo SetterCodeReference
        {
            get
            {
                return this.setterCodeReference;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                if (this.getterCodeReference == null)
                {
                    throw new GetValueException("GetWithoutGetterFromCodePropertyTypeOfValue", null, ExtendedTypeSystem.GetWithoutGetterException, new object[] { base.Name });
                }
                return this.getterCodeReference.ReturnType.FullName;
            }
        }

        public override object Value
        {
            get
            {
                object obj2;
                if (this.getterCodeReference == null)
                {
                    throw new GetValueException("GetWithoutGetterFromCodePropertyValue", null, ExtendedTypeSystem.GetWithoutGetterException, new object[] { base.Name });
                }
                try
                {
                    obj2 = this.getterCodeReference.Invoke(null, new object[] { base.instance });
                }
                catch (TargetInvocationException exception)
                {
                    Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                    throw new GetValueInvocationException("CatchFromCodePropertyGetTI", innerException, ExtendedTypeSystem.ExceptionWhenGetting, new object[] { base.name, innerException.Message });
                }
                catch (Exception exception3)
                {
                    if (exception3 is GetValueException)
                    {
                        throw;
                    }
                    CommandProcessorBase.CheckForSevereException(exception3);
                    throw new GetValueInvocationException("CatchFromCodePropertyGet", exception3, ExtendedTypeSystem.ExceptionWhenGetting, new object[] { base.name, exception3.Message });
                }
                return obj2;
            }
            set
            {
                if (this.setterCodeReference == null)
                {
                    throw new SetValueException("SetWithoutSetterFromCodeProperty", null, ExtendedTypeSystem.SetWithoutSetterException, new object[] { base.Name });
                }
                try
                {
                    this.setterCodeReference.Invoke(null, new object[] { base.instance, value });
                }
                catch (TargetInvocationException exception)
                {
                    Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                    throw new SetValueInvocationException("CatchFromCodePropertySetTI", innerException, ExtendedTypeSystem.ExceptionWhenSetting, new object[] { base.name, innerException.Message });
                }
                catch (Exception exception3)
                {
                    if (exception3 is SetValueException)
                    {
                        throw;
                    }
                    CommandProcessorBase.CheckForSevereException(exception3);
                    throw new SetValueInvocationException("CatchFromCodePropertySet", exception3, ExtendedTypeSystem.ExceptionWhenSetting, new object[] { base.name, exception3.Message });
                }
            }
        }
    }
}

