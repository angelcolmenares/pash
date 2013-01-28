namespace System.Management.Automation
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class PSObjectPropertyDescriptor : PropertyDescriptor
    {
        private bool _isReadOnly;
        private AttributeCollection _propertyAttributes;
        private Type _propertyType;
        internal const string InvalidComponentMsg = "InvalidComponent";

        internal event EventHandler<GettingValueExceptionEventArgs> GettingValueException;

        internal event EventHandler<SettingValueExceptionEventArgs> SettingValueException;

        internal PSObjectPropertyDescriptor(string propertyName, Type propertyType, bool isReadOnly, AttributeCollection propertyAttributes) : base(propertyName, new Attribute[0])
        {
            this._isReadOnly = isReadOnly;
            this._propertyAttributes = propertyAttributes;
            this._propertyType = propertyType;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        private object DealWithGetValueException(ExtendedTypeSystemException e, out bool shouldThrow)
        {
            GettingValueExceptionEventArgs eventArgs = new GettingValueExceptionEventArgs(e);
            if (this.GettingValueException != null)
            {
                this.GettingValueException.SafeInvoke<GettingValueExceptionEventArgs>(this, eventArgs);
                PSObjectTypeDescriptor.typeDescriptor.WriteLine("GettingValueException event has been triggered resulting in ValueReplacement:\"{0}\".", new object[] { eventArgs.ValueReplacement });
            }
            shouldThrow = eventArgs.ShouldThrow;
            return eventArgs.ValueReplacement;
        }

        private void DealWithSetValueException(ExtendedTypeSystemException e, out bool shouldThrow)
        {
            SettingValueExceptionEventArgs eventArgs = new SettingValueExceptionEventArgs(e);
            if (this.SettingValueException != null)
            {
                this.SettingValueException.SafeInvoke<SettingValueExceptionEventArgs>(this, eventArgs);
                PSObjectTypeDescriptor.typeDescriptor.WriteLine("SettingValueException event has been triggered resulting in ShouldThrow:\"{0}\".", new object[] { eventArgs.ShouldThrow });
            }
            shouldThrow = eventArgs.ShouldThrow;
        }

        private static PSObject GetComponentPSObject(object component)
        {
            PSObject obj2 = component as PSObject;
            if (obj2 != null)
            {
                return obj2;
            }
            PSObjectTypeDescriptor descriptor = component as PSObjectTypeDescriptor;
            if (descriptor == null)
            {
                throw PSTraceSource.NewArgumentException("component", "ExtendedTypeSystem", "InvalidComponent", new object[] { "component", typeof(PSObject).Name, typeof(PSObjectTypeDescriptor).Name });
            }
            return descriptor.Instance;
        }

        public override object GetValue(object component)
        {
            if (component == null)
            {
                throw PSTraceSource.NewArgumentNullException("component");
            }
            PSObject componentPSObject = GetComponentPSObject(component);
            try
            {
                PSPropertyInfo info = componentPSObject.Properties[this.Name];
                if (info == null)
                {
                    bool flag;
                    PSObjectTypeDescriptor.typeDescriptor.WriteLine("Could not find property \"{0}\" to get its value.", new object[] { this.Name });
                    ExtendedTypeSystemException e = new ExtendedTypeSystemException("PropertyNotFoundInPropertyDescriptorGetValue", null, ExtendedTypeSystem.PropertyNotFoundInTypeDescriptor, new object[] { this.Name });
                    object obj3 = this.DealWithGetValueException(e, out flag);
                    if (flag)
                    {
                        throw e;
                    }
                    return obj3;
                }
                return info.Value;
            }
            catch (ExtendedTypeSystemException exception2)
            {
                bool flag2;
                PSObjectTypeDescriptor.typeDescriptor.WriteLine("Exception getting the value of the property \"{0}\": \"{1}\".", new object[] { this.Name, exception2.Message });
                object obj4 = this.DealWithGetValueException(exception2, out flag2);
                if (flag2)
                {
                    throw;
                }
                return obj4;
            }
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
            if (component == null)
            {
                throw PSTraceSource.NewArgumentNullException("component");
            }
            PSObject componentPSObject = GetComponentPSObject(component);
            try
            {
                PSPropertyInfo info = componentPSObject.Properties[this.Name];
                if (info == null)
                {
                    bool flag;
                    PSObjectTypeDescriptor.typeDescriptor.WriteLine("Could not find property \"{0}\" to set its value.", new object[] { this.Name });
                    ExtendedTypeSystemException e = new ExtendedTypeSystemException("PropertyNotFoundInPropertyDescriptorSetValue", null, ExtendedTypeSystem.PropertyNotFoundInTypeDescriptor, new object[] { this.Name });
                    this.DealWithSetValueException(e, out flag);
                    if (flag)
                    {
                        throw e;
                    }
                    return;
                }
                info.Value = value;
            }
            catch (ExtendedTypeSystemException exception2)
            {
                bool flag2;
                PSObjectTypeDescriptor.typeDescriptor.WriteLine("Exception setting the value of the property \"{0}\": \"{1}\".", new object[] { this.Name, exception2.Message });
                this.DealWithSetValueException(exception2, out flag2);
                if (flag2)
                {
                    throw;
                }
            }
            this.OnValueChanged(component, EventArgs.Empty);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                return this._propertyAttributes;
            }
        }

        public override Type ComponentType
        {
            get
            {
                return typeof(PSObject);
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this._isReadOnly;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this._propertyType;
            }
        }
    }
}

