namespace System.Management.Automation
{
    using System;
    using System.ComponentModel;
    using System.Threading;

    public class PSObjectTypeDescriptionProvider : TypeDescriptionProvider
    {
        public event EventHandler<GettingValueExceptionEventArgs> GettingValueException;

        public event EventHandler<SettingValueExceptionEventArgs> SettingValueException;

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            PSObject obj2 = instance as PSObject;
            PSObjectTypeDescriptor descriptor = new PSObjectTypeDescriptor(obj2);
            descriptor.SettingValueException += this.SettingValueException;
            descriptor.GettingValueException += this.GettingValueException;
            return descriptor;
        }
    }
}

